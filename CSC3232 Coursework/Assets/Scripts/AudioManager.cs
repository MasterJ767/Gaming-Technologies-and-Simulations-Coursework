using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    public Audio[] sounds;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        foreach (Audio sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.playOnAwake = false;
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = true;
            if (sound.blend3D)
            {
                sound.source.spatialBlend = 1;
            }
            else
            {
                sound.source.spatialBlend = 0;
            }
        }
    }

    public void Play(string audioName)
    {
        Array.Find(sounds, sound => sound.name == audioName)?.source.Play();
    }

    public void Stop(string audioName)
    {
        Array.Find(sounds, sound => sound.name == audioName)?.source.Stop();
    }
}
