using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTriggerer : MonoBehaviour
{
    [SerializeField] 
    private AudioSource audioSource;

    [SerializeField] 
    private AudioClip audioClip;

    [SerializeField] [Range(0, 10)] private float startDelay = 1;
    
    public void PlayAudioClip()
    {
        audioSource.clip = audioClip;
        audioSource.PlayDelayed(startDelay);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public float GetVolume()
    {
        return audioSource.volume;
    }
    
}
