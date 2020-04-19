/*
 * Player.cs
 *
 * Manages the player object.
 * I guess we call this thing a "vessel"?
 */

using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
	// Singleton.
	public static Player Inst;

	// Serialised members.
	[SerializeField] private Vector2[] m_TubePositions = null;

	// Player status.
	private float m_BPM;
	public static float BPM {
		get => Inst.m_BPM;
		set {
			GUIManager.SetBPM(value);
			Inst.m_BPM = value;
			ScoreManager.CheckBPM(Mathf.RoundToInt(value));
		}
	}

	// Privates
	private RectTransform m_Trans;
	private int    m_CurTube = 0;
	private bool   m_SwitchingTubes = false;
	private float  m_SwitchProgress = 0.0f;
	private Cell[] m_StoredCells;
	private int    m_StoredCellCount = 0;
	private float  m_ReleaseCooldownTimer = 0.0f;

	// Powerups.
	private bool _m_SpeedPowerupEnabled = false;
	public bool SpeedPowerupEnabled {
		get => _m_SpeedPowerupEnabled;
		private set {
			GUIManager.PowerupTextEnabled(value);
			_m_SpeedPowerupEnabled = value;
		}
	}
	private float _m_SpeedPowerupTimer = 0.0f;
	public float SpeedPowerupTimer {
		get => _m_SpeedPowerupTimer;
		set {
			GUIManager.PowerupTime(POWERUP_SPEED_LENGTH - value);
			_m_SpeedPowerupTimer = value;
		}
	}

	// Constants.
	private const float TUBE_SWITCH_MARGIN = 0.8f;
	public  const float HEALTH_DROP_CELL_DIE = 10.0f;
	public  const float HEALTH_DROP_CELL_WRONG = 20.0f;
	private const float MOVE_SPEED = 0.38f;
	private const float MOVE_SPEED_POWERED_UP = 0.2f;
	private const int   MAX_CELL_STORAGE = 3;
	private const int   CELL_STORAGE_PT_END = 3;
	private const int   NUM_OF_TUBES = 4;
	private const float START_BPM = 60.0f;
	private const float MAX_BPM   = 165.0f;
	private const float POWERUP_HEALTH_ADD = 27.0f;
	private const float POWERUP_SPEED_LENGTH = 15.0f;
	private const float RELEASE_COOLDOWN_TIME = 0.45f;
	private static readonly float[] CELL_SCORES = {
		3.0f, 2.0f, 1.0f, 0.5f
	};

	/*
	 * Called before Start.
	 */
	private void Awake() => Inst = this;

	/*
	 * Use this for initialisation.
	 */
	private void Start()
	{
		m_Trans = GetComponent<RectTransform>();
		m_CurTube = 0;
		m_StoredCells = new Cell[MAX_CELL_STORAGE];
		m_StoredCellCount = 0;
		BPM = START_BPM;
	}

	/*
	 * Called each frame.
	 */
	private void Update()
	{
		// Handle powerups.
		if (SpeedPowerupEnabled)
		{
			SpeedPowerupTimer += Time.deltaTime;
			if (SpeedPowerupTimer > POWERUP_SPEED_LENGTH)
			{
				SpeedPowerupTimer = 0.0f;
				SpeedPowerupEnabled = false;
			}
		}

		// Get inputs.
		HandleInput();
	}

	/*
	 * Deducts BPM from player.
	 *
	 * @param amount  The amount to deduct.
	 */
	public static void DeductBPM(float amount)
	{
		// Subtract. Lose if lower than 0.
		float newBPM = BPM - amount;
		if (newBPM < 1.0f)
		{
			newBPM = 0.0f;
			GameManager.Lose();
		}
		BPM = newBPM;
	}

	/*
	 * Adds BPM to player.
	 *
	 * The amount to add is computing, depends on
	 * what the current BPM is is.
	 *
	 * @param t  The type of cell that made it.
	 */
	public static void AddBPM(CellType t)
	{
		// Probably a much better way to do this...
		float x = CELL_SCORES[(int)t];
		if (BPM > 120)
		{
			x *= 0.5f;
		}
		else if (BPM > 100)
		{
			x *= 1.0f;
		}
		else if (BPM > 80)
		{
			x *= 1.2f;
		}
		else if (BPM > 50)
		{
			x *= 1.5f;
		}
		BPM += x;
		//Debug.Log(x);
	}

	/*
	 * Adds a cell to the player storage.
	 *
	 * @param c  The cell to add.
	 *
	 * @return true if cell was added. We can't add if storage is full.
	 */
	public bool AddCellToStorage(Cell c)
	{
		// Return if we aren't at the first tube!
		// Or if we are already full.
		// Or if we are currently switching tubes.
		if (m_CurTube != 0 ||
			m_StoredCellCount >= MAX_CELL_STORAGE ||
			m_SwitchingTubes)
		{
			// Kill the cell.
			c.SetState(CellState.Died);
			return false;
		}

		// Place the cell in storage.
		c.SetStorageIndex(m_StoredCellCount);
		m_StoredCells[m_StoredCellCount++] = c;
		c.SetState(CellState.Stored);

		// If we are getting our first cell.
		if (GameManager.FirstReleased == -1)
		{
			GameManager.FirstReleased = 0;
			GUIManager.HelpTextShowRelease();
		}

		// Added, return true.
		return true;
	}

	/*
	 * Get the target point for a cell in storage.
	 *
	 * @param idx  The storage index of the point.
	 */
	public static int GetStorageTargetPoint(int idx)
	{
		return CELL_STORAGE_PT_END - idx;
	}

	/*
	 * Handles the inputs.
	 */
	private void HandleInput()
	{
		// Increment cooldown timer.
		if (m_ReleaseCooldownTimer < RELEASE_COOLDOWN_TIME)
		{
			m_ReleaseCooldownTimer += Time.deltaTime;
		}

		// Allow a little bit of error if we are dropping.
		if (m_SwitchingTubes && m_SwitchProgress < TUBE_SWITCH_MARGIN)
		{
			return;
		}

		// Movement input is locked when switching.
		if (!m_SwitchingTubes)
		{
			// Check for left input.
			int switchDir = 0;
			if (Input.GetKey(KeyCode.LeftArrow) ||
				Input.GetKey(KeyCode.A) ||
				Input.GetKey(KeyCode.H))
			{
				switchDir = 1;
			}

			// Check for right input.
			if (Input.GetKey(KeyCode.RightArrow) ||
				Input.GetKey(KeyCode.D) ||
				Input.GetKey(KeyCode.L))
			{
				switchDir = -1;
			}

			// If we are switching tubes, start the coroutine.
			if (switchDir != 0)
			{
				StartCoroutine(DoSwitch(switchDir));

				// If game not yet started, start it.
				if (!GameManager.GameStarted)
				{
					GameManager.InputMade();
				}
			}
		}

		// Check for drop press.
		// Don't need to KeyDown anymore because of new "wait until drop".
		if (Input.GetKey(KeyCode.Space) ||
			Input.GetKey(KeyCode.J) ||
			Input.GetKey(KeyCode.S) ||
			Input.GetKey(KeyCode.DownArrow) ||
			Input.GetKey(KeyCode.Return))
		{
			// We need to be cooled down.
			if (m_ReleaseCooldownTimer < RELEASE_COOLDOWN_TIME)
			{
				return;
			}

			// Can't drop if we don't have any cells.
			if (m_StoredCellCount == 0)
			{
				return;
			}

			// New rule: Cannot drop unless the cell has
			// reached the end.
			if (!m_StoredCells[0].StoredCell_AtTargetPt)
			{
				return;
			}

			// Drop the bottom cell into the tube we're above.
			m_StoredCells[0].SetTube(m_CurTube);
			m_StoredCells[0].SetState(CellState.Ended);

			// Shift places of the old cells.
			for (int i = 0; i < m_StoredCellCount - 1; ++i)
			{
				m_StoredCells[i] = m_StoredCells[i + 1];
			}

			// Decrement stored cell count.
			m_StoredCells[--m_StoredCellCount] = null;

			for (int i = 0; i < m_StoredCellCount; ++i)
			{
				m_StoredCells[i].SetStorageIndex(i);
			}

			// Hides the helper text for first release.
			if (GameManager.FirstReleased == 0)
			{
				GameManager.FirstReleaseDone();
			}

			// Reset cooldown timer.
			m_ReleaseCooldownTimer = 0.0f;
		}
	}

	/*
	 * Moves the current tube to left/right.
	 *
	 * @param dir  The direction to switch. - for left, + for right.
	 */
	private IEnumerator DoSwitch(int dir)
	{
		int nextTube = m_CurTube + dir;
		m_SwitchProgress = 0.0f;

		// Check if we would go out of bounds.
		if (nextTube < 0 || nextTube >= NUM_OF_TUBES)
		{
			yield break;
		}

		// Begin switching.
		m_SwitchingTubes = true;

		// Prepare positions for transition.
		Vector2 posOld = m_TubePositions[m_CurTube];
		Vector2 posNew = m_TubePositions[nextTube];

		// Transition positions.
		float movespeed = SpeedPowerupEnabled ? MOVE_SPEED_POWERED_UP : MOVE_SPEED;
		for (float t = 0.0f; t < movespeed; t += Time.deltaTime)
		{
			// Square the inverse of lerp value to get a less linear effect.
			// float lerpVal = 1.0f - Mathf.Clamp01(t / movespeed);
			// lerpVal = 1.0f - (lerpVal * lerpVal);
			m_SwitchProgress = t / movespeed;
			m_Trans.anchoredPosition = Vector2.Lerp(posOld, posNew, m_SwitchProgress);

			// We allow to cut it a bit short.
			if (m_SwitchProgress > TUBE_SWITCH_MARGIN && m_CurTube != nextTube)
			{
				m_CurTube = nextTube;
			}

			// Wait frame.
			yield return null;
		}

		// Set position to final.
		m_Trans.anchoredPosition = posNew;

		// No longer switching.
		m_SwitchProgress = 0.0f;
		m_SwitchingTubes = false;
	}

	/*
	 * Applies a health powerup.
	 */
	public static void PowerupHealth()
	{
		float newBPM = BPM + POWERUP_HEALTH_ADD;
		if (newBPM > MAX_BPM)
		{
			newBPM = MAX_BPM;
		}
		BPM = newBPM;
		ScoreManager.PowerupAddHealth();
	}

	/*
	 * Applies a speed powerup.
	 */
	public static void PowerupSpeed()
	{
		// Enable speed powerup.
		Inst.SpeedPowerupTimer   = 0.0f;
		Inst.SpeedPowerupEnabled = true;
		ScoreManager.PowerupAddSpeed();
	}
}
