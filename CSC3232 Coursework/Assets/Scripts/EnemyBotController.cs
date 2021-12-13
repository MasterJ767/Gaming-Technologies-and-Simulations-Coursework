using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

/// <summary>
/// Controls every aspect of enemies droids
/// </summary>
public class EnemyBotController : MonoBehaviour
{
    [Header("Model")] 
    public Rigidbody rb;
    public GameObject model;
    public GameObject head;
    public GameObject deathEffect;
    public Canvas Ui;
    public GameObject highlight;
    private bool isHighlighted;
    
    [Header("Ground Check Attributes")] 
    public Transform groundCheck;
    public float groundDistance = 0.04f;
    public LayerMask groundMask;
    
    [Header("Navigation Mesh Settings")]
    public float lookRadius = 20f;
    public float maxSpeed = 6f;
    public float acceleration = 2.25f;
    public Transform target;
    public Transform headTarget;
    [NonSerialized]
    public NavMeshAgent agent;
    [NonSerialized] 
    public bool skipGroundCheck;
    private bool isGrounded;
    [NonSerialized] 
    public bool updraftNextFrame;
    private float updraftForce = 21.7006f;
    
    [Header("Enemy Health Attributes")]
    public Slider slider;
    public Image healthFill;
    public Gradient healthColour;
    public float maxHealth;
    [NonSerialized] 
    public float currentHealth;
    private bool isDead = false;
    
    [Header("Attack Settings")]
    public GameObject attackFlash;
    public float baseAttackDamage;
    public float baseAttackRange;
    private bool canAttack = true;

    [Header("Flocking Settings")] 
    public Flock flock;
    
    private bool recordFall = false;
    private float maxY;

    private void Start()
    {
        SetSliderMaxHealth();
        
        // Set NavMeshAgent attributes
        agent = GetComponent<NavMeshAgent>();
        agent.acceleration = acceleration;
        agent.speed = maxSpeed;
    }
    
    private void OnDrawGizmos()
    {
        if (!isDead)
        {
            // Show Vision Radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, lookRadius);
            
            // Show Shooting Range
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(head.transform.position, head.transform.position + (transform.forward * baseAttackRange));
        }
    }
    
    private void FixedUpdate()
    {
        // Apply levitate force if the player has used the levitate ability on the enemy
        if (updraftNextFrame)
        {
            skipGroundCheck = true;
            agent.enabled = false;
            rb.AddForce(new Vector3(0, updraftForce,0), ForceMode.Impulse);
            updraftNextFrame = false;
        }
    }

    private void Update()
    {
        // Check if Dead
        DeathCheck();

        if (!isDead)
        {
            // Check if Grounded
            GroundCheck();

            // Check if the NavMeshAgent should be active
            CheckNavMeshActivity();

            // Check if the enemy is at the apex of a fall
            CheckFalling();

            // Check if the enemy has just finished falling
            CheckLanding();

            // Take action
            MakeDecision();
        }
    }

    /// <summary>
    /// Checks if the enemy has died and if they have, runs the death co-routine
    /// </summary>
    private void DeathCheck()
    {
        if (currentHealth <= 0)
        {
            isDead = true;
            StartCoroutine(Death());
        }
    }
    
    /// <summary>
    /// Checks if the enemy is touching the ground
    /// </summary>
    private void GroundCheck()
    {
        isGrounded = Physics.CheckCapsule(groundCheck.position, groundCheck.position - new Vector3(0, groundDistance, 0), 0.4f, groundMask);
    }

    /// <summary>
    /// Checks if the enemy should have the NavMeshAgent enabled or not. The NavMeshAgent has to be disabled in order for the player's
    /// Levitate ability to affect the enemy as the ability pulls the enemy off of the NavMesh. 
    /// </summary>
    private void CheckNavMeshActivity()
    {
        if (!skipGroundCheck)
        {
            if (isGrounded)
            {
                agent.enabled = true;
            }
            else
            {
                agent.enabled = false;
                transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, 0, transform.rotation.z));
            }
        }
        else
        {
            if (isGrounded)
            {
                skipGroundCheck = false;
            }
        }
    }
    
    /// <summary>
    /// Begins recording information about the enemy at the apex of a fall
    /// </summary>
    private void CheckFalling()
    {
        if (!isGrounded && !recordFall && rb.velocity.y <= 0)
        {
            recordFall = true;
            maxY = transform.position.y;
        }
    }

    /// <summary>
    /// Calculates fall damage
    /// </summary>
    private void CheckLanding()
    {
        if (isGrounded && recordFall)
        {
            recordFall = false;
            float difference = maxY - transform.position.y;
            float fallDamage = (difference - 3) / 2;
            if (fallDamage > 0)
            {
                TakeDamage(fallDamage);
            }
        }
    }

    /// <summary>
    /// Determines what the enemy should be doing while on the NavMesh
    /// </summary>
    private void MakeDecision()
    {
        foreach (GameObject boid in flock.droids)
        {
            if (boid != gameObject)
            {
                if (Vector3.Distance(boid.transform.position, transform.position) < flock.separationDistance)
                {
                    Vector3 direction = boid.transform.position - transform.position;
                    agent.Warp(transform.position - (direction.normalized * agent.speed * Time.deltaTime));
                }
            }
        }
        
        // If not on NavMesh enemy cannot pathfind
        if (agent.enabled)
        {
            float distance = Vector3.Distance(target.position, transform.position);
            
            // If player in the enemy looking distance, pursue player
            if (distance <= lookRadius)
            {
                agent.SetDestination(target.position);
                
                // If player is within the enemy stopping distance, turn to face player and shoot
                if (distance <= agent.stoppingDistance)
                {
                    FaceTarget();
                    if (canAttack)
                    {
                        Attack();
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Rotates the enemy, their head, their ui and their gun towards the player
    /// </summary>
    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        Vector3 headDirection = (headTarget.position - head.transform.position).normalized;
        Quaternion headLookRotation = Quaternion.LookRotation(headDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
        Quaternion facePlayer = Quaternion.Slerp(head.transform.rotation, headLookRotation, Time.deltaTime * 5);
        head.transform.rotation = facePlayer;
        Ui.transform.rotation = facePlayer;
    }

    /// <summary>
    /// Casts rays striaght forward from the enemy, causing damage to a player if it intersects with a player
    /// </summary>
    private void Attack()
    {
        GameObject effect = Instantiate(attackFlash, transform.position + (transform.forward * 0.5f), transform.rotation);
        Destroy(effect, 1f);
        if (Vector3.Distance(transform.position, target.transform.position) <= 3f)
        {
            target.GetComponent<PlayerController>().TakeDamage(baseAttackDamage);
        }
        StartCoroutine(AttackCooldown());
    }
    
    /// <summary>
    /// External function for changing the 'levitate' highlight aura on an enemy
    /// </summary>
    /// <param name="value"></param>
    public void SetHighlight(bool value)
    {
        isHighlighted = value;
        ChangeHighlight();
    }
    
    /// <summary>
    /// Internal function for changing the 'levitate' highlight aura on an enemy
    /// </summary>
    private void ChangeHighlight()
    {
        if (isHighlighted)
        {
            highlight.gameObject.SetActive(true);
        }
        else
        {
            highlight.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Sets Health and Health Slider to maximum
    /// </summary>
    public void SetSliderMaxHealth()
    {
        slider.maxValue = maxHealth;
        currentHealth = maxHealth;
        SetSliderHealth();
    }

    /// <summary>
    /// Sets Health and Health slider to the value of the enemy's current health
    /// </summary>
    public void SetSliderHealth()
    {
        slider.value = currentHealth;
        healthFill.color = healthColour.Evaluate(slider.normalizedValue);
    }
    
    /// <summary>
    /// Deals damage of a specified value to the enemy
    /// </summary>
    /// <param name="value">The damage taken</param>
    public void TakeDamage(float value)
    {
        currentHealth -= value;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }
        SetSliderHealth();
    }

    /// <summary>
    /// Heals the enemy by a specified amount
    /// </summary>
    /// <param name="value"></param>
    public void Heal(float value)
    {
        currentHealth += value;
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        SetSliderHealth();
    }

    /// <summary>
    /// Adds a down time between enemy shots
    /// </summary>
    /// <returns></returns>
    private IEnumerator AttackCooldown()
    {
        canAttack = false;

        yield return new WaitForSeconds(0.3f);
        
        canAttack = true;
    }
    
    /// <summary>
    /// Disables all visible elements of the enemy then instantiates a blood particle effect before disabling the entire enemy
    /// </summary>
    /// <returns></returns>
    IEnumerator Death()
    {
        model.gameObject.SetActive(false);
        Ui.gameObject.SetActive(false);
        
        Instantiate(deathEffect, transform.position - new Vector3( 0, 1f, 0), transform.rotation, transform);

        yield return new WaitForSeconds(0.5f);
        
        gameObject.SetActive(false);
    }
}
