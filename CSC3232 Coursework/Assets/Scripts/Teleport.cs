using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the player's transition from a level back to the overworld
/// </summary>
public class Teleport : MonoBehaviour
{
    public UnityEngine.Object nextScene;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            PlayerController pc = GameObject.Find("Player").GetComponent<PlayerController>();
            GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
            TransferBoosts(gm, pc);
            gm.SetParameters(State.Overworld, SceneManager.GetActiveScene().name, true, true);
            SceneManager.LoadSceneAsync(nextScene.name, LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// Adds all of the powerups the player has collected in the level to the GameManager as they have successfully escaped
    /// </summary>
    /// <param name="gm">GameManager present in the level</param>
    /// <param name="pc">PlayerController from the player present in the level</param>
    private void TransferBoosts(GameManager gm, PlayerController pc)
    {
        gm.healthMultiplier += pc.healthMultiplier;
        gm.magicRangeMultiplier += pc.magicRangeMultiplier;
        gm.magicRateMultiplier += pc.magicRateMultiplier;
        gm.magicSpeedMultiplier += pc.magicSpeedMultiplier;
        gm.magicDamageMultiplier += pc.magicDamageMultiplier;
        gm.swordSpeedMultiplier += pc.swordSpeedMultiplier;
        gm.swordDamageMultiplier += pc.swordDamageMultiplier;
        gm.vineExtentMultiplier += pc.vineExtentMultiplier;
        gm.vineCooldownMultiplier += pc.vineCooldownMultiplier;
        gm.levitateRangeMultiplier += pc.levitateRangeMultiplier;
        gm.levitateCooldownMultiplier += pc.levitateCooldownMultiplier;
    }
}
