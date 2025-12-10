using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WetGround : MonoBehaviour
{
    [Header("Slow Settings")]
    public float slowMultiplier = 0.5f;   // reduce to 50%
    public float lifeTime = 5f;
    
    [Header("Detection Settings")]
    public float detectionRadius = 1.5f;  // How far to check for players (1.5 = 3 tiles)

    // Track players and their original speed
    private Dictionary<PlayerControl, float> playersInside = new Dictionary<PlayerControl, float>();

     Dictionary<NavMeshAgent, float> enemiesInside = new Dictionary<NavMeshAgent, float>();
     private List<EnemyHealth> enemiesToHit = new List<EnemyHealth>();

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        StartCoroutine(BurnLoop());
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

        if (other.CompareTag("FireEnemy"))
        {
            var e =other.GetComponent<NavMeshAgent>();
            var eHealth = other.GetComponent<EnemyHealth>();
            if (e != null && !enemiesInside.ContainsKey(e) && eHealth != null && !enemiesToHit.Contains(eHealth))
            {
                enemiesInside.Add(e, e.speed);
                e.speed *= slowMultiplier;
                enemiesToHit.Add(eHealth);
            }
        }
        if (other.CompareTag("ElectricEnemy"))
        {
            var e = other.GetComponent<NavMeshAgent>();
            if (e != null && !enemiesInside.ContainsKey(e))
            {
                enemiesInside.Add(e, e.speed);
                e.speed *= slowMultiplier;
            }
        }
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

        if (other.CompareTag("FireEnemy"))
        {
            var e =other.GetComponent<NavMeshAgent>();
            var eHealth = other.GetComponent<EnemyHealth>();
            if (e != null && enemiesInside.ContainsKey(e) && eHealth != null && enemiesToHit.Contains(eHealth))
            {
                enemiesInside.Remove(e);
                enemiesToHit.Remove(eHealth);
            }
        }
        if (other.CompareTag("ElectricEnemy"))
        {
            var e = other.GetComponent<NavMeshAgent>();
            if (e != null && !enemiesInside.ContainsKey(e))
            {
                enemiesInside.Remove(e);
            }
        }
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
        
        
         foreach (var kvp in enemiesInside)
         {
             if (kvp.Key != null)
                 kvp.Key.speed = kvp.Value;
         }
         enemiesInside.Clear();
    }
    
    private IEnumerator BurnLoop()
    {
        while (true)
        {
            foreach (var e in enemiesInside)
                if (e.Key != null)
                {
                    e.Key.speed = e.Value;
                    Debug.Log("enemySlow");
                }

            yield return null; // ← THIS prevents the crash
        }
    }

}