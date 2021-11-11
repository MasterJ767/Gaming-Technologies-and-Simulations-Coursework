using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable Object for defining powerup variants
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/PowerupType")]
public class PowerupType : ScriptableObject
{
    public string powerupName;
    public PowerupVariant variant;
    public Color colour;
    public string description;
}

[Serializable]
public enum PowerupVariant
{
    Health,
    MagicRange,
    MagicRate,
    MagicSpeed,
    MagicDamage,
    SwordSpeed,
    SwordDamage,
    VineExtent,
    VineCooldown,
    LevitateRange,
    LevitateCooldown
}
