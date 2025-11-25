using System.Collections.Generic;
using UnityEngine;

public class WetGround : MonoBehaviour
{
    [Header("Slow Settings")]
    public float slowMultiplier = 0.5f;   // reduce to 50%
    public float lifeTime = 3f;

    // Track players and their original speed
    private Dictionary<PlayerControl, float> playersInside = new Dictionary<PlayerControl, float>();

    //TODO: wait for enemy implementation
    // private Dictionary<Enemy, float> enemiesInside = new Dictionary<Enemy, float>();

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player
        if (other.CompareTag("Player"))
        {
            PlayerControl pc = other.GetComponent<PlayerControl>();
            if (pc != null && !playersInside.ContainsKey(pc))
            {
                playersInside.Add(pc, pc.moveSpeed);   // store original speed
                pc.moveSpeed *= slowMultiplier;        // apply slow
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
                pc.moveSpeed = playersInside[pc];  // restore original speed
                playersInside.Remove(pc);
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
                kvp.Key.moveSpeed = kvp.Value;
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
