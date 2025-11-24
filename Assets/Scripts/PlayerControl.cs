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
    private enum BombType { Normal, Electric, Fire, Water }
    private BombType currentBombType = BombType.Normal;

    private int maxSpeedPowerUp = 5;
    private int countSpeedPowerUp = 0;
    private float powerUpSpeed = 0.5f;

    private int maxRangePowerUp = 4;
    private int rangePowerUpLevel = 0;
    private float rangePerLevel = 1f;

    private int maxBombPowerUp = 4;
    private int countBombPowerUp = 0;

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
            DropBomb();
            _lastBombTime = Time.time;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Water power-up
        if (other.CompareTag("PowerUp_Water"))
        {
            currentBombType = BombType.Water;
            Debug.Log("Picked up WATER power-up! Now dropping Water Bombs.");
            Destroy(other.gameObject);
        }
        // Fire power-up
        else if (other.CompareTag("PowerUp_Fire"))
        {
            currentBombType = BombType.Fire;
            Debug.Log("Picked up FIRE power-up! Now dropping Fire Bombs.");
            Destroy(other.gameObject);
        }
        // Electric power-up
        else if (other.CompareTag("PowerUp_Electric"))
        {
            currentBombType = BombType.Electric;
            Debug.Log("Picked up ELECTRIC power-up! Now dropping Electric Bombs.");
            Destroy(other.gameObject);
        }
        
        if (other.CompareTag("PowerUp_Heart"))
        {
            var hearts = GetComponentInChildren<PlayerHearts>();
            if (hearts != null)
            {
                hearts.PickupHeart();
            }
            Destroy(other.gameObject);
        }

        if (other.CompareTag("PowerUp_Speed"))
        {
            if (countSpeedPowerUp < maxSpeedPowerUp)
            {
                speed = speed + powerUpSpeed;
                countSpeedPowerUp += 1;
            }
            
            Destroy(other.gameObject);
        }

        if (other.CompareTag("PowerUp_Range"))
        {
            if (rangePowerUpLevel < maxRangePowerUp)
            {
                rangePowerUpLevel++;
            }
            Destroy(other.gameObject);
        }

        if (other.CompareTag("PowerUp_Bomb"))
        {
            if (countBombPowerUp < maxBombPowerUp)
            {
                countBombPowerUp += 1;
            }
            Destroy(other.gameObject);
        }
    }

    void DropBomb()
    {
        if (!bombPrefab) { Debug.LogWarning("No bombPrefab set on PlayerControl."); return; }

        Vector3 spawnPos = transform.position + transform.forward * 1f + Vector3.up * 0.5f;
        Quaternion spawnRot = Quaternion.identity;
        Debug.Log(Vector3.up);
        Debug.Log(spawnPos);

        GameObject selectedPrefab = null;

        switch (currentBombType)
        {
            case BombType.Normal:
                selectedPrefab = bombPrefab;
                break;
            case BombType.Electric:
                selectedPrefab = electricBombPrefab;
                break;
            case BombType.Fire:
                selectedPrefab = fireBombPrefab;
                break;
            case BombType.Water:
                selectedPrefab = waterBombPrefab;
                break;
        }

        if (selectedPrefab == null)
        {
            Debug.LogWarning("No prefab assigned for current bomb type: " + currentBombType);
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject bombInstance = Instantiate(selectedPrefab, spawnPos, spawnRot);


            BombController bc = bombInstance.GetComponent<BombController>();
            if (bc != null)
            {
                bc.blastRange += rangePowerUpLevel * rangePerLevel;
                bc.damage += countBombPowerUp;
            }
        }
    }
}