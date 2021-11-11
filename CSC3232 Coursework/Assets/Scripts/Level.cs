using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls level interactions in the overworld
/// </summary>
public class Level : MonoBehaviour
{
    public OverworldPlayerController pc;
    
    [Header("Level Attributes")] 
    public LevelType variant;
    public string levelName;
    public string prerequisiteName;
    public UnityEngine.Object scene;
    public GameObject bonus;
    public bool isUnlocked = false;
    public bool unlockImageSet = false;
    public bool isCompleted = false;
    public GameObject graphic;
    public GameObject description;
    public TextMeshProUGUI descriptionText;
    public Sprite lockedImage;
    public Sprite unlockedImage;
    
    [Header("Text Settings")]
    public float textSize = 4f;
    public Color textColor;
    public Color lightTextColor;
    public Font font;

    private bool canInteract;

    public void Start()
    {
        ApplyTextSettings(descriptionText, true);
        SetLockedText();
    }

    public void Update()
    {
        if (isUnlocked && !unlockImageSet)
        {
            Unlock();
        }

        if (canInteract && isUnlocked)
        {
            // Open scene if it is a regular level
            if (variant == LevelType.Level && Input.GetButtonDown("Interact"))
            {
                GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
                switch (levelName)
                {
                    case "Level 1":
                        gm.SetParameters(State.Level1, transform.position, SceneManager.GetActiveScene().name, false);
                        break;
                    case "Level 2":
                        gm.SetParameters(State.Level2, transform.position, SceneManager.GetActiveScene().name, false);
                        break;
                    case "Level 3":
                        gm.SetParameters(State.Level3, transform.position, SceneManager.GetActiveScene().name, false);
                        break;
                    case "Level 4":
                        gm.SetParameters(State.Level4, transform.position, SceneManager.GetActiveScene().name, false);
                        break;
                    case "Level 5":
                        gm.SetParameters(State.Level5, transform.position, SceneManager.GetActiveScene().name, false);
                        break;
                    default:
                        gm.SetParameters(State.Default, transform.position, SceneManager.GetActiveScene().name, false);
                        break;
                }
                SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Single);
            }
            // Open Monty Hall problem if it is a bonus level
            else if (variant == LevelType.Bonus && Input.GetButtonDown("Interact") && !isCompleted)
            {
                pc.InUI = true;
                Cursor.lockState = CursorLockMode.Confined;
                Completed();
                GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
                switch (levelName)
                {
                    case "Bonus 1":
                        gm.bonus1Complete = true;
                        break;
                    case "Bonus 2":
                        gm.bonus2Complete = true;
                        break;
                    case "Bonus 3":
                        gm.bonus3Complete = true;
                        break;
                    case "Bonus 4":
                        gm.bonus4Complete = true;
                        break;
                }
                Instantiate(bonus);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        graphic.SetActive(false);
        description.SetActive(true);
        canInteract = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        graphic.SetActive(true);
        description.SetActive(false);
        canInteract = false;
    }

    /// <summary>
    /// Change the state of an overworld level to unlocked
    /// </summary>
    public void Unlock()
    {
        isUnlocked = true;
        graphic.GetComponent<Image>().sprite = unlockedImage;
        SetUnlockedText();
        unlockImageSet = true;
    }

    /// <summary>
    /// Change the state of an overworld level to completed
    /// </summary>
    public void Completed()
    {
        isCompleted = true;

        if (variant == LevelType.Bonus)
        {
            graphic.GetComponent<Image>().sprite = lockedImage;
            SetCompletedText();
        }
        
        graphic.GetComponent<Image>().color = Color.yellow;
    }

    /// <summary>
    /// Set initial popup text
    /// </summary>
    private void SetLockedText()
    {
        string title = "<<< " + levelName + " >>>";
        string body = "Complete " + prerequisiteName + " in order to unlock this level.";
        descriptionText.text = title + "\n \n" + body;
    }
    
    /// <summary>
    /// Set popup text after having unlocked a level
    /// </summary>
    private void SetUnlockedText()
    {
        string title = "<<< " + levelName + " >>>";
        string body = "Press F to enter this level.";
        descriptionText.text = title + "\n \n" + body;
    }
    
    /// <summary>
    /// Set pop up text after having completed a level
    /// </summary>
    private void SetCompletedText()
    {
        string title = "<<< " + levelName + " >>>";
        string body = "Level has already been completed.";
        descriptionText.text = title + "\n \n" + body;
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
}

[Serializable]
public enum LevelType
{
    Level,
    Bonus
}
