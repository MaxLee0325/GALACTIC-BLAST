using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningGround : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageInterval = 1f;   // Damage every second
    public int damageAmount = 1;
    public float lifeTime = 3f;         // Destroy after this many seconds

    private List<PlayerHearts> playersInside = new List<PlayerHearts>();

    //TODO: wait for enemy implementation
    //private List<Enemy> enemiesInside = new List<Enemy>();

    private void Start()
    {
        Destroy(gameObject, lifeTime);  // Fire disappears after 3 sec
        StartCoroutine(BurnLoop());     // Always run burn loop
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player
        if (other.CompareTag("Player"))
        {
            PlayerHearts ph = other.GetComponent<PlayerHearts>();
            if (ph != null && !playersInside.Contains(ph))
            {
                playersInside.Add(ph);
                Debug.Log("Player on fire!");
            }
        }

        //TODO: wait for enemy implementation
        // if (other.CompareTag("Enemy"))
        // {
        //     Enemy eh = other.GetComponent<Enemy>();
        //     if (eh != null && !enemiesInside.Contains(eh))
        //         enemiesInside.Add(eh);
        // }

        // Extinguish fire if touches wet ground
        if (other.CompareTag("WetGround"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHearts ph = other.GetComponent<PlayerHearts>();
            if (ph != null && playersInside.Contains(ph))
                playersInside.Remove(ph);
        }

        //TODO: wait for enemy implementation
        // if (other.CompareTag("Enemy"))
        // {
        //     Enemy eh = other.GetComponent<Enemy>();
        //     if (eh != null && enemiesInside.Contains(eh))
        //         enemiesInside.Remove(eh);
        // }
    }

    private IEnumerator BurnLoop()
    {
        while (true)
        {
            // Damage players
            foreach (var p in playersInside)
                if (p != null)
                {
                    p.TakeDamage(damageAmount);
                    Debug.Log("Fire cause damage");
                }

            //TODO: wait for enemy implementation
            // foreach (var e in enemiesInside)
            //     if (e != null) e.TakeDamage(damageAmount);

            yield return new WaitForSeconds(damageInterval);
        }
    }
}
