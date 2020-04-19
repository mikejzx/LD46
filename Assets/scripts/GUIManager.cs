/*
 * GUIManager.cs
 *
 * Manages GUI in the application.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
	// Singleton
	public static GUIManager Inst;

	// Serialised members
	[SerializeField] private Text m_TextBPM     = null;
	[SerializeField] private Text m_TextScore   = null;
	[SerializeField] private Text m_TextPowerup = null;
	[Header("Lose screen")]
	[SerializeField] private CanvasGroup m_LoseScreenCg    = null;
	[SerializeField] private Text        m_LoseTextScore   = null;
	[SerializeField] private Text        m_LoseTextBest    = null;
	[SerializeField] private Text        m_LoseTextHBPM    = null;
	[SerializeField] private Text        m_LoseTextHealths = null;
	[SerializeField] private Text        m_LoseTextSpeeds  = null;
	[SerializeField] private Text        m_LoseTextComment = null;
	[Header("Misc")]
	[SerializeField] private CanvasGroup m_Black = null;
	[SerializeField] private CanvasGroup m_PauseScreenCg   = null;

	// Privates
	private Coroutine m_FadeCr;

	// Constants
	private const float FADE_TIME = 1.0f;

	/*
	 * Called before Start.
	 */
	private void Awake() => Inst = this;

	/*
	 * Set the BPM text.
	 *
	 * @param bpm  The BPM to set to.
	 */
	public static void SetBPM(float bpm)
		=> Inst.m_TextBPM.text = $"Heart rate: {Mathf.Round(bpm)} BPM";

	/*
	 * Set score text.
	 *
	 * @param s  The score text to set to.
	 */
	public static void SetScore (int score)
		=> Inst.m_TextScore.text = $"Score: {score:n0}";

	/*
	 * Shows the lose screen.
	 */
	public static void ShowLoseScreen()
	{
		// Enable
		Inst.m_LoseScreenCg.gameObject.SetActive(true);
		Inst.m_LoseScreenCg.alpha = 1.0f;

		// Set texts.
		Inst.m_LoseTextScore.text   = $"Score: {ScoreManager.Score:n0}";
		Inst.m_LoseTextBest.text    = $"Record: {ScoreManager.ScoreBest:n0}";
		Inst.m_LoseTextHBPM.text    = $"Highest BPM: {ScoreManager.HighestBPM:n0}";
		Inst.m_LoseTextHealths.text = $"Health powerups collected: {ScoreManager.PowerupsHealthUsed:n0}";
		Inst.m_LoseTextSpeeds.text  = $"Speed powerups collected: {ScoreManager.PowerupsSpeedUsed:n0}";

		// Show additional text if we beat highest score.
		if (ScoreManager.Score > ScoreManager.ScoreBest)
		{
			Inst.m_LoseTextScore.text += " <color='lime'>- New record</color>";
		}

		// Show a nice comment.
		string comm = string.Empty;
		if (ScoreManager.Score == 0)
		{
			comm = "Maybe next time?";
		}
		else if (ScoreManager.Score < 10)
		{
			comm = "Better than nothing...";
		}
		else if (ScoreManager.Score < 50)
		{
			comm = "Decent job!";
		}
		else if (ScoreManager.Score < 100)
		{
			comm = "Not too shabby!";
		}
		else if (ScoreManager.Score < 150)
		{
			comm = "Pretty damn good!";
		}
		else
		{
			comm = "'A' for effort!";
		}
		Inst.m_LoseTextComment.text = comm;

		// Check if we beat highest score.
		ScoreManager.CheckHiscore(ScoreManager.Score);
	}

	/*
	 * Shows the pause screen.
	 */
	public static void ShowPauseScreen()
	{
		Inst.m_PauseScreenCg.gameObject.SetActive(true);
		Inst.m_PauseScreenCg.alpha = 1.0f;
	}

	/*
	 * Hides the pause screen.
	 */
	public static void HidePauseScreen()
	{
		Inst.m_PauseScreenCg.gameObject.SetActive(false);
		Inst.m_PauseScreenCg.alpha = 0.0f;
	}

	/*
	 * Black fade.
	 *
	 * @param fadein      Set true to fade into black.
	 * @param speed[opt]  The fade speed.
	 */
	public static void BlackFade(bool fadein, float speed=FADE_TIME)
	{
		if (Inst.m_FadeCr != null)
		{
			Inst.StopCoroutine(Inst.m_FadeCr);
		}
		Inst.m_FadeCr = Inst.StartCoroutine(Inst.BlackFadeCr(fadein, speed));
	}

	/*
	 * Black fade coroutine.
	 */
	private IEnumerator BlackFadeCr(bool fadein, float speed)
	{
		m_Black.alpha = fadein ? 0.0f : 1.0f;
		m_Black.gameObject.SetActive(true);

		for (float i = 0.0f; i < 1.0f; i = Mathf.Clamp01(i + Time.unscaledDeltaTime * speed))
		{
			m_Black.alpha = fadein ? i : (1.0f - i);
			yield return null;
		}

		m_Black.alpha = fadein ? 1.0f : 0.0f;

		if (!fadein)
		{
			m_Black.gameObject.SetActive(false);
		}
	}

	/*
	 * Enable/disable the speed powerup text.
	 *
	 * @param e  Enable?
	 */
	public static void PowerupTextEnabled(bool e)
		=> Inst.m_TextPowerup.gameObject.SetActive(e);

	/*
	 * Set the powerup text time.
	 *
	 * @param t  The time.
	 */
	public static void PowerupTime(float t)
	{
		Inst.m_TextPowerup.text = $"Speed powerup: {t:n0}";
	}
}
