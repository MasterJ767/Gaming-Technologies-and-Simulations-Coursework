using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentDamage : MonoBehaviour
{
    public float damage;
    
    private void OnTriggerStay(Collider other)
    {
        // If Player walks into Electrified Wall, Acid Pool etc.
        if (other.gameObject.layer == 6)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            player.TakeDamage(damage * Time.fixedDeltaTime);
        }
        // If Enemy walks into Electrified Wall, Acid Pool, etc.
        else if (other.gameObject.layer == 8)
        {
            EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
            if (gameObject.name == "Acid")
            {
                if (!other.gameObject.CompareTag("Head"))
                {
                    enemy.TakeDamage(enemy.maxHealth + 1);
                }
            }
            else
            {
                if (!other.gameObject.CompareTag("Head"))
                {
                    enemy.TakeDamage(damage * Time.fixedDeltaTime);
                }
            }
        }
    }
}
