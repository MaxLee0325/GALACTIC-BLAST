using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float liveTime = 1.5f;

    //Schedules the object to destroy itself after a defined amount of time.
    void Start()
    {
        Destroy(gameObject, liveTime);
    }
}
