using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float liveTime = 1.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, liveTime);
    }
}
