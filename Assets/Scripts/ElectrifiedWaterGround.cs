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
    private Dictionary<PlayerHearts, PlayerHearts> playerHearts = new Dictionary<PlayerHearts, PlayerHearts>();
    private HashSet<PlayerControl> currentlyStunned = new HashSet<PlayerControl>();

    private Coroutine damageCoroutine;

    void Start()
    {
        Destroy(gameObject, lifeTime);
        damageCoroutine = StartCoroutine(DamageLoop());
        
        Debug.Log($"ElectrifiedWater created at {transform.position} with radius {detectionRadius}");
    }

    // Continuous detection using sphere overlap
    private void Update()
    {
        // Check for players within detection radius
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        // Track who should be in the area
        HashSet<PlayerControl> playersInRange = new HashSet<PlayerControl>();
        HashSet<PlayerHearts> heartsInRange = new HashSet<PlayerHearts>();
        
        foreach (var col in nearbyColliders)
        {
            if (col.CompareTag("Player"))
            {
                PlayerControl pc = col.GetComponent<PlayerControl>();
                PlayerHearts ph = col.GetComponent<PlayerHearts>();
                
                if (pc != null)
                {
                    playersInRange.Add(pc);
                    
                    // Player entered electric field
                    if (!playersInside.ContainsKey(pc))
                    {
                        Debug.Log($"Player entered electrified water at {transform.position}");
                        playersInside.Add(pc, pc.speed);
                        
                        if (!currentlyStunned.Contains(pc))
                        {
                            currentlyStunned.Add(pc);
                            StartCoroutine(StunPlayer(pc));
                        }
                    }
                }
                
                if (ph != null)
                {
                    heartsInRange.Add(ph);
                    if (!playerHearts.ContainsKey(ph))
                    {
                        playerHearts[ph] = ph;
                    }
                }
            }
        }
        
        // Remove players who left the area
        List<PlayerControl> playersToRemove = new List<PlayerControl>();
        foreach (var kvp in playersInside)
        {
            if (kvp.Key != null && !playersInRange.Contains(kvp.Key))
            {
                Debug.Log($"Player left electrified water at {transform.position}");
                kvp.Key.speed = kvp.Value;  // Restore speed
                playersToRemove.Add(kvp.Key);
                currentlyStunned.Remove(kvp.Key);
            }
        }
        
        List<PlayerHearts> heartsToRemove = new List<PlayerHearts>();
        foreach (var kvp in playerHearts)
        {
            if (kvp.Key != null && !heartsInRange.Contains(kvp.Key))
            {
                heartsToRemove.Add(kvp.Key);
            }
        }
        
        // Clean up
        foreach (var pc in playersToRemove)
        {
            playersInside.Remove(pc);
        }
        foreach (var ph in heartsToRemove)
        {
            playerHearts.Remove(ph);
        }
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
                    StartCoroutine(StunPlayer(pc));
                }
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
                pc.speed = playersInside[pc];
                playersInside.Remove(pc);
                currentlyStunned.Remove(pc);
            }

            if (ph != null && playerHearts.ContainsKey(ph))
            {
                playerHearts.Remove(ph);
            }
        }
    }

    IEnumerator StunPlayer(PlayerControl pc)
    {
        Debug.Log($"Stunning player for {stunDuration} seconds");
        
        if (stunEffectPrefab)
        {
            GameObject effect = Instantiate(
                stunEffectPrefab,
                new Vector3(pc.transform.position.x, pc.transform.position.y + 2f, pc.transform.position.z),
                Quaternion.identity,
                pc.transform
            );

            Destroy(effect, stunDuration);
        }

        float originalSpeed = pc.speed;
        pc.speed = 0f;
        
        yield return new WaitForSeconds(stunDuration);
        
        // Restore only if still inside electrified water
        if (playersInside.ContainsKey(pc))
        {
            pc.speed = originalSpeed;
        }
        
        currentlyStunned.Remove(pc);
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
        }
    }

    private void OnDestroy()
    {
        // Restore player speeds if still stunned
        foreach (var kvp in playersInside)
        {
            if (kvp.Key != null)
            {
                kvp.Key.speed = kvp.Value;
                Debug.Log($"Restored speed to {kvp.Value}");
            }
        }

        playersInside.Clear();
        playerHearts.Clear();
        currentlyStunned.Clear();
    }
    
    // Visual debug in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}