/*
 * BackgroundScroller.cs
 *
 * Scrolls the background to make the game look more
 * interesting than it really is.
 */

using System.Collections;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
	// Private member.
	private SpriteRenderer m_Spr;
	private float          m_Cur;

	// Constants
	private const float SIZE = 19.2f;
	private const float SPEED = 5.0f;

	/*
	 * Use this for initialisation.
	 */
	private void Start()
	{
		m_Spr = GetComponent<SpriteRenderer>();
		m_Spr.size = new Vector2(SIZE, SIZE);
	}

	/*
	 * Called after update.
	 */
	private void LateUpdate()
	{
		// Compute size.
		m_Cur += Time.deltaTime / SPEED;
		if (m_Cur > 1.0f)
		{
			m_Cur = 0.0f;
		}
		float s = (1.0f + m_Cur) * SIZE;
		m_Spr.size = new Vector2(s, s);
	}
}
