using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    Animation anim;
    public float speed = 5;
    public float rotationSpeed = 10f; // how fast the character turns

    public GameObject bombPrefab;        // assign your Bomb prefab
    public GameObject electricBombPrefab;        // assign your Bomb prefab
    public GameObject fireBombPrefab;        // assign your Bomb prefab
    public GameObject waterBombPrefab;        // assign your Bomb prefab
    public float bombCooldown = 0.75f;   // time between drops
    public float spawnForward = 0.6f;    // a bit in front of feet
    private float _lastBombTime = -999f;

    //Scout Ability
    private bool scoutBoostActive = false;
    private float scoutBoostDuration = 15f;
    private float scoutBoostMultiplier = 1.2f;

    //Boost Ability
    private bool medicAbilityActive = false;
    public float medicAbilityDuration = 25f;

    void Start()
    {
        anim = GetComponent<Animation>();
    }

    void Update()
    {
        // Get input (WASD or arrow keys)
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // Combine into one direction vector
        Vector3 direction = new Vector3(moveX, 0f, moveY).normalized;

        // If the player is pressing a direction
        if (direction.magnitude > 0.1f)
        {
            // Move the player
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            // Smoothly rotate to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Play the walk animation if not already playing
            if (!anim.isPlaying)
                anim.Play("Armature|Walk"); // Replace with your clip name if needed
        }
        else
        {
            // Stop animation when idle
            anim.Stop();
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time - _lastBombTime >= bombCooldown)
        {
            DropBomb('B');
            _lastBombTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.E) && Time.time - _lastBombTime >= bombCooldown)
        {
            DropBomb('E');
            _lastBombTime = Time.time;
        }
                
        if (Input.GetKeyDown(KeyCode.R) && Time.time - _lastBombTime >= bombCooldown)
        {
            DropBomb('F');
            _lastBombTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.T) && Time.time - _lastBombTime >= bombCooldown)
        {
            DropBomb('W');
            _lastBombTime = Time.time;
        }
    }

    // -------------- SCOUT BOOST TRIGGER --------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ScoutAbility"))
        {
            Debug.Log("ScoutAbility activated");
            Destroy(other.gameObject);  // remove pickup
            StartCoroutine(ApplyScoutBoost());
        }

        // Medic ability pickup
        if (other.CompareTag("MedicAbility"))
        {
            Debug.Log("MedicAbility activated");
            Destroy(other.gameObject);
            StartCoroutine(ApplyMedicAbility());
        }

        if (medicAbilityActive && (other.CompareTag("Bomb") || other.CompareTag("WaterBomb") || other.CompareTag("FireBomb") || other.CompareTag("ElectricBomb")))
        {
            BombController bomb = other.GetComponent<BombController>();
            if (bomb != null)
            {
                bomb.Defuse();
                Debug.Log("Bomb defused by MedicAbility!");
            }
        }
    }

    private System.Collections.IEnumerator ApplyScoutBoost()
    {
        if (scoutBoostActive) yield break; // prevent stacking

        scoutBoostActive = true;
        speed *= scoutBoostMultiplier; // increase speed

        Debug.Log("Scout boost activated! Speed increased by 20%");

        yield return new WaitForSeconds(scoutBoostDuration);

        speed /= scoutBoostMultiplier; // reset to normal
        scoutBoostActive = false;

        Debug.Log("Scout boost expired.");
    }

    private System.Collections.IEnumerator ApplyMedicAbility()
    {
        if (medicAbilityActive) yield break;

        medicAbilityActive = true;
        Debug.Log("Medic ability active for " + medicAbilityDuration + " seconds.");

        yield return new WaitForSeconds(medicAbilityDuration);

        medicAbilityActive = false;
        Debug.Log("Medic ability expired.");
    }

    void DropBomb(char bombType)
    {
        if (!bombPrefab) { Debug.LogWarning("No bombPrefab set on PlayerControl."); return; }

        Vector3 spawnPos = transform.position + transform.forward * spawnForward + Vector3.up * 0.5f;
        Quaternion spawnRot = Quaternion.identity;
        Debug.Log(Vector3.up);
        Debug.Log(spawnPos);

        switch (bombType)
            {
                case 'B': // Normal Bomb
                    Instantiate(bombPrefab, spawnPos, spawnRot);
                    break;
                case 'E': // Electric Bomb
                    Instantiate(electricBombPrefab, spawnPos, spawnRot);
                    break;
                case 'F': // Fire Bomb
                    Instantiate(fireBombPrefab, spawnPos, spawnRot);
                    break;
                case 'W': // Water Bomb
                    Instantiate(waterBombPrefab, spawnPos, spawnRot);
                    break;
                default:
                    Debug.LogWarning("Unknown bomb type: " + bombType);
                    return;
            }
    }
}