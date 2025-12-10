using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class WaterTurtleAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range;

    public Transform centrePoint;

    [SerializeField] float visionDistance, attackRange;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask obstacleLayers;

    bool playerInSightRange, playerInAttackRange;

    GameObject player;

    [SerializeField] private float loseMemory = 1.5f;
    [SerializeField] private Animator anim;
    private float lastTimeSeenPlayer;
    
    
    //Let's weaponize this baby like hammertech did Iron Man

    [Header("Weapon System")] 
    public float explodeDelay = 2f;

    public float blastRange = 2f;
    public bool isDetonating = false;
    
    [Header("BlastUi & Audio")] 
    private TextMeshProUGUI countdownText;
    public GameObject wetGroundPrefab;

    public AudioSource explosionAudio;
    
    [Header("Blast Preview")]
    public Color previewColor = Color.red; // color of the square
    public float previewLineWidth = 0.2f;  // thickness
    private GameObject blastPreview;       // temporary GameObject for square


    private bool inAttackSequnce = false;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        countdownText = GetComponentInChildren<TextMeshProUGUI>();
        if (countdownText != null)
        {
            countdownText.transform.localPosition = new Vector3(0, 2, 0);
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.text = "";
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

        if (shouldChase && !playerInAttackRange && !inAttackSequnce)
        {
            Chase();
        }
        else if (playerInAttackRange)
        {
            Attack();
        }
        else if(!inAttackSequnce)
        {
            Patrol();
        }
        
        UpdateMovementAnimation();
    }
    
    void UpdateMovementAnimation()
    {
        if (inAttackSequnce)
        {
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsAttacking", true);
            return;
        }
        
        
        // Snappy walking detection
        bool walking = agent.velocity.magnitude > 0.1f; // small threshold
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
        agent.stoppingDistance = attackRange;
        agent.SetDestination(player.transform.position);
        
        Vector3 lookDirection = (player.transform.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, lookDirection, Time.deltaTime * 5f);
        }
        Debug.Log(" Chase");
    }

    void Attack()
    {
        if (isDetonating) return;

        inAttackSequnce = true;
        
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsAttacking", true);
        
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        
        StartCoroutine(ExplosionCountdown());

    }

    IEnumerator ExplosionCountdown()
    {
        isDetonating = true;
        float timer = explodeDelay;

        // Create the preview visual immediately
        blastPreview = new GameObject("BlastPreview");
        blastPreview.transform.position = transform.position;

        LineRenderer lr = blastPreview.AddComponent<LineRenderer>();
        lr.positionCount = 5; // 4 corners + return to first
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
        corners[4] = corners[0]; // close the square
        lr.SetPositions(corners);

        // Blink logic
        bool visible = true;
        while (timer > 0f)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(timer).ToString();

                // optional: color gradient
                if (timer > explodeDelay / 2f) countdownText.color = Color.green;
                else if (timer > explodeDelay / 4f) countdownText.color = Color.yellow;
                else countdownText.color = Color.red;

                // always face camera
                if (Camera.main != null)
                {
                    countdownText.transform.LookAt(Camera.main.transform);
                    countdownText.transform.Rotate(0, 180, 0);
                }
            }

            // Blink every 0.5 seconds
            if (lr != null)
            {
                visible = !visible;
                lr.enabled = visible;
            }

            yield return new WaitForSeconds(0.5f);
            timer -= 0.5f;
        }

        // Explosion happens
        if (countdownText != null)
            countdownText.text = "BOOM!";

        if (blastPreview != null)
            Destroy(blastPreview);

        Explode();
        isDetonating = false;
    }


     void Explode()
    {
        Debug.Log("Boom boom man said boom");
        

        if (explosionAudio != null)
        {
            explosionAudio.Play();
        }
        Collider[] hits = Physics.OverlapSphere(transform.position, blastRange);

        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                var hearts = h.GetComponent<Collider>().GetComponentInParent<PlayerHearts>();
                if (hearts) hearts.TakeDamage(1);
                Debug.Log("Player takes damage!");
            }

            if (h.CompareTag("WaterEnemy") && h.gameObject != gameObject)
            {
                var e = h.GetComponent<Collider>().GetComponent<EnemyHealth>();
                if(e) e.TakeDamage(1);
            }

            if (h.CompareTag("FireEnemy") && h.gameObject != gameObject)
            {
                var e = h.GetComponent<Collider>().GetComponent<EnemyHealth>();
                if(e) e.TakeDamage(2);
            }
            
            if (h.CompareTag("ElectricEnemy") && h.gameObject != gameObject)
            {
                var e = h.GetComponent<Collider>().GetComponent<EnemyHealth>();
                if(e) e.TakeDamage(2);
            }

            if (h.CompareTag("Destructible"))
            {
                Destroy(h.gameObject);
            }
        }

        Vector3 groundPosition = new Vector3(transform.position.x, -0.412f, transform.position.z);
        Instantiate(wetGroundPrefab, groundPosition, Quaternion.identity);

        GetComponent<EnemyHealth>().TakeDamage(3);
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
        // if (player != null && playerInAttackRange)
        // {
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawWireSphere(player.transform.position, circleRadius);
        // }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, blastRange);
    
        // FOV cone visualization (optional)
        Vector3 forward = transform.forward * visionDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, fov / 2f, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -fov / 2f, 0) * forward;
    
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, rightBoundary);
        Gizmos.DrawRay(transform.position, leftBoundary);
    }

}
