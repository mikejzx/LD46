/*
 * CellManager.cs
 *
 * Manages cells in the game.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour
{
	// Singleton
	public static CellManager Inst;

	// Serialised members.
	[Header("Objects")]
	[SerializeField] private Transform  m_CellsParent = null;
	[SerializeField] private GameObject m_CellPrefab  = null;
	[SerializeField] private GameObject m_PowerupHealthPrefab = null;
	[SerializeField] private GameObject m_PowerupSpeedPrefab  = null;
	[Header("Paths. (G,C,O,P)")]
	[SerializeField] private CellPath   m_PathTop    = null;
	[SerializeField] private CellPath[] m_PathsTyped = null;
	[SerializeField] private CellPath   m_PathPlayerStorage = null;

	// List of colours of each cell.
	public static Dictionary<CellType, Color> CellColours;

	// Speed of cells.
	public static float CellSpeed { get => Inst.m_CellSpeed; }

	// Private members.
	private float m_Timer = 5.0f;
	private float m_TimerTarget = 5.5f;
	private float m_TimerTargetCurrent = 0.0f;
	private float m_CellSpeed = 3.5f;
	private int   m_SpawnCounter = 0;

	// Constants.
	private const float TIMER_RANDOMNESS = 1.0f;
	private const float TIMER_DECREMENT_SPEED = 0.07f;
	private const float TIMER_MIN = 1.1f;
	private const float CELL_SPEED_BEGIN = 1.8f;
	private const float CELL_SPEED_INCREMENT = 0.2f;
	private const int   POWERUP_SPAWN_FREQ = 10; // 15;

	/*
	 * Called before Start.
	 */
	private void Awake() => Inst = this;

	/*
	 * Use this for initialisation.
	 */
	private void Start()
	{
		// Initialise colours of each cell type.
		CellColours = new Dictionary<CellType, Color>();
		CellColours[CellType.Green]  = new Color(0.0f, 1.0f, 0.0f);
		CellColours[CellType.Cyan]   = new Color(0.0f, 1.0f, 1.0f);
		CellColours[CellType.Orange] = new Color(1.0f, 0.5f, 0.0f);
		CellColours[CellType.Pink]   = new Color(1.0f, 0.0f, 1.0f);

		// Set time target to the accurate version.
		m_TimerTargetCurrent = m_TimerTarget;

		m_CellSpeed = CELL_SPEED_BEGIN;
	}

	/*
	 * Called each frame.
	 */
	private void Update()
	{
		// If not yet started.
		if (!GameManager.GameStarted)
		{
			return;
		}

		// Increment timer.
		m_Timer += Time.deltaTime;

		// Decrement timer target over time.
		if (m_TimerTarget > TIMER_MIN)
		{
			m_TimerTarget -= Time.deltaTime * TIMER_DECREMENT_SPEED;
		}

		// Spawn if our timer passes target.
		if (m_Timer > m_TimerTargetCurrent)
		{
			// Reset timer.
			m_Timer = 0.0f;

			// Our prefab
			GameObject prefab = m_CellPrefab;

			// Give powerups every now and then.
			Powerup powup = Powerup.Not;
			if (++m_SpawnCounter > POWERUP_SPAWN_FREQ)
			{
				// Randomly choose health or speed.
				if (Random.Range(0, 2) == 0)
				{
					powup = Powerup.Speed;
					prefab = m_PowerupSpeedPrefab;
				}
				else
				{
					powup = Powerup.Health;
					prefab = m_PowerupHealthPrefab;
				}
				m_SpawnCounter = 0;
			}

			// Spawn. TODO: Object pool.
			GameObject cobj = GameObject.Instantiate(prefab, m_CellsParent);
			Cell c = cobj.GetComponent<Cell>();

			// Initialise cell.
			c.Initialise(powup);

			// Set next target.
			m_TimerTargetCurrent = Random.Range(m_TimerTarget - TIMER_RANDOMNESS, m_TimerTarget + TIMER_RANDOMNESS);
			if (m_TimerTargetCurrent < 0.2f)
			{
				m_TimerTargetCurrent = 0.2f;
			}

			// Increment cell movement speed.
			m_CellSpeed += CELL_SPEED_INCREMENT;
		}
	}

	/*
	 * Get the path for a cell with state and type.
	 */
	public static CellPath GetPath(CellState s, CellType t)
	{
		// Dead cells have no path.
		if (s == CellState.Died)
		{
			return null;
		}

		// Stored are a bit of a special case. We give them the
		// path but they do not aim for the end of path,
		// they aim for start of the "queue".
		if (s == CellState.Stored)
		{
			return Inst.m_PathPlayerStorage;
		}

		// Return the top path if not in a tube.
		if (s == CellState.JustSpawned)
		{
			return Inst.m_PathTop;
		}

		// We are in a tube, return the path for the colour.
		return Inst.m_PathsTyped[(int)t];
	}
}
