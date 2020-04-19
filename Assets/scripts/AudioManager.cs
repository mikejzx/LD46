/*
 * AudioManager.cs
 *
 * Handles audio. BGM and SFX.
 */

using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	// Singleton
	public static AudioManager Inst;

	// Serialised members.
	[SerializeField] private bool m_MuteMusic = false;
	[SerializeField] private AudioClip m_BGM = null;
	[SerializeField] private AudioClip m_SFXCellDie = null;
	[SerializeField] private AudioClip m_SFXCellWrong = null;
	[SerializeField] private AudioClip m_SFXCellRight = null;

	// Private members.
	private AudioSource m_Audio;

	/*
	 * Called before Start
	 */
	private void Awake () => Inst = this;

	/*
	 * Use this for initialisation.
	 */
	private void Start()
	{
		m_Audio = GetComponent<AudioSource>();
		m_Audio.clip = m_BGM;
		m_Audio.loop = true;

		// Play music if not muted.
		if (!m_MuteMusic)
		{
			m_Audio.Play();
		}
	}

	/*
	 * Plays the cell die sound.
	 */
	public static void SFXCellDie()
		=> Inst.m_Audio.PlayOneShot(Inst.m_SFXCellDie);

	/*
	 * Plays the sound when cell goes in wrong tube.
	 */
	public static void SFXCellWrong()
		=> Inst.m_Audio.PlayOneShot(Inst.m_SFXCellWrong);

	/*
	 * Plays the sound when cell goes in correct tube.
	 */
	public static void SFXCellCorrect()
		=> Inst.m_Audio.PlayOneShot(Inst.m_SFXCellRight);
}
