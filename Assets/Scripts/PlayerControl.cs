using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    Animation anim;
    public float speed = 5;
    public float rotationSpeed = 10f; // how fast the character turns

    public GameObject bombPrefab;        // assign your Bomb prefab
    public float bombCooldown = 0.75f;   // time between drops
    public float spawnForward = 0.6f;    // a bit in front of feet
    private float _lastBombTime = -999f;

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

    void DropBomb()
    {
        if (!bombPrefab) { Debug.LogWarning("No bombPrefab set on PlayerControl."); return; }

        Vector3 spawnPos = transform.position + transform.forward * spawnForward + Vector3.up * 0.5f;
        Quaternion spawnRot = Quaternion.identity;
        Debug.Log(Vector3.up);
        Debug.Log(spawnPos);

        Instantiate(bombPrefab, spawnPos, spawnRot);
    }
}