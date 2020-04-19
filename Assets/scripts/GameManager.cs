/*
 * GameManager.cs
 *
 * Manages the game state.
 */

using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	// Publics
	public static bool GameStarted   { get; private set; } = false;
	public static int  FirstReleased { get; set; } = -1;

	// Privates.
	private static bool m_LoseScreen  = false;
	private static bool m_PauseScreen = false;

	/*
	 * Called on first frame of game.
	 */
	private void Start()
	{
		// Fade from black.
		m_LoseScreen  = false;
		m_PauseScreen = false;
		GUIManager.BlackFade(false);

		// Deliberately don't reset these, so after each restart we
		// don't see help message. Rebooting game will stop this though.
		// GameStarted   = false;
		// FirstReleased = -1;
	}

	/*
	 * Start the game. Gets called when the user makes first input.
	 */
	public static void InputMade()
	{
		GameStarted = true;

		// Hide the help text.
		GUIManager.HelpTextHide();
	}

	/*
	 * Called after releasing first cell.
	 */
	public static void FirstReleaseDone()
	{
		FirstReleased = 1;
		GUIManager.HelpTextHide();
	}

	/*
	 * Called each frame.
	 */
	private void Update()
	{
		// Check for pause input.
		if (Input.GetKeyDown(KeyCode.P) && !m_LoseScreen)
		{
			if (!m_PauseScreen)
			{
				PauseGame();
			}
			else
			{
				ResumeGame();
			}
		}
	}

	/*
	 * Lose the game.
	 */
	public static void Lose()
	{
		m_LoseScreen = true;
		GUIManager.BlackFade(true, 5.0f);
		Time.timeScale = 0.0f;
		GUIManager.ShowLoseScreen();
	}

	/*
	 * Quit the game.
	 */
	public void _Quit() => Quit();
	public static void Quit()
	{
		Application.Quit();
	}

	/*
	 * Restart the game. Loads the dummy scene.
	 */
	public void RestartGame()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("scn_dummy");
	}

	/*
	 * Pauses the game.
	 */
	public void PauseGame()
	{
		// Don't allow pause on lose screen.
		if (m_LoseScreen || m_PauseScreen)
		{
			return;
		}
		m_PauseScreen = true;
		GUIManager.BlackFade(true, 5.0f);
		Time.timeScale = 0.0f;
		GUIManager.ShowPauseScreen();
	}

	/*
	 * Resume the game.
	 */
	public void ResumeGame()
	{
		if (!m_PauseScreen || m_LoseScreen)
		{
			return;
		}
		m_PauseScreen = false;
		GUIManager.BlackFade(false, 5.0f);
		Time.timeScale = 1.0f;
		GUIManager.HidePauseScreen();
	}
}
