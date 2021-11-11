using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the bobbing motion of powerup syringes
/// </summary>
public class Oscillator : MonoBehaviour
{
    public GameObject gravityTarget;
    public float initialSpeed = 1;
    public float gravityScale = 1;

    public GameObject player;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.up * initialSpeed;
    }

    private void FixedUpdate()
    {
        // Bob up and down about a fixed point
        Vector3 force = (gravityTarget.transform.position - transform.position).normalized * gravityScale;
        rb.AddForce(force, ForceMode.Force);
        
        // Rotate to face the player
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 5);
    }
    
}
