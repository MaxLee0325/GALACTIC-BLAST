using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    Animation anim;
    public float speed = 5f;
    public float moveAmount = 1f;
    public float moveSpeed = 5;
    private bool isMoving = false;

    public GameObject bombPrefab;        // assign your Bomb prefab
    public GameObject electricBombPrefab;        // assign your Bomb prefab
    public GameObject fireBombPrefab;        // assign your Bomb prefab
    public GameObject waterBombPrefab;        // assign your Bomb prefab
    public float bombCooldown = 20f;   // time between drops
    public float spawnForward = 0.6f;    // a bit in front of feet
    private float _lastBombTime = -999f;
    private enum BombType { Normal, Electric, Fire, Water }
    private BombType currentBombType = BombType.Normal;

    private int maxSpeedPowerUp = 5;
    private int countSpeedPowerUp = 0;
    private float powerUpSpeed = 0.5f;

    private int maxRangePowerUp = 6;
    private int rangePowerUpLevel = 0;
    private float rangePerLevel = 1f;

    private int maxBombPowerUp = 6;
    private int countBombPowerUp = 0;

    [Header("Audio")]
    [SerializeField] private AudioSource maxPowerUpAudio;

    void Start()
    {
        anim = GetComponent<Animation>();
    }

    void Update()
    {
        if (isMoving) return;

        Vector3 direction = Vector3.zero;
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            direction = Vector3.forward;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            direction = Vector3.back;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            direction = Vector3.left;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            direction = Vector3.right;
        }

        if (direction != Vector3.zero)
        {
            RotateToDirection(direction);
            if (CanMove(direction))
            {
                anim.Play();
                StartCoroutine(Move(direction));
            }
            else
            {
                // Stop animation when idle
                anim.Stop();
            }
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
        
        // Heart Power-up
        else if (other.CompareTag("PowerUp_Heart"))
        {
            var hearts = GetComponentInChildren<PlayerHearts>();
            if (hearts != null )
            {
                hearts.PickupHeart();
            }
            Destroy(other.gameObject);
        }

        // Speed Power-up
        else if (other.CompareTag("PowerUp_Speed"))
        {
            if (countSpeedPowerUp < maxSpeedPowerUp)
            {
                speed = speed + powerUpSpeed;
                countSpeedPowerUp += 1;
            }
            else
            {
                PlayMaxPowerUpAudio();
            }

                Destroy(other.gameObject);
        }

        // Range power-up
        else if (other.CompareTag("PowerUp_Range"))
        {
            if (rangePowerUpLevel < maxRangePowerUp)
            {
                rangePowerUpLevel++;
            }
            else
            {
                PlayMaxPowerUpAudio();
            }
            Destroy(other.gameObject);
        }

        // Bomb power-up
        else if (other.CompareTag("PowerUp_Bomb"))
        {
            if (countBombPowerUp < maxBombPowerUp)
            {
                countBombPowerUp += 1;
                Debug.Log("Bomb power up"+countBombPowerUp);
            }
            else
            {
                PlayMaxPowerUpAudio();
            }
            Destroy(other.gameObject);
        }
    }

    bool CanMove(Vector3 direction)
    {
        Vector3 targetPos = transform.position + direction * moveAmount;

        Collider[] hits = Physics.OverlapSphere(targetPos, 0.35f);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Wall") || hit.CompareTag("Destructible")) {
                return false;
            }
        }

        return true;
    }

    void RotateToDirection(Vector3 direction)
    {
        if (direction == Vector3.forward)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction == Vector3.back)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (direction == Vector3.left)
        {
            transform.rotation = Quaternion.Euler(0, -90, 0);
        }
        else if (direction == Vector3.right)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }

    IEnumerator Move(Vector3 direction)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 target = start + direction * moveAmount;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, target, t);

            yield return null;
        }
        transform.position = target;
        isMoving = false;
    }

    void DropBomb()
    {
        if (!bombPrefab) { Debug.LogWarning("No bombPrefab set on PlayerControl."); return; }

        Vector3 spawnPos = new Vector3(Mathf.Round(transform.position.x), (float)(transform.position.y + 0.5), Mathf.Round(transform.position.z));
        Quaternion spawnRot = Quaternion.identity;

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

        for (int i = 0; i < 1 + countBombPowerUp; i++)
        {
            GameObject bombInstance = Instantiate(selectedPrefab, spawnPos, spawnRot);


            BombController bc = bombInstance.GetComponent<BombController>();
            if (bc != null)
            {
                bc.blastRange += rangePowerUpLevel * rangePerLevel;
            }
        }
        
    }

    private void PlayMaxPowerUpAudio()
    {
        if (maxPowerUpAudio != null && !maxPowerUpAudio.isPlaying)
        {
            maxPowerUpAudio?.Play();
        }
    }
}