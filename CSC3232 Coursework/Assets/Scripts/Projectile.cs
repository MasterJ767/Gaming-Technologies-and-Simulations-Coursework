using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages magic projectiles launched by the player
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Attributes")]
    public Rigidbody rb;
    public GameObject impactEffect;
    public GameObject bonusEffect;
    public GameObject graphic;
    private PlayerController pc;
    private bool initialised = false;

    private Vector3 startPosition;

    private void Update()
    {
        // Destroy if the projectile has surpassed its maximum range
        if (initialised && Vector3.Distance(startPosition, transform.position) > pc.GetMagicRange())
        {
            StartCoroutine(Decay());
        }
    }

    private void OnDrawGizmos()
    {
        if (initialised)
        {
            // Draw range of projectile
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPosition, startPosition + rb.velocity * (pc.GetMagicRange() / rb.velocity.magnitude));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Environment") || other.CompareTag("Interactable") || other.CompareTag("Ice"))
        {
            StartCoroutine(Decay());
        }
        else if (other.CompareTag("Head"))
        {
            StartCoroutine(DecayEnemy(other, true));
        }
        else if (other.CompareTag("Enemy"))
        {
            StartCoroutine(DecayEnemy(other, false));
        }
    }

    /// <summary>
    /// Take inputs form a player controller in order to determine projectile attributes
    /// </summary>
    /// <param name="playerController">A player controller from the player present in the level</param>
    public void Initialise(PlayerController playerController)
    {
        pc = playerController;
        rb.velocity = transform.forward * pc.GetMagicProjectileSpeed();
        startPosition = gameObject.transform.position;
        initialised = true;
    }

    /// <summary>
    /// Destroy projectile on collision with environment
    /// </summary>
    /// <returns></returns>
    IEnumerator Decay()
    {
        rb.velocity = Vector3.zero;
        graphic.SetActive(false);
        Instantiate(impactEffect, transform.position, transform.rotation, transform);
        
        yield return new WaitForSeconds(0.5f);
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Destory porjectile on collision with enemy, deal calculated damage, taking into account if it hit the enemy's head or not
    /// </summary>
    /// <param name="other">Enemy's collider</param>
    /// <param name="head">True if the collider is the enemy's head, false otherwise</param>
    /// <returns></returns>
    IEnumerator DecayEnemy(Collider other, bool head)
    {
        rb.velocity = Vector3.zero;
        graphic.SetActive(false);

        if (head)
        {
            StartCoroutine(HeadShot(other));
        }
        
        Instantiate(impactEffect, transform.position, transform.rotation, transform);

        EnemyController enemy;
        
        if (head)
        {
            enemy = other.gameObject.transform.parent.parent.parent.parent.parent.parent.parent.parent.gameObject.GetComponent<EnemyController>();
        }
        else
        {
            enemy = other.gameObject.GetComponent<EnemyController>();
        }

        enemy.TakeDamage(pc.GetMagicDamage());
        
        yield return new WaitForSeconds(2.5f);
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Display Headshot, particle effect
    /// </summary>
    /// <param name="other">Enemy's collider</param>
    /// <returns></returns>
    IEnumerator HeadShot(Collider other)
    {
        bonusEffect.SetActive(true);
        bonusEffect.GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(0.5f);
    }
}
