using UnityEngine;

//Script to float and rotate power ups
public class PowerUpFloatAndRotate : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatAmplitude = 0.25f;   
    public float floatSpeed = 2f;          

    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;     

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}
