/*
 * ScoreManager.cs
 *
 * Manages score and stats in the game.
 */

using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
	// Singleton.
	public static ScoreManager Inst;

	// Properties.
	private int m_Score = 0;
	public static int Score {
		get => Inst.m_Score;
		private set {
			GUIManager.SetScore(value);
			Inst.m_Score = value;
		}
	}
	public static int ScoreBest          { get; private set; }
	public static int HighestBPM         { get; private set; }
	public static int PowerupsHealthUsed { get; private set; }
	public static int PowerupsSpeedUsed  { get; private set; }

	// Constants.
	private const string PREF_KEY_SCORE_BEST = "score_best";
	public static readonly int[] SCORE_CELL_PASS = {
		4, 3, 2, 1
	};

	/*
	 * Called before Start.
	 */
	private void Awake() => Inst = this;

	/*
	 * Called on first frame.
	 */
	private void Start()
	{
		// Get best score if we have it.
		if (!PlayerPrefs.HasKey(PREF_KEY_SCORE_BEST))
		{
			PlayerPrefs.SetInt(PREF_KEY_SCORE_BEST, 0);
			return;
		}
		ScoreBest = PlayerPrefs.GetInt(PREF_KEY_SCORE_BEST);
		HighestBPM = 0;
		PowerupsHealthUsed = 0;
		PowerupsSpeedUsed = 0;
	}

	/*
	 * Adds score for cell passing.
	 *
	 * @param c    The type of cell collected.
	 * @param mul  Score multiplier.
	 */
	public static void ScoreAdd(CellType c, int mul=1)
		=> Score += SCORE_CELL_PASS[(int)c] * mul;

	/*
	 * Called when the player dies. Just checks if their score
	 * is larger than the previous highest score.
	 *
	 * @param s  The score to test.
	 */
	public static void CheckHiscore(int s)
	{
		// If we are larger, set the pref.
		if (s > ScoreBest)
		{
			ScoreBest = s;
			PlayerPrefs.SetInt(PREF_KEY_SCORE_BEST, ScoreBest);
		}
	}

	/*
	 * Check if the BPM is larger than highest.
	 *
	 * @param bpm  The BPM to check.
	 */
	public static void CheckBPM(int bpm)
	{
		HighestBPM = Mathf.Max(HighestBPM, bpm);
	}

	/*
	 * Called when a health powerup is used.
	 */
	public static void PowerupAddHealth()
		=> ++PowerupsHealthUsed;

	/*
	 * Called when a speed powerup is used.
	 */
	public static void PowerupAddSpeed()
		=> ++PowerupsSpeedUsed;
}
