using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMG : MonoBehaviour
{
    public AudioSource au;
    public static AudioMG st;
    private void Awake()
    {
        st = this;
    }
    private void Start()
    {
        
    }
    public void PlayOneShot(AudioClip audioClip)
    {
        if(audioClip == null)
        {
            Debug.Log("empty clip");
            return;
        }
        au.Stop();
        au.PlayOneShot(audioClip);
    }
    public void Play(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.Log("empty clip");
            return;
        }
        au.clip = audioClip;
        au.Play();
    }
    public void Stop()
    {
        au.Stop();
    }
}
