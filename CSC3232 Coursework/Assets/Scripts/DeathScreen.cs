using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Provides functions for the buttons present on the death screen, when the player dies in-level
/// </summary>
public class DeathScreen : MonoBehaviour
{
    public UnityEngine.Object levelScene;
    public UnityEngine.Object overworldScene;
    
    /// <summary>
    /// Restarts the current level, this means that any powerups acquired in the level are removed
    /// </summary>
    public void Respawn()
    {
        SceneManager.LoadSceneAsync(levelScene.name, LoadSceneMode.Single);
    }
    
    /// <summary>
    /// Returns the player to the overworld
    /// </summary>
    public void QuitToOverworld()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().SetParameters(State.Overworld, SceneManager.GetActiveScene().name, false, true);
        SceneManager.LoadSceneAsync(overworldScene.name, LoadSceneMode.Single);
    }
}
