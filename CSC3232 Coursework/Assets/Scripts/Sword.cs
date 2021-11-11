using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's sword
/// </summary>
public class Sword : MonoBehaviour
{
    public Animator animator;
    public GameObject bonusEffect;
    public GameObject player;
    
    private PlayerController pc;

    private void Awake()
    {
        pc = player.GetComponent<PlayerController>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            if (other.CompareTag("Psychic Box"))
            {
                other.gameObject.SetActive(false);
            }
            else if (other.CompareTag("Head"))
            {
                EnemyController ec = other.gameObject.transform.parent.parent.parent.parent.parent.parent.parent.parent
                    .gameObject.GetComponent<EnemyController>();
                StartCoroutine(HeadShot(other));
                ec.TakeDamage(pc.GetSwordDamage() * 1.5f);
            }
            else if (other.CompareTag("Enemy"))
            {
                EnemyController ec = other.gameObject.GetComponent<EnemyController>();
                ec.TakeDamage(pc.GetSwordDamage());
            }
        }
    }
    
    /// <summary>
    /// Display Headshot, particle effect
    /// </summary>
    /// <param name="other">Enemy's collider</param>
    /// <returns></returns>
    IEnumerator HeadShot(Collider other)
    {
        bonusEffect.transform.position = other.gameObject.transform.position;
        bonusEffect.SetActive(true);
        bonusEffect.GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(0.5f);
        
        bonusEffect.SetActive(false);
    }
}
