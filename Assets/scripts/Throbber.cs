/*
 * Throbber.cs
 *
 * This is a nice little "throbber" effect which
 * will adjust based on player's BPM.
 *
 * Also helps the user realise what the game is even meant
 * to be...
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Throbber : MonoBehaviour
{
	// Serialised members.
	[SerializeField] private float m_ThrobFactor = 1.0f;

	// Privates.
	private RectTransform m_Trans;
	private Vector2       m_SizeOrig;
	private Vector2       m_SizeThrob;
	private float         m_Timer = 0.0f;

	/*
	 * Use this for initialisation.
	 */
	private void Start()
	{
		m_Trans = GetComponent<RectTransform>();
		m_SizeOrig = m_Trans.sizeDelta;
		m_SizeThrob = m_SizeOrig;
		m_SizeThrob.x *= m_ThrobFactor;
	}

	/*
	 * Called each frame.
	 */
	private void Update()
	{
		// Increment timer.
		m_Timer = Mathf.Clamp01(m_Timer + Time.deltaTime * Player.BPM / 60.0f);
		if (m_Timer == 1.0f)
		{
			m_Timer = 0.0f;
		}

		// Resize based on quadratic curve from timer.
		// y = 4(x - 0.5)^2
		float lerp = m_Timer - 0.5f;
		lerp *= lerp * 4.0f;
		m_Trans.sizeDelta = Vector2.Lerp(m_SizeOrig, m_SizeThrob, lerp);
	}
}
