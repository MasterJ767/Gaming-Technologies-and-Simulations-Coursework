using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A class for managing variables across scenes as well as some scene transitions during setup
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public UnityEngine.Object mainMenuScene;  
    
    public int healthMultiplier = 0;
    public int magicRangeMultiplier = 0;
    public int magicRateMultiplier = 0;
    public int magicSpeedMultiplier = 0;
    public int magicDamageMultiplier = 0;
    public int swordSpeedMultiplier = 0;
    public int swordDamageMultiplier = 0;
    public int vineExtentMultiplier = 0;
    public int vineCooldownMultiplier = 0;
    public int levitateRangeMultiplier = 0;
    public int levitateCooldownMultiplier = 0;

    [NonSerialized] public Vector3 playerPosition = Vector3.zero;
    [NonSerialized] public State currentState = State.Default;

    public bool level1Complete = false;
    public bool level2Complete = false;
    public bool level3Complete = false;
    public bool level4Complete = false;
    public bool level5Complete = false;
    public bool bonus1Complete = false;
    public bool bonus2Complete = false;
    public bool bonus3Complete = false;
    public bool bonus4Complete = false; 
    private bool runCompleteUpdate = false;
    private string previousScene;
    
    private void Awake() {
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
    }

    private void Update()
    {
        if (currentState == State.Default)
        {
            SetParameters(State.Menu, SceneManager.GetActiveScene().name, false, false);
            SceneManager.LoadSceneAsync(mainMenuScene.name, LoadSceneMode.Single);
        }

        if (runCompleteUpdate && currentState == State.Overworld && (previousScene != SceneManager.GetActiveScene().name))
        {
            Transform levels = GameObject.Find("World").transform.Find("Events");

            if (level5Complete)
            {
                levels.Find("Level 5").GetComponent<Level>().Completed();
            }

            if (level4Complete)
            {
                levels.Find("Level 5").GetComponent<Level>().Unlock();
                levels.Find("Bonus 4").GetComponent<Level>().Unlock();
                levels.Find("Level 4").GetComponent<Level>().Completed();
            }
            
            if (level3Complete)
            {
                levels.Find("Level 4").GetComponent<Level>().Unlock();
                levels.Find("Bonus 3").GetComponent<Level>().Unlock();
                levels.Find("Level 3").GetComponent<Level>().Completed();
            }
            
            if (level2Complete)
            {
                levels.Find("Level 3").GetComponent<Level>().Unlock();
                levels.Find("Bonus 2").GetComponent<Level>().Unlock();
                levels.Find("Level 2").GetComponent<Level>().Completed();
            }
            
            if (level1Complete)
            {
                levels.Find("Level 2").GetComponent<Level>().Unlock();
                levels.Find("Bonus 1").GetComponent<Level>().Unlock();
                levels.Find("Level 1").GetComponent<Level>().Completed();
            }

            if (bonus4Complete)
            {
                levels.Find("Bonus 4").GetComponent<Level>().Completed();
            }
            
            if (bonus3Complete)
            {
                levels.Find("Bonus 3").GetComponent<Level>().Completed();
            }
            
            if (bonus2Complete)
            {
                levels.Find("Bonus 2").GetComponent<Level>().Completed();
            }
            
            if (bonus1Complete)
            {
                levels.Find("Bonus 1").GetComponent<Level>().Completed();
            }
            
            runCompleteUpdate = false;
        }
    }

    public void Pause(bool pause)
    {
        if (pause)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void SetParameters(State newState, Vector3 nextPlayerPosition, string currentSceneName, bool enteringFromMain)
    {
        currentState = newState;
        playerPosition = nextPlayerPosition;

        if (enteringFromMain)
        {
            runCompleteUpdate = true;
        }
        
        previousScene = currentSceneName;
    }

    public void SetParameters(State newState, string sceneCompletedName, bool levelComplete, bool exitingLevel)
    {
        currentState = newState;
        if (levelComplete)
        {
            switch (sceneCompletedName)
            {
                case "Level1":
                    level1Complete = true;
                    break;
                case "Level2":
                    level2Complete = true;
                    break;
                case "Level3":
                    level3Complete = true;
                    break;
                case "Level4":
                    level4Complete = true;
                    break;
                case "Level5":
                    level5Complete = true;
                    break;
            }
        }

        if (exitingLevel)
        {
            runCompleteUpdate = true;
        }
        
        previousScene = sceneCompletedName;
    }
}

public enum State
{
    Default,
    Menu,
    Overworld,
    Level1,
    Level2,
    Level3,
    Level4,
    Level5
}