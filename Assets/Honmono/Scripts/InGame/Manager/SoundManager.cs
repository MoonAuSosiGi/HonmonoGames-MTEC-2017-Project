using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singletone<SoundManager> {

    // -- Audio Source -------------------------------------//
    [SerializeField]
    private AudioSource m_bgmSource = null;
    [SerializeField]
    private AudioSource m_soundSource = null;

    [SerializeField]
    private List<AudioClip> m_bgmList = new List<AudioClip>();
    // -----------------------------------------------------//
    
    // temp Code
    public void PlayBGM(int stage)
    {
        if (m_bgmSource.clip == m_bgmList[stage])
            return;
        m_bgmSource.clip = m_bgmList[stage];
        m_bgmSource.Play();
    }

    public void PlaySound(AudioClip sound)
    {
        //if(sound != null)
        {
            m_soundSource.clip = sound;
            m_soundSource.Play();
        }
    }
}
