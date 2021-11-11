using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages powerups found in 3D levels
/// </summary>
public class Powerup : MonoBehaviour
{
    [Header("Powerup Settings")]
    public PowerupType powerup;
    public GameObject icon;
    public GameObject particles;
    public GameObject text;
    public TextMeshProUGUI description;
    public GameObject player;
    public GameObject head;
    private PlayerController pc;
    
    [Header("Text Settings")] 
    public float textSize = 0.1f;
    public Color textColor;
    public Color lightTextColor;
    public Font font;

    private void Start()
    {
        icon.GetComponent<Image>().color = powerup.colour;
        pc = player.GetComponent<PlayerController>();
        ApplyTextSettings(description, true);
        SetDescription();
    }

    private void FixedUpdate()
    {
        // Rotate description to face the player
        Vector3 direction = (text.transform.position - head.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        text.transform.rotation = Quaternion.Slerp(text.transform.rotation, lookRotation, Time.fixedDeltaTime * 5);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Change glowing effect to the powerup description when the player is near
        if (other.gameObject == player)
        {
            particles.SetActive(false);
            text.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Give player powerup when they press 'interact' key while standing near the powerup
        if (other.gameObject == player && Input.GetButton("Interact"))
        {
            CollectPowerup();
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Change description back to glowing effect when player moves away from the powerup
        if (other.gameObject == player)
        {
            particles.SetActive(true);
            text.SetActive(false);
        }
    }

    /// <summary>
    /// Applies text settings to a given TextMeshPro object
    /// </summary>
    /// <param name="textMesh">The TextMeshPro object which needs the settings to be applied to it</param>
    /// <param name="light">A bool which determines the color of text. Text is light when true, dark when false</param>
    private void ApplyTextSettings(TextMeshProUGUI textMesh, bool light)
    {
        textMesh.font = TMP_FontAsset.CreateFontAsset(font);
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
        textMesh.fontSize = textSize;
        textMesh.fontStyle = FontStyles.SmallCaps;

        if (light)
        {
            textMesh.color = lightTextColor;
        }
        else
        {
            textMesh.color = textColor;
        }
    }

    /// <summary>
    /// Modifies powerup description to appropriate powerup type
    /// </summary>
    private void SetDescription()
    {
        string title = "<<< " + powerup.powerupName + " >>>";
        string body;

        switch (powerup.variant)
        {
            case PowerupVariant.Health:
                body = powerup.description.Replace("<replace>", pc.healthMultiplierAmount.ToString(CultureInfo.InvariantCulture));
                break;
            case PowerupVariant.MagicRange:
                body = powerup.description.Replace("<replace>", pc.magicRangeMultiplierAmount.ToString(CultureInfo.InvariantCulture));
                break;
            case PowerupVariant.MagicRate:
                body = powerup.description;
                break;
            case PowerupVariant.MagicSpeed:
                body = powerup.description.Replace("<replace>", pc.magicSpeedMultiplierAmount.ToString(CultureInfo.InvariantCulture));
                break;
            case PowerupVariant.MagicDamage:
                body = powerup.description.Replace("<replace>", pc.magicDamageMultiplierAmount.ToString(CultureInfo.InvariantCulture));
                break;
            case PowerupVariant.SwordSpeed:
                body = powerup.description;
                break;
            case PowerupVariant.SwordDamage:
                body = powerup.description.Replace("<replace>", pc.swordDamageMultiplierAmount.ToString(CultureInfo.InvariantCulture));
                break;
            case PowerupVariant.VineExtent:
                body = powerup.description.Replace("<replace>", pc.vineExtentMultiplierAmount.ToString(CultureInfo.InvariantCulture));
                break;
            case PowerupVariant.VineCooldown:
                body = powerup.description.Replace("<replace>", pc.vineCooldownMultiplierAmount.ToString(CultureInfo.InvariantCulture));
                break;
            case PowerupVariant.LevitateRange:
                body = powerup.description.Replace("<replace>", pc.levitateRangeMultiplierAmount.ToString(CultureInfo.InvariantCulture));
                break;
            case PowerupVariant.LevitateCooldown:
                body = powerup.description.Replace("<replace>", pc.levitateCooldownMultiplierAmount.ToString(CultureInfo.InvariantCulture));
                break;
            default:
                title = "<<<Error>>>";
                body = "Invalid Powerup Type";
                break;
        }

        string descriptionString = title + "\n \n" + body + "\n \n" + "Press F to collect powerup.";

        description.text = descriptionString;
    }

    /// <summary>
    /// Applies powerup to the player, not the GameManager so if the player dies in the level the powerups are then removed
    /// </summary>
    private void CollectPowerup()
    {
        switch (powerup.variant)
        {
            case PowerupVariant.Health:
                pc.healthMultiplier += 1;
                pc.SetSliderMaxHealth();
                break;
            case PowerupVariant.MagicRange:
                pc.magicRangeMultiplier += 1;
                break;
            case PowerupVariant.MagicRate:
                pc.magicRateMultiplier += 1;
                break;
            case PowerupVariant.MagicSpeed:
                pc.magicSpeedMultiplier += 1;
                break;
            case PowerupVariant.MagicDamage:
                pc.magicDamageMultiplier += 1;
                break;
            case PowerupVariant.SwordSpeed:
                pc.swordSpeedMultiplier += 1;
                break;
            case PowerupVariant.SwordDamage:
                pc.swordDamageMultiplier += 1;
                break;
            case PowerupVariant.VineExtent:
                pc.vineExtentMultiplier += 1;
                break;
            case PowerupVariant.VineCooldown:
                pc.vineCooldownMultiplier += 1;
                break;
            case PowerupVariant.LevitateRange:
                pc.levitateRangeMultiplier += 1;
                break;
            case PowerupVariant.LevitateCooldown:
                pc.levitateCooldownMultiplier += 1;
                break;
            default:
                Debug.Log("Powerup not collected");
                break;
        }
    }
}

