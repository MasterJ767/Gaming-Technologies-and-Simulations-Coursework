using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ice : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // If player touches ice
        if (other.gameObject.layer == 6)
        {
            other.gameObject.GetComponent<PlayerController>().onIce = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // If Enemy touches ice
        if (other.gameObject.layer == 8)
        {
            if (!other.gameObject.CompareTag("Head"))
            {
                GameObject enemy = other.gameObject;
                EnemyController ec = enemy.GetComponent<EnemyController>();
                if (ec.rb.velocity.magnitude > 0.01f)
                {
                    ec.rb.AddForce(enemy.transform.forward.normalized * 0.0075f, ForceMode.Impulse);
                }
                
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If player touches ice
        if (other.gameObject.layer == 6)
        {
            other.gameObject.GetComponent<PlayerController>().onIce = false;
        }
    }
}
