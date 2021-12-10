using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Defines button functions for the main menu
/// </summary>
public class MainMenu : MonoBehaviour
{
    public UnityEngine.Object overworldScene;
    public GameObject mainPanel;
    public GameObject settingsPanel;
    
    public void PlayGame()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().SetParameters(State.Overworld, new Vector3(-520, 40, -1), SceneManager.GetActiveScene().name, true);
        SceneManager.LoadSceneAsync(overworldScene.name, LoadSceneMode.Single);
    }

    public void SettingsOn()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void SettingsOff()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
