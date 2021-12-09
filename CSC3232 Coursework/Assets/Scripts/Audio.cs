using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Audio
{
    public string name;
    
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(0.1f, 3f)]
    public float pitch;

    [NonSerialized] 
    public AudioSource source;
}
