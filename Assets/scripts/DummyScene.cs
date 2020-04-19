/*
 * DummyScene.cs
 *
 * All this script does is just load the main scene.
 * This is run in the dummy scene so that all the stats
 * and game state are reset.
 */

using System.Collections;
using UnityEngine;

public class DummyScene : MonoBehaviour
{
	/*
	 * Use this for initialisation.
	 */
	private void Start()
	{
		Time.timeScale = 1.0f;
		UnityEngine.SceneManagement.SceneManager.LoadScene("scn_main");
	}
}
