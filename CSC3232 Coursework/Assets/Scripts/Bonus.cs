using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Implements the functionality for the monty hall problem, which serves as bonus levels in the game which provide the player
/// with a way of obtaining additional random powerups.
/// </summary>
public class Bonus : MonoBehaviour
{
    [Header("Pages")]
    public GameObject startPage;
    public GameObject highlightPage;
    public GameObject resultPage;

    [Header("Highlight Page Customisations")]
    public TextMeshProUGUI messageText;
    public Button leftButton;
    public Image leftImage;
    public Button centreButton;
    public Image centreImage;
    public Button rightButton;
    public Image rightImage;

    [Header("Result Page Customisations")] 
    public TextMeshProUGUI resultText;
    public Image icon;
    public Sprite fail;
    public Sprite Syringe;
    public PowerupType[] powerupPossibilities;
    
    private SelectedCompartment selected = SelectedCompartment.None;
    
    private SelectedCompartment result;
    private PowerupType powerup;

    private SelectedCompartment shown;

    private void Start()
    {
        // Determine which door the powerup is behind
        result = (SelectedCompartment) UnityEngine.Random.Range(1, 3);
        
        // Determine which powerup type is the prize
        powerup = powerupPossibilities[UnityEngine.Random.Range(0, powerupPossibilities.Length - 1)];

        leftButton.enabled = false;
        centreButton.enabled = false;
        rightButton.enabled = false;
    }
    
    /// <summary>
    /// Sets the player's initial decision to the left door
    /// </summary>
    public void LeftInitialPress()
    {
        selected = SelectedCompartment.Left;
        ColorBlock colors = leftButton.colors;
        colors.normalColor = Color.cyan;
        TransitionHighlight();
    }
    
    /// <summary>
    /// Sets the player's initial decision to the middle door
    /// </summary>
    public void CentreInitialPress()
    {
        selected = SelectedCompartment.Centre;
        ColorBlock colors = centreButton.colors;
        colors.normalColor = Color.cyan;
        TransitionHighlight();
    }
    
    /// <summary>
    /// Sets the player's initial decision to the right door
    /// </summary>
    public void RightInitialPress()
    {
        selected = SelectedCompartment.Right;
        ColorBlock colors = rightButton.colors;
        colors.normalColor = Color.cyan;
        TransitionHighlight();
    }
    
    /// <summary>
    /// Transitions to a display which reveals one of the doors with nothing behind it
    /// </summary>
    private void TransitionHighlight()
    {
        DetermineShown();
        SetHighlightText();
        
        startPage.gameObject.SetActive(false);
        highlightPage.gameObject.SetActive(true);

        Thread.Sleep(1000);

        SetActiveButtons();
    }
    
    /// <summary>
    /// Sets the text on the screen after the player's initial decision
    /// </summary>
    private void SetHighlightText()
    {
        string temp = messageText.text;
        string dir;
        string altdir;

        switch (selected)
        {
            case SelectedCompartment.Left:
                dir = "left";
                break;
            case SelectedCompartment.Centre:
                dir = "central";
                break;
            case SelectedCompartment.Right:
                dir = "right";
                break;
            default:
                dir = "ERROR";
                break;
        }

        switch (shown)
        {
            case SelectedCompartment.Left:
                altdir = "left";
                break;
            case SelectedCompartment.Centre:
                altdir = "central";
                break;
            case SelectedCompartment.Right:
                altdir = "right";
                break;
            default:
                altdir = "ERROR";
                break;
        }

        temp = temp.Replace("<direction>", dir);
        temp = temp.Replace("<altdirection>", altdir);

        messageText.text = temp;
    }
    
    /// <summary>
    /// Determines which door to reveal to the player as having nothing behind it
    /// </summary>
    private void DetermineShown()
    {
        List<SelectedCompartment> compartments = new List<SelectedCompartment>(){ SelectedCompartment.Left, SelectedCompartment.Centre, SelectedCompartment.Right};
        if (selected == result)
        {
            compartments.Remove(selected);
            shown = compartments[UnityEngine.Random.Range(0, 1)];
        }
        else
        {
            compartments.Remove(selected);
            compartments.Remove(result);
            shown = compartments[0];
        }
    }

    /// <summary>
    /// Visually displays the revealed door and enables the buttons for the two doors the player cna select
    /// </summary>
    private void SetActiveButtons()
    {
        switch (shown)
        {
            case SelectedCompartment.Left:
                leftImage.gameObject.SetActive(true);
                centreButton.enabled = true;
                rightButton.enabled = true;
                break;
            case SelectedCompartment.Centre:
                centreImage.gameObject.SetActive(true);
                leftButton.enabled = true;
                rightButton.enabled = true;
                break;
            case SelectedCompartment.Right:
                rightImage.gameObject.SetActive(true);
                centreButton.enabled = true;
                leftButton.enabled = true;
                break;
            default:
                Debug.Log("Something went wrong in the monty hall problem");
                break;
        }
    }
    
    /// <summary>
    /// Set's the player's new decision to the left door
    /// </summary>
    public void LeftSecondPress()
    {
        selected = SelectedCompartment.Left;
        Check();
    }

    /// <summary>
    /// Set's the player's new decision to the middle door
    /// </summary>
    public void CentreSecondPress()
    {
        selected = SelectedCompartment.Centre;
        Check();
    }

    /// <summary>
    /// Set's the player's new decision to the right door
    /// </summary>
    public void RightSecondPress()
    {
        selected = SelectedCompartment.Right;
        Check();
    }

    /// <summary>
    /// Determine if the player has selected correctly or not
    /// </summary>
    private void Check()
    {
        if (selected == result)
        {
            Success();
        }
        else
        {
            Failure();
        }
    }

    /// <summary>
    /// Set final page to display syringe, coloured correctly and edit text appropriately
    /// </summary>
    private void Success()
    {
        SetResultText(true);
        icon.sprite = Syringe;
        icon.color = powerup.colour;
        
        ApplyPowerup();
        
        TransitionResult();
    }
    
    /// <summary>
    /// Set final page to displaye red cross and edit text appropriately
    /// </summary>
    private void Failure()
    {
        SetResultText(false);
        icon.sprite = fail;
        
        TransitionResult();
    }
    
    /// <summary>
    /// Set final page text
    /// </summary>
    /// <param name="won">A bool which is true if the player has won and false if they have lost</param>
    private void SetResultText(bool won)
    {
        string temp = resultText.text;
        string dir;
        string res;
        
        switch (selected)
        {
            case SelectedCompartment.Left:
                dir = "left";
                break;
            case SelectedCompartment.Centre:
                dir = "central";
                break;
            case SelectedCompartment.Right:
                dir = "right";
                break;
            default:
                dir = "ERROR";
                break;
        }

        if (won)
        {
            res = "a " + powerup.powerupName;
        }
        else
        {
            res = "the compartment is empty";
        }
        
        temp = temp.Replace("<direction>", dir);
        temp = temp.Replace("<result>", res);

        resultText.text = temp;
    }

    /// <summary>
    /// Applies powerup to GameManager object so that the player can take advantage of the powerup in-level
    /// </summary>
    private void ApplyPowerup()
    {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        switch (powerup.variant)
        {
            case PowerupVariant.Health:
                gm.healthMultiplier += 1;
                break;
            case PowerupVariant.MagicRange:
                gm.magicRangeMultiplier += 1;
                break;
            case PowerupVariant.MagicRate:
                gm.magicRateMultiplier += 1;
                break;
            case PowerupVariant.MagicSpeed:
                gm.magicSpeedMultiplier += 1;
                break;
            case PowerupVariant.MagicDamage:
                gm.magicDamageMultiplier += 1;
                break;
            case PowerupVariant.SwordSpeed:
                gm.swordSpeedMultiplier += 1;
                break;
            case PowerupVariant.SwordDamage:
                gm.swordDamageMultiplier += 1;
                break;
            case PowerupVariant.VineExtent:
                gm.vineExtentMultiplier += 1;
                break;
            case PowerupVariant.VineCooldown:
                gm.vineCooldownMultiplier += 1;
                break;
            case PowerupVariant.LevitateRange:
                gm.levitateRangeMultiplier += 1;
                break;
            case PowerupVariant.LevitateCooldown:
                gm.levitateCooldownMultiplier += 1;
                break;
            default:
                Debug.Log("Powerup not collected");
                break;
        }
    }

    /// <summary>
    /// Transitions to a display if the player has won the powerup or not
    /// </summary>
    private void TransitionResult()
    {
        highlightPage.gameObject.SetActive(false);
        resultPage.gameObject.SetActive(true);
    }

    /// <summary>
    /// Exits the bonus level
    /// </summary>
    public void LeaveButton()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameObject.Find("Player").GetComponent<OverworldPlayerController>().InUI = false;
        Destroy(gameObject);
    }
}

enum SelectedCompartment
{
    None,
    Left,
    Centre,
    Right
}
