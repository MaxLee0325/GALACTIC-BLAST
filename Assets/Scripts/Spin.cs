using UnityEngine;

public class Spin : MonoBehaviour
{
    public float speed = 30f;

    //Continuously rotates the object around the Y-axis
    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }
}
