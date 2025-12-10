using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectrifiedWaterGround : MonoBehaviour
{
    [Header("Effect Settings")]
    public float stunDuration = 1f;
    public float damageInterval = 1f;
    public int damageAmount = 1;
    public float lifeTime = 4f;

    [Header("Detection Settings")]
    public float detectionRadius = 0.5f;
    public GameObject stunEffectPrefab;

    // Track players and their original speed
    private Dictionary<PlayerControl, float> playersInside = new Dictionary<PlayerControl, float>();
    private List<EnemyHealth> enemiesInside = new List<EnemyHealth>();
    private Dictionary<PlayerHearts, PlayerHearts> playerHearts = new Dictionary<PlayerHearts, PlayerHearts>();
    private HashSet<PlayerControl> currentlyStunned = new HashSet<PlayerControl>();
    private Coroutine damageCoroutine;

    void Start()
    {
        Destroy(gameObject, lifeTime);
        damageCoroutine = StartCoroutine(DamageLoop());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControl pc = other.GetComponent<PlayerControl>();
            PlayerHearts ph = other.GetComponent<PlayerHearts>();
            if (pc != null && !playersInside.ContainsKey(pc))
            {
                Debug.Log($"Player triggered electrified water at {transform.position}");
                playersInside.Add(pc, pc.speed);
                if (ph != null) playerHearts[ph] = ph;
                if (!currentlyStunned.Contains(pc))
                {
                    currentlyStunned.Add(pc);
                    pc.Stun(stunDuration, stunEffectPrefab); // Player handles their own stun
                }
            }
        }

        if (other.CompareTag("WaterEnemy") || other.CompareTag("FireEnemy") || other.CompareTag("ElectricEnemy"))
        {
            var enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemiesInside.Add(enemy);
            }
        }
            
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControl pc = other.GetComponent<PlayerControl>();
            PlayerHearts ph = other.GetComponent<PlayerHearts>();
            if (pc != null && playersInside.ContainsKey(pc))
            {
                Debug.Log($"Player exited electrified water at {transform.position}");
                playersInside.Remove(pc);
                currentlyStunned.Remove(pc);
            }
            if (ph != null && playerHearts.ContainsKey(ph))
            {
                playerHearts.Remove(ph);
            }
        }
        
        if (other.CompareTag("WaterEnemy") || other.CompareTag("FireEnemy") || other.CompareTag("ElectricEnemy"))
        {
            var enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemiesInside.Remove(enemy);
            }
        }
    }

    IEnumerator DamageLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(damageInterval);

            foreach (var kvp in playerHearts)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.TakeDamage(damageAmount);
                    Debug.Log($"Electric damage dealt to player");
                }
            }

            foreach (var kvp in enemiesInside)
            {
                if (kvp != null)
                {
                    kvp.TakeDamage(damageAmount);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up dictionaries
        playersInside.Clear();
        playerHearts.Clear();
        currentlyStunned.Clear();
    }
}
