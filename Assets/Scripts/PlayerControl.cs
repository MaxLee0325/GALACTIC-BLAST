using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    Animation anim;
    public float moveAmount = 1;
    public float moveSpeed = 5;
    private bool isMoving = false;

    public GameObject bombPrefab;        // assign your Bomb prefab
    public GameObject electricBombPrefab;        // assign your Bomb prefab
    public GameObject fireBombPrefab;        // assign your Bomb prefab
    public GameObject waterBombPrefab;        // assign your Bomb prefab
    public float bombCooldown = 0.75f;   // time between drops
    public float spawnForward = 0.6f;    // a bit in front of feet
    private float _lastBombTime = -999f;

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

    void DropBomb(char bombType)
    {
        if (!bombPrefab) { Debug.LogWarning("No bombPrefab set on PlayerControl."); return; }

        Vector3 spawnPos = new Vector3(Mathf.Round(transform.position.x), (float)(transform.position.y + 0.5), Mathf.Round(transform.position.z));
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