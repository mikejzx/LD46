/*
 * Menu.cs
 *
 * Used for the main menu.
 */

using System.Collections;
using UnityEngine;

public class Menu : MonoBehaviour
{
	/*
	 * Load the game.
	 */
	public void StartGame()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("scn_main");
	}

	/*
	 * Quit the game.
	 */
	public void QuitGame()
	{
		Application.Quit();
	}
}
