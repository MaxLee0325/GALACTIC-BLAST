using UnityEngine;

public class PowerUpFloatAndRotate : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatAmplitude = 0.25f;   // how high it moves up/down
    public float floatSpeed = 2f;          // how fast it moves

    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;      // degrees per second

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Floating motion
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}
