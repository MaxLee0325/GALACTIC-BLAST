using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class EnemyController : MonoBehaviour
{
    [Header("Detection & Chase")]
    public float chaseRange = 10f; // Kept for SphereCollider radius
    public float attackRange = 2f;
    [Header("Movement")]
    public float speed = 5f;
    public float angularSpeed = 120f;

    [SerializeField] public Transform playerTransform; 

    private Rigidbody rb;
    private bool playerInChaseRange = false;
    public int health = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        SphereCollider triggerCol = GetComponent<SphereCollider>();
        triggerCol.isTrigger = true;
        triggerCol.radius = chaseRange;
        triggerCol.center = Vector3.zero;
    }

    void Update()
    {
        if(health <= 0){
            Destroy(gameObject);
        }

        if (!playerInChaseRange)
            return;

        Vector3 playerPos = playerTransform.position;
        Vector3 directionToPlayer = (playerPos - transform.position).normalized;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, angularSpeed * Time.fixedDeltaTime);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > attackRange)
        {
            Vector3 moveVelocity = new Vector3(directionToPlayer.x * speed, rb.linearVelocity.y, directionToPlayer.z * speed);
            rb.linearVelocity = moveVelocity;
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInChaseRange = true;
            Debug.Log(gameObject.name + " detected player - starting chase!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInChaseRange = false;
            Debug.Log(gameObject.name + " lost player - stopping chase!");
        }
    }

    public void OnDeath()
    {
        if (rb != null)
            rb.linearVelocity = Vector3.zero;
        enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
