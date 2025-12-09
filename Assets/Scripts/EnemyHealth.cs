using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 2;
    private int currentHealth;

    [Header("Hit Stun")]
    [SerializeField] private float hitStunDuration = 0.5f;
    private bool isStunned = false;

    [Header("Optional")]
    public bool destroyOnDeath = true;
    public float deathDelay = 2f;

    [Header("References")]
    public Animator anim;
    public GameObject deathEffect;
    
    private NavMeshAgent agent;
    public MonoBehaviour aiScript;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        if (anim == null) anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        // Get AI script (try all three types)
        
    }

    public void TakeDamage(int amount = 1)
    {
        if (isStunned || isDead) return;
        
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth == 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitStun());
        }
    }

    IEnumerator HitStun()
    {
        isStunned = true;

        // Disable AI script temporarily
        if (aiScript != null)
        {
            aiScript.enabled = false;
        }

        // Stop movement
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }

        // Play hit animation
        if (anim)
        {
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsHit", true);
        }

        yield return new WaitForSeconds(hitStunDuration);

        anim.SetBool("IsHit", false);
        // Re-enable AI
        if (aiScript != null && !isDead)
        {
            aiScript.enabled = true;
        }

        // Resume movement
        if (agent != null && agent.enabled && !isDead)
        {
            agent.isStopped = false;
        }

        isStunned = false;
    }

    void Die()
    {
        isDead = true;
        
        // Disable AI immediately
        if (aiScript != null)
        {
            aiScript.enabled = false;
        }

        // Stop movement
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        // Play death animation
        if (anim)
        {
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsAttacking", false);
            anim.SetBool("Dead", true);
        }

        // Spawn death effect
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Remove collision
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        if (destroyOnDeath)
            Destroy(gameObject, deathDelay);
    }
    
    public bool IsStunned => isStunned;
    public bool IsDead => isDead;
}