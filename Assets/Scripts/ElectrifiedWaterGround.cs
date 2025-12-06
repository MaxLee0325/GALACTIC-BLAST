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

    public GameObject stunEffectPrefab;   // 🔥 assign in Inspector

    // Track players and their original speed
    private Dictionary<PlayerControl, float> playersInside = new Dictionary<PlayerControl, float>();
    private Dictionary<PlayerHearts, PlayerHearts> playerHearts = new Dictionary<PlayerHearts, PlayerHearts>();

    //TODO: wait for enemy implementation
    // private Dictionary<Enemy, float> enemiesInside = new Dictionary<Enemy, float>();

    private Coroutine damageCoroutine;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        // Start the damage loop immediately
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
                playersInside.Add(pc, pc.speed);
                playerHearts[ph] = ph;

                StartCoroutine(StunPlayer(pc));
            }
        }

        //TODO: wait for enemy implementation
        // if (other.CompareTag("Enemy"))
        // {
        //     Enemy e = other.GetComponent<Enemy>();
        //     if (e != null && !enemiesInside.ContainsKey(e))
        //     {
        //         enemiesInside.Add(e, e.speed);
        //         StartCoroutine(StunEnemy(e));
        //     }
        // }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControl pc = other.GetComponent<PlayerControl>();
            PlayerHearts ph = other.GetComponent<PlayerHearts>();

            if (pc != null && playersInside.ContainsKey(pc))
            {
                pc.speed = playersInside[pc]; // restore speed just in case
                playersInside.Remove(pc);
            }

            if (ph != null && playerHearts.ContainsKey(ph))
            {
                playerHearts.Remove(ph);
            }
        }

        //TODO: wait for enemy implementation
        // if (other.CompareTag("Enemy"))
        // {
        //     Enemy e = other.GetComponent<Enemy>();
        //     if (e != null && enemiesInside.ContainsKey(e))
        //     {
        //         e.speed = enemiesInside[e];
        //         enemiesInside.Remove(e);
        //     }
        // }
    }

    IEnumerator StunPlayer(PlayerControl pc)
    {
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
            pc.speed = originalSpeed;
    }

    IEnumerator DamageLoop()
    {
        while (true)
        {
            foreach (var kvp in playerHearts)
            {
                if (kvp.Key != null)
                    kvp.Key.TakeDamage(damageAmount);
            }

            //TODO: wait for enemy implementation
            // foreach (var kvp in enemiesInside)
            // {
            //     if (kvp.Key != null)
            //         kvp.Key.TakeDamage(damageAmount);
            // }

            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void OnDestroy()
    {
        // Restore player speeds if still stunned
        foreach (var kvp in playersInside)
        {
            if (kvp.Key != null)
                kvp.Key.speed = kvp.Value;
            Debug.Log(kvp.Value);
        }

        playersInside.Clear();
        playerHearts.Clear();

        //TODO: wait for enemy implementation
        // foreach (var kvp in enemiesInside)
        // {
        //     if (kvp.Key != null)
        //         kvp.Key.speed = kvp.Value;
        // }
        // enemiesInside.Clear();
    }
}
