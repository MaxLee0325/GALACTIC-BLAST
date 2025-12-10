using Unity.VisualScripting;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform player;      
    public float height = 10f;
    public float smoothSpeed = 5f;

    // Bounds for horizontal camera movement
    public float minX = -4;
    public float maxX = 4;
    public float minZ = -1.5f;
    public float maxZ = 4;

    private float fixedY;

    private void Start()
    {
        fixedY = transform.position.y;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float targetX = Mathf.Clamp(player.position.x, minX, maxX);
        float targetZ = Mathf.Clamp(player.position.z, minZ, maxZ);

        float rad = Mathf.Deg2Rad * 60;
        float yOffset = Mathf.Sin(rad) * height;
        float zOffset = Mathf.Cos(rad) * height;

        // Keep fixed Z and Y (distance and height)
        Vector3 targetPos = new Vector3(targetX, player.position.y + yOffset, targetZ - zOffset);

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // Keep camera tilted at 45 degrees
        transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}
