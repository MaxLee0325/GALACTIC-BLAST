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

    // Dictionary storing players currently inside the area and their original speed
    private Dictionary<PlayerControl, float> playersInside = new Dictionary<PlayerControl, float>();

    // Dictionary tracking PlayerHearts components inside the area
    private Dictionary<PlayerHearts, PlayerHearts> playerHearts = new Dictionary<PlayerHearts, PlayerHearts>();

    // Tracks which players are currently stunned so you don't stun twice
    private HashSet<PlayerControl> currentlyStunned = new HashSet<PlayerControl>();
    
    private Coroutine damageCoroutine;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        // Start damage loop that repeatedly applies damage to all players standing on tile
        damageCoroutine = StartCoroutine(DamageLoop());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControl pc = other.GetComponent<PlayerControl>();
            PlayerHearts ph = other.GetComponent<PlayerHearts>();

            // Add player to active electrified zone tracking
            if (pc != null && !playersInside.ContainsKey(pc))
            {
                Debug.Log($"Player triggered electrified water at {transform.position}");
                playersInside.Add(pc, pc.speed);
                if (ph != null) playerHearts[ph] = ph;

                // Apply stun only once on initial entry
                if (!currentlyStunned.Contains(pc))
                {
                    currentlyStunned.Add(pc);
                    pc.Stun(stunDuration, stunEffectPrefab); 
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
                playersInside.Remove(pc);
                currentlyStunned.Remove(pc);
            }
            if (ph != null && playerHearts.ContainsKey(ph))
            {
                playerHearts.Remove(ph);
            }
        }
    }

    // Loop that continues damaging all players standing on electrified water
    IEnumerator DamageLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(damageInterval);

            // Apply damage to each player still inside the area
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
        // Clean up all tracking dictionaries when the effect disappears
        playersInside.Clear();
        playerHearts.Clear();
        currentlyStunned.Clear();
    }
}
