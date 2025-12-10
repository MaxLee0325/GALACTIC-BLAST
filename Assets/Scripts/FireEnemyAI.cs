using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FireEnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range;

    public Transform centrePoint;

    [SerializeField] float visionDistance, attackRange;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask obstacleLayers;

    bool playerInSightRange, playerInAttackRange;

    GameObject player;

    [SerializeField] private float loseMemory = 1.0f; // Shorter memory than water turtle
    [SerializeField] private Animator anim;
    private float lastTimeSeenPlayer;
    
    
    [Header("Lava Attack System")] 
    [SerializeField] private GameObject burningGroundPrefab;
    [SerializeField] private float delayBetweenAttacks = 1f;
    [SerializeField] private float circleRadius = 3f; // Radius around player to spawn lava
    
    [Header("Audio")] 
    [SerializeField] private AudioSource lavaSpawnAudio;
    
    private bool inAttackSequence = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
        //Ignore player collider for smoother gameplay.... 
        if (player != null)
        {
            Collider enemyCollider = GetComponent<Collider>();
            Collider playerCollider = player.GetComponent<Collider>();
        
            if (enemyCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(enemyCollider, playerCollider, true);
            }
        }
    }

    void Update()
    {
        
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null && health.IsStunned) return;
        
        
        Vector3 checkPos = transform.position + Vector3.up * 1f;

        playerInSightRange = CanSeePlayer();
        playerInAttackRange = Physics.CheckSphere(checkPos, attackRange, playerLayer);

        if (playerInSightRange)
        {
            lastTimeSeenPlayer = Time.time;
        }
        
        bool shouldChase = Time.time < lastTimeSeenPlayer + loseMemory;

        if (shouldChase && !playerInAttackRange && !inAttackSequence)
        {
            Chase();
        }
        else if (playerInAttackRange)
        {
            Attack();
        }
        else if(!inAttackSequence)
        {
            Patrol();
        }
        
        UpdateMovementAnimation();
    }
    
    void UpdateMovementAnimation()
    {
        if (inAttackSequence)
        {
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsAttacking", true);
            return;
        }
        
        // Snappy walking detection
        bool walking = agent.velocity.magnitude > 0.1f;
        anim.SetBool("IsWalking", walking);

        // Optional: stop attack animation if not attacking
        if (!playerInAttackRange && anim.GetBool("IsAttacking"))
            anim.SetBool("IsAttacking", false);
    }

    void Patrol()
    {
        anim.SetBool("IsAttacking", false);
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1f);
                agent.SetDestination(point);
            }
        }
    }

    void Chase()
    {
        anim.SetBool("IsAttacking", false);
        
        agent.isStopped = false;
        agent.stoppingDistance = 1f;
        agent.SetDestination(player.transform.position);
        
        Vector3 lookDirection = (player.transform.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, lookDirection, Time.deltaTime * 5f);
        }
    }

    void Attack()
    {
        if (inAttackSequence) return;

        inAttackSequence = true;
        
        StartCoroutine(CircleAndAttack());
    }

    IEnumerator CircleAndAttack()
    {
        // Continue attacking as long as player is in range and we have memory
        while (playerInAttackRange && Time.time < lastTimeSeenPlayer + loseMemory)
        {
            // Pick a random point around the player
            Vector3 randomPoint;
            if (GetRandomPointAroundPlayer(out randomPoint) && agent && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                // Set stopping distance small
                agent.stoppingDistance = 0.2f;
                agent.isStopped = false;
                agent.SetDestination(randomPoint);
    
                anim.SetBool("IsWalking", true);
                anim.SetBool("IsAttacking", false);
    
                yield return new WaitForSeconds(0.3f);
                if (agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
                {
                    Debug.Log("Path blocked or invalid, skipping this point");
                    continue;
                }
                // Wait until close to destination OR timeout
                float moveTimer = 0f;
                float maxMoveTime = 3f;
    
                while (Vector3.Distance(transform.position, randomPoint) > 0.8f && moveTimer < maxMoveTime)
                {
                    moveTimer += Time.deltaTime;
                    

                    // Check if player left range while moving
                    Vector3 checkP = transform.position + Vector3.up * 1f;
                    playerInAttackRange = Physics.CheckSphere(checkP, attackRange, playerLayer);
                    
                    if (!playerInAttackRange || Time.time >= lastTimeSeenPlayer + loseMemory)
                    {
                        inAttackSequence = false;
                        anim.SetBool("IsAttacking", false);
                        yield break;
                    }
                    
                    yield return null;
                }
                
                // Stop and face player
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                
                Vector3 lookDirection = (player.transform.position - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    transform.forward = lookDirection;
                }
                
                // Play attack animation and spawn lava
                anim.SetBool("IsWalking", false);
                anim.SetBool("IsAttacking", true);
                
                yield return new WaitForSeconds(0.3f); // Short delay for animation to play
                
                SpawnLavaPool();
                
                // Wait before next attack
                yield return new WaitForSeconds(delayBetweenAttacks);
            }
            else
            {
                // Couldn't find point, wait a bit
                yield return new WaitForSeconds(0.5f);
            }
            
            // Refresh attack range check
            Vector3 checkPos = transform.position + Vector3.up * 1f;
            playerInAttackRange = Physics.CheckSphere(checkPos, attackRange, playerLayer);
        }
        
        // Attack sequence ended
        inAttackSequence = false;
        anim.SetBool("IsAttacking", false);
        agent.isStopped = false;
    }

    void SpawnLavaPool()
    {
        
        // Play audio
        if (lavaSpawnAudio != null)
        {
            lavaSpawnAudio.Play();
        }
        
        // Spawn at enemy's position with fixed Y coordinate
        Vector3 spawnPosition = new Vector3(transform.position.x, -0.412f, transform.position.z);
        Instantiate(burningGroundPrefab, spawnPosition, Quaternion.identity);
    }

    bool GetRandomPointAroundPlayer(out Vector3 result)
    {
        if (player == null)
        {
            result = Vector3.zero;
            return false;
        }
        
        // Get random point in a circle around the player
        Vector2 randomCircle = Random.insideUnitCircle * circleRadius;
        Vector3 randomPoint = player.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
        {
            result = hit.position;
            Debug.DrawRay(result, Vector3.up * 2f, Color.red, 1f);
            Debug.Log($"Random point found: {result}, distance from player: {Vector3.Distance(result, player.transform.position)}");
            return true;
        }
        
        result = Vector3.zero;
        return false;
    }
    
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    //===================== VISION / RAYCAST SECTION=====================//

    int fov = 90;

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 enemyEye = transform.position + Vector3.up * 0.6f;
        Vector3 playerChest = player.transform.position + Vector3.up * 0.5f;

        Vector3 direction = playerChest - enemyEye;
        float distance = direction.magnitude;

        if (distance > visionDistance) return false;

        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > fov / 2f) return false;

        // Combined layermask for raycast visibility check
        int mask = obstacleLayers | playerLayer;

        //------------------- RAYCAST WITH FULL VISUAL DEBUG -------------------//
        if (Physics.Raycast(enemyEye, direction.normalized, out RaycastHit hit, distance, mask))
        {
            if (hit.transform == player.transform)
            {
                Debug.DrawRay(enemyEye, direction.normalized * distance, Color.red, 0.2f);   // sees player
                return true;
            }
            else
            {
                Debug.DrawRay(enemyEye, direction.normalized * distance, Color.yellow, 0.2f); // blocked by wall
                return false;
            }
        }

        Debug.DrawRay(enemyEye, direction.normalized * visionDistance, Color.white, 0.2f); // no hit at all
        return false;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Vision range - Blue
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    
        // Attack range - Red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    
        // Circle radius around player - Yellow
        if (player != null && playerInAttackRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.transform.position, circleRadius);
        }
    
        // FOV cone visualization (optional)
        Vector3 forward = transform.forward * visionDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, fov / 2f, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -fov / 2f, 0) * forward;
    
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, rightBoundary);
        Gizmos.DrawRay(transform.position, leftBoundary);
    }
}