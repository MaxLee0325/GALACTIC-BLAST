using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZapBatAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range;

    public Transform centrePoint;

    [Header("Sonar Detection (No Raycast - Blind Enemy)")]
    [SerializeField] float sonarRange = 15f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask obstacleLayers;

    bool playerInSonarRange, playerInAttackRange;

    GameObject player;

    [SerializeField] private float loseMemory = 5f; // Lightning fast reflexes
    [SerializeField] private Animator anim;
    private float lastTimeDetectedPlayer;
    
    [Header("Teleport System")]
    [SerializeField] private float teleportDistance = 4f;
    [SerializeField] private float teleportCheckRadius = 0.5f; // Radius to check for obstacles at teleport destination
    
    [Header("Electric Attack System")] 
    [SerializeField] private GameObject electricTilePrefab;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float electricTileDuration = 3f;
    [SerializeField] private float blastRange = 2f;
    [SerializeField] private float blinkDelay = 1f; // Time before spawning tiles
    
    [Header("Blast Preview")]
    [SerializeField] private Color previewColor = Color.cyan;
    [SerializeField] private float previewLineWidth = 0.2f;
    private GameObject blastPreview;
    
    [Header("Audio")] 
    [SerializeField] private AudioSource electricSpawnAudio;
    [SerializeField] private AudioSource teleportAudio;
    
    private bool inAttackSequence = false;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Vector3 checkPos = transform.position + Vector3.up * 1f;

        playerInSonarRange = DetectPlayerWithSonar();
        playerInAttackRange = Physics.CheckSphere(checkPos, attackRange, playerLayer);

        if (playerInSonarRange)
        {
            lastTimeDetectedPlayer = Time.time;
            
            // Check if we should teleport through destructible wall
            if (!inAttackSequence)
            {
                TryTeleportThroughWall();
            }
        }
        
        bool shouldChase = Time.time < lastTimeDetectedPlayer + loseMemory;

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

        // Stop attack animation if not attacking
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
        agent.stoppingDistance = attackRange;
        agent.SetDestination(player.transform.position);
        
        Vector3 lookDirection = (player.transform.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, lookDirection, Time.deltaTime * 5f);
        }
        Debug.Log("ZapBat Chase");
    }

    void Attack()
    {
        if (inAttackSequence) return;

        inAttackSequence = true;
        
        StartCoroutine(ElectricAttack());
    }

    IEnumerator ElectricAttack()
    {
        if (agent == null) yield break;;
        // Stop all movement
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsAttacking", true);
        
        // Create preview visual
        blastPreview = new GameObject("ElectricBlastPreview");
        blastPreview.transform.position = transform.position;

        LineRenderer lr = blastPreview.AddComponent<LineRenderer>();
        lr.positionCount = 5;
        lr.loop = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = previewLineWidth;
        lr.startColor = lr.endColor = previewColor;

        float r = blastRange;
        Vector3[] corners = new Vector3[5];
        corners[0] = transform.position + new Vector3(-r, 0.1f, -r);
        corners[1] = transform.position + new Vector3(-r, 0.1f, r);
        corners[2] = transform.position + new Vector3(r, 0.1f, r);
        corners[3] = transform.position + new Vector3(r, 0.1f, -r);
        corners[4] = corners[0];
        lr.SetPositions(corners);

        // Blink once
        float timer = blinkDelay;
        bool visible = true;
        
        while (timer > 0f)
        {
            if (lr != null)
            {
                visible = !visible;
                lr.enabled = visible;
            }
            
            // Keep tracking player even while stationary
            if (playerInSonarRange)
            {
                lastTimeDetectedPlayer = Time.time;
            }
            
            yield return new WaitForSeconds(0.5f);
            timer -= 0.5f;
        }

        // Destroy preview
        if (blastPreview != null)
            Destroy(blastPreview);

        // Spawn electric tiles
        SpawnElectricTiles();
        
        // Stay stationary for tile duration
        yield return new WaitForSeconds(electricTileDuration);
        
        // Attack sequence complete
        inAttackSequence = false;
        anim.SetBool("IsAttacking", false);
        agent.isStopped = false;
    }

    void SpawnElectricTiles()
    {
        Debug.Log("ZapBat spawns electric tiles!");
        
        // Play audio
        if (electricSpawnAudio != null)
        {
            electricSpawnAudio.Play();
        }
        
        // ## DEAL DAMAGE CODE HERE ##
        // Check for entities in blast range and deal damage
        Collider[] hits = Physics.OverlapSphere(transform.position, blastRange);
        
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                var hearts = h.GetComponent<Collider>().GetComponentInParent<PlayerHearts>();
                if (hearts) hearts.TakeDamage(1);
                Debug.Log("Player takes electric damage!");
            }

            if (h.CompareTag("WaterEnemy") && h.gameObject != gameObject)
            {
                var e = h.GetComponent<Collider>().GetComponent<EnemyHealth>();
                if(e) e.TakeDamage(2); // Water is weak to electric
            }

            if (h.CompareTag("FireEnemy") && h.gameObject != gameObject)
            {
                var e = h.GetComponent<Collider>().GetComponent<EnemyHealth>();
                if(e) e.TakeDamage(1);
            }
        }
        
        // Spawn tiles in a square pattern around the bat
        Vector3 spawnPosition = new Vector3(transform.position.x, -0.412f, transform.position.z);
        Instantiate(electricTilePrefab, spawnPosition, Quaternion.identity);
    }

    void TryTeleportThroughWall()
    {
        if (player == null) return;
        
        // Direction towards player
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        
        // Calculate teleport destination
        Vector3 teleportDestination = transform.position + directionToPlayer * teleportDistance;
        
        // Check if there's a destructible wall between us and the teleport point
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, teleportDistance, obstacleLayers))
        {
            // ANY destructible wall, not just ones between us and player
            if (hit.collider.CompareTag("Destructible"))
            {
                // Check if teleport destination is clear (not inside another wall)
                if (!Physics.CheckSphere(teleportDestination, teleportCheckRadius, obstacleLayers))
                {
                    // Safe to teleport
                    Teleport(teleportDestination);
                }
                else
                {
                    Debug.Log("ZapBat: Teleport blocked - destination obstructed");
                }
            }
        }
    }

    void Teleport(Vector3 destination)
    {
        Debug.Log("ZapBat teleports!");
        
        // Play teleport audio
        if (teleportAudio != null)
        {
            teleportAudio.Play();
        }
        
        // Teleport
        agent.Warp(destination);
        
        // Optional: Add teleport effect/particle here if you have one
    }

    //===================== SONAR DETECTION (NO RAYCAST) =====================//
    
    bool DetectPlayerWithSonar()
    {
        if (player == null) return false;

        // Simple sphere check - no line of sight needed (sonar goes through walls)
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        if (distanceToPlayer <= sonarRange)
        {
            Debug.DrawLine(transform.position, player.transform.position, Color.cyan, 0.2f);
            return true;
        }

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
    
    //===================== GIZMOS FOR DEBUGGING =====================//
    
    private void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        
        // Sonar range (detection) - Cyan wire sphere
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pos, sonarRange);
        
        // Attack range (when to start attacking) - Yellow wire sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, attackRange);
        
        // Electric blast damage area - Red wire sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, blastRange);
        
        // Teleport distance indicator - Green line
        if (player != null)
        {
            Vector3 directionToPlayer = (player.transform.position - pos).normalized;
            directionToPlayer.y = 0;
            Vector3 teleportDest = pos + directionToPlayer * teleportDistance;
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos, teleportDest);
            Gizmos.DrawWireSphere(teleportDest, teleportCheckRadius);
        }
        
        // Patrol range - Blue wire sphere (centered on centrePoint)
        if (centrePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(centrePoint.position, range);
        }
    }
}