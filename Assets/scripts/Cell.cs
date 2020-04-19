/*
 * Cell.cs
 *
 * Defines a "cell" in the game.
 * Each cell has a colour associated with it, which tells
 * the user which "tube" it must go in. If it doesn't go in
 * the correct tube, the user loses BPM points.
 * When the BPM drops too low, the user loses.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
 * The cell type.
 */
public enum CellType
{
	Green  = 0,
	Cyan   = 1,
	Orange = 2,
	Pink   = 3
};

/*
 * The state of a cell.
 * JustSpawned -> The cell has just been spawned.
 * Stored      -> The user is storing the cell.
 * Ended       -> The cell has finished being controlled.
 * Died        -> The cell was not stored in time.
 */
public enum CellState
{
	JustSpawned,
	Stored,
	Ended,
	Died
};

/*
 * Used for powerups. Regular cells are simply Not.
 */
public enum Powerup
{
	Not    = 0,
	Health = 1,
	Speed  = 2
};

public class Cell : MonoBehaviour
{
	// Serialised members.
	[SerializeField] private Image       m_ImgID = null;
	[SerializeField] private CanvasGroup m_Cg    = null;
	[SerializeField] private Powerup     m_Powerup = Powerup.Not;

	// Properties.
	public CellType  Type  { get; private set; }
	public CellState State { get; private set; }
	public bool IsPowerup  { get => m_Powerup != Powerup.Not; }
	public bool StoredCell_AtTargetPt {
		get => m_PathCurPoint == m_PathStoredTargetPt;
	}

	// Privates.
	private RectTransform m_Trans;

	// Path-related.
	private CellPath m_Path;
	private int      m_PathCurPoint = 0;
	private float    m_PathComplete = 0.0f;
	private float    m_PathSectorCurComplete = 0.0f;
	private float    m_PathSectorPrevDist    = 0.0f;
	private int      m_PathStoredTargetPt    = -1;
	private int      m_StorageIndex          = -1;
	private int      m_Tube                  = -1;

	/*
	 * Initialise the cell. This needs to be called by the spawner.
	 *
	 * @param powup  If this is a powerup, set this to anything but Not.
	 */
	public void Initialise(Powerup powup=Powerup.Not)
	{
		// Set members. Give random celltype.
		// We assume all cells that are being initialised
		// were just spawned.
		// Green is most difficult, so we give user 1/5 chance of getting them.
		if (Random.Range(0, 6) >= 4)
		{
			Type = CellType.Green;
		}
		else
		{
			Type  = (CellType)Random.Range((int)CellType.Cyan, (int)CellType.Pink + 1);
		}
		SetState(CellState.JustSpawned);
		m_Trans = GetComponent<RectTransform>();

		// Set image colour based on type.
		m_ImgID.color = CellManager.CellColours[Type];

		// If we have a path, move to the first point position.
		m_PathCurPoint = 0;
		m_PathSectorCurComplete = 0.0f;
		m_PathComplete = 0.0f;
		if (m_Path != null)
		{
			m_Trans.anchoredPosition = m_Path[m_PathCurPoint].PosAnch;
		}

		// Enable the object for viewing.
		gameObject.SetActive(true);
	}

	/*
	 * Called each frame.
	 */
	private void Update()
	{
		// If we have a path, follow it.
		if (m_Path != null)
		{

			// If we are stored and at correct point we don't need to compute
			// all this shit, just set the position we should be at without lerp.
			// Prevents the delay in player movement also.
			if (State == CellState.Stored && StoredCell_AtTargetPt)
			{
				m_Trans.position = m_Path[m_PathStoredTargetPt].PosWorld;
				return;
			}

			// Increment completion if this cell isn't in storage,
			// or has free space available in storage.
			if (State != CellState.Stored ||
				(State == CellState.Stored &&
				 m_PathCurPoint < m_PathStoredTargetPt))
			{
				m_PathComplete += Time.deltaTime * CellManager.CellSpeed / m_Path.TotalDistance;
			}

			// Calculate completion of current sector.
			float distTravelled = m_Path.TotalDistance * m_PathComplete;
			m_PathSectorCurComplete = Mathf.Clamp01(
				(distTravelled - m_PathSectorPrevDist)
				/ m_Path[m_PathCurPoint].Distance
			);

			// Check whether current point index should increment.
			float distToNextPt = Vector3.Distance(transform.position, m_Path[m_PathCurPoint + 1].PosWorld);
			bool passedPoint = Mathf.Round(distToNextPt * 1000.0f) / 1000.0f == 0.0f;
			if ((passedPoint || m_PathSectorCurComplete >= 1.0f) && m_PathCurPoint < m_Path.Size - 2)
			{
				m_PathSectorPrevDist += m_Path[m_PathCurPoint].Distance;
				++m_PathCurPoint;
				m_PathSectorCurComplete = 0.0f;
			}

			// Interpolate between path points based on completion.
			m_Trans.position = Vector2.Lerp(
				m_Path[m_PathCurPoint].PosWorld,
				m_Path[m_PathCurPoint + 1].PosWorld,
				m_PathSectorCurComplete
			);

			// If we pass the completion point.
			if (m_PathComplete >= 1.0f &&
				State != CellState.Stored)
			{
				// Completed, call the callback method.
				OnPathComplete();
			}
		}
	}

	/*
	 * Called when the path is completed.
	 */
	private void OnPathComplete()
	{
		switch(State)
		{
		/*
		 * The cell has completed the top path. Move into storeage if the player is
		 * at the first tube.
		 */
		case CellState.JustSpawned:
		{
			Player.Inst.AddCellToStorage(this);
		} break;

		/*
		 * Ended cell. Destroy it.
		 * TODO: Object pool.
		 */
		case CellState.Ended:
		{
			// If we are in correct tube, then increase BPM slightly.
			// If in wrong tube, subtract a bit of BPM.
			if(Type != (CellType)((int)CellType.Pink - m_Tube))
			{
				if (!IsPowerup)
				{
					Player.DeductBPM(Player.HEALTH_DROP_CELL_WRONG);
				}
				// Play wrong cell sound.
				AudioManager.SFXCellWrong();
			}
			else
			{
				if (!IsPowerup)
				{
					Player.AddBPM(Type);
					ScoreManager.ScoreAdd(Type);
				}
				else if (m_Powerup == Powerup.Health)
				{
					// Health powerup, add a bit of health.
					Player.PowerupHealth();
					ScoreManager.ScoreAdd(Type, 2);
				}
				else if (m_Powerup == Powerup.Speed)
				{
					// Speed powerup, enable on player.
					Player.PowerupSpeed();
					ScoreManager.ScoreAdd(Type, 2);
				}
				// Play correct sound.
				AudioManager.SFXCellCorrect();
			}

			Destroy(gameObject);
		} break;
		}
	}

	/*
	 * Set the state of the Cell.
	 */
	public void SetState(CellState state)
	{
		// Assign.
		State = state;

		// If the cell dies, just play a little animation and destroy.
		// Player BPM is also deducted.
		if (State == CellState.Died)
		{
			if (!IsPowerup)
			{
				Player.DeductBPM(Player.HEALTH_DROP_CELL_DIE);
			}
			StartCoroutine(DieEffect());
		}

		// Set the path variables.
		m_PathCurPoint          = 0;
		m_PathComplete          = 0.0f;
		m_PathSectorCurComplete = 0.0f;
		m_PathSectorPrevDist    = 0.0f;

		// Reset storage information if we aren't being stored.
		if (State != CellState.Stored)
		{
			SetStorageTargetPoint(-1);
			m_StorageIndex = -1;
		}

		// If not an ending cell, set tube to -1.
		if (State != CellState.Ended)
		{
			m_Tube = -1;
			m_Path = CellManager.GetPath(State, Type);
		}
		else
		{
			m_Path = CellManager.GetPath(State, (CellType)((int)CellType.Pink - m_Tube));
		}
	}

	/*
	 * Set the storage target point.
	 * This only needs to be called for cells
	 * that are in storage. This point is where the
	 * cell is aiming to be. If the front cell if removed,
	 * then this function gets called for all previous cells
	 * to shift over.
	 *
	 * @param pt  The target point to set to.
	 */
	public void SetStorageTargetPoint(int pt)
	{
		m_PathStoredTargetPt = pt;
	}

	/*
	 * Sets this points index in storage.
	 *
	 * @param idx  The index to set to.
	 */
	public void SetStorageIndex(int idx)
	{
		m_StorageIndex = idx;
		SetStorageTargetPoint(Player.GetStorageTargetPoint(m_StorageIndex));
	}

	/*
	 * Sets the tube for cells going into a tube.
	 *
	 * @param t  The tube to go in.
	 */
	public void SetTube(int t)
	{
		m_Tube = t;
	}


	/*
	 * A nice effect for when cell dies.
	 */
	private IEnumerator DieEffect()
	{
		// Play sound effect.
		AudioManager.SFXCellDie();

		// Scale the cell down to zero.
		// Also fade out.
		for(float t = 0.0f; t < 1.0f; t = Mathf.Clamp01(t + Time.deltaTime * 5.0f))
		{
			m_Cg.alpha = 1.0f - t;
			m_Trans.localScale = Vector2.Lerp(Vector2.one, Vector2.zero, t);
			yield return null;
		}

		// Destroy the object.
		Destroy(gameObject);
	}
}
