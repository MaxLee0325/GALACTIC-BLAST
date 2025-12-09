using System.Collections.Generic;
using UnityEngine;

public class WetGround : MonoBehaviour
{
    [Header("Slow Settings")]
    public float slowMultiplier = 0.5f;   // reduce to 50%
    public float lifeTime = 5f;
    
    [Header("Detection Settings")]
    public float detectionRadius = 1.5f;  // How far to check for players (1.5 = 3 tiles)

    // Track players and their original speed
    private Dictionary<PlayerControl, float> playersInside = new Dictionary<PlayerControl, float>();

    //TODO: wait for enemy implementation
    // private Dictionary<Enemy, float> enemiesInside = new Dictionary<Enemy, float>();

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Use Update to continuously check for players in radius (not just OnTriggerEnter)
    private void Update()
    {
        // Check for players within detection radius
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach (var col in nearbyColliders)
        {
            if (col.CompareTag("Player"))
            {
                PlayerControl pc = col.GetComponent<PlayerControl>();
                if (pc != null && !playersInside.ContainsKey(pc) && !pc.isSlowed)
                {
                    // Player entered water area
                    pc.isSlowed = true;
                    playersInside.Add(pc, pc.originalSpeed);
                    pc.speed *= slowMultiplier;
                    Debug.Log("Player on water!");
                }
            }
        }
        
        // Check if players left the water area
        List<PlayerControl> playersToRemove = new List<PlayerControl>();
        foreach (var kvp in playersInside)
        {
            if (kvp.Key != null)
            {
                float distance = Vector3.Distance(transform.position, kvp.Key.transform.position);
                if (distance > detectionRadius)
                {
                    // Player left water area
                    kvp.Key.speed = kvp.Value;  // restore original speed
                    kvp.Key.isSlowed = false;
                    playersToRemove.Add(kvp.Key);
                }
            }
        }
        
        // Remove players who left
        foreach (var pc in playersToRemove)
        {
            playersInside.Remove(pc);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player
        if (other.CompareTag("Player"))
        {
            PlayerControl pc = other.GetComponent<PlayerControl>();
            if (pc != null && !playersInside.ContainsKey(pc) && !pc.isSlowed)
            {
                pc.isSlowed = true;
                playersInside.Add(pc, pc.originalSpeed);   // store original speed
                pc.speed *= slowMultiplier;        // apply slow
                Debug.Log("Player on water!");
            }
        }

        //TODO: wait for enemy implementation
        // if (other.CompareTag("Enemy"))
        // {
        //     Enemy e = other.GetComponent<Enemy>();
        //     if (e != null && !enemiesInside.ContainsKey(e))
        //     {
        //         enemiesInside.Add(e, e.speed);  // store original speed
        //         e.speed *= slowMultiplier;      // apply slow
        //     }
        // }
    }

    private void OnTriggerExit(Collider other)
    {
        // Player
        if (other.CompareTag("Player"))
        {
            PlayerControl pc = other.GetComponent<PlayerControl>();
            if (pc != null && playersInside.ContainsKey(pc))
            {
                pc.speed = playersInside[pc];  // restore original speed
                playersInside.Remove(pc);
                pc.isSlowed = false;
            }
        }

        //TODO: wait for enemy implementation
        // if (other.CompareTag("Enemy"))
        // {
        //     Enemy e = other.GetComponent<Enemy>();
        //     if (e != null && enemiesInside.ContainsKey(e))
        //     {
        //         e.speed = enemiesInside[e];  // restore original speed
        //         enemiesInside.Remove(e);
        //     }
        // }
    }

    private void OnDestroy()
    {
        // Restore speed for any players still inside
        foreach (var kvp in playersInside)
        {
            if (kvp.Key != null)
            {
                kvp.Key.speed = kvp.Value;
                kvp.Key.isSlowed = false;
            }
        }
        playersInside.Clear();

        //TODO: wait for enemy implementation
        // foreach (var kvp in enemiesInside)
        // {
        //     if (kvp.Key != null)
        //         kvp.Key.speed = kvp.Value;
        // }
        // enemiesInside.Clear();
    }
}