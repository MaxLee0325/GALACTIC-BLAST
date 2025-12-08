using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BombController : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionTime = 4f;
    public float blastRange = 1f;
    public GameObject blastBeamPrefab;
    public float beamDuration = 0.3f;

    [Header("Beam Visuals")]
    public Gradient beamColorGradient;
    public float beamStartWidth = 0.8f;
    public float beamEndWidth = 0.1f;

    [Header("Pulse Animation")]
    public float pulseSpeed = 3f;
    public float pulseAmplitude = 0.05f;

    [Header("Preview Settings")]
    public float previewStartWidth = 0.15f;
    public float previewEndWidth = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioSource explosionAudio;
    [SerializeField] private AudioClip bombPlacementSound;
    [SerializeField] private AudioClip bombTickingSound;
    [SerializeField] private AudioClip fireExplosionSound;
    [SerializeField] private AudioClip waterExplosionSound;
    [SerializeField] private AudioClip electricExplosionSound;
    [SerializeField] private AudioClip chainReactionSound;

    private TextMeshProUGUI countdownText;
    private Vector3 initialScale;
    private bool hasExploded = false;
    private AudioSource tickingAudioSource;

    // Separate arrays for preview and explosion
    private GameObject[] previewBeams;
    public GameObject PreviewBeamPrefab;
    private float blinkTimer = 0f;
    private bool previewVisible = true;

    [Header("Ground Effects")]
    public GameObject burningGroundPrefab;
    public GameObject wetGroundPrefab;
    public GameObject ElectrifiedWaterGroundPrefab;

    [Header("Ground Effect Audio")]
    [SerializeField] private AudioClip burningGroundLoopSound;
    [SerializeField] private AudioClip wetGroundLoopSound;
    [SerializeField] private AudioClip electricFieldLoopSound;

    [Header("Damage")]
    public int damage = 1;

    public GameObject explosionPrefab;

    void Start()
    {
        initialScale = transform.localScale;
        countdownText = GetComponentInChildren<TextMeshProUGUI>();
        if (countdownText != null)
        {
            countdownText.transform.localPosition = new Vector3(0, 1f, 0);
            countdownText.transform.localScale = Vector3.one;
            countdownText.alignment = TextAlignmentOptions.Center;
        }

        // Play bomb placement sound
        if (bombPlacementSound != null && explosionAudio != null)
        {
            explosionAudio.PlayOneShot(bombPlacementSound, 0.5f);
        }

        // Setup ticking audio source
        tickingAudioSource = gameObject.AddComponent<AudioSource>();
        tickingAudioSource.spatialBlend = 1f; // 3D sound
        tickingAudioSource.loop = false;

        // Default gradient if none set
        if (beamColorGradient == null || beamColorGradient.colorKeys.Length == 0)
        {
            beamColorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0].color = new Color(1f, 0.5f, 0f, 1f);
            colorKeys[0].time = 0f;
            colorKeys[1].color = Color.yellow;
            colorKeys[1].time = 0.5f;
            colorKeys[2].color = Color.yellow;
            colorKeys[2].time = 1f;

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
            alphaKeys[0].alpha = 1f; alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = 1f; alphaKeys[1].time = 0.5f;
            alphaKeys[2].alpha = 0f; alphaKeys[2].time = 1f;

            beamColorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    void Update()
    {
        // Pulsing animation
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        transform.localScale = initialScale * (1f + pulse);

        // Countdown
        explosionTime -= Time.deltaTime;
        UpdateCountdownText();

        // Play ticking sound in last 0.5 seconds
        if (explosionTime <= 0.5f && explosionTime > 0f && bombTickingSound != null && !tickingAudioSource.isPlaying)
        {
            tickingAudioSource.clip = bombTickingSound;
            tickingAudioSource.Play();
        }

        // Preview blinking speed
        float blinkSpeed = explosionTime <= 0.6f ? 10f : (explosionTime <= 2f ? 4f : 2f);
        blinkTimer += Time.deltaTime * blinkSpeed;
        previewVisible = Mathf.Sin(blinkTimer) > 0;

        // Show blast preview
        if (!hasExploded)
        {
            ShowBlastPreview();
        }

        if (explosionTime <= 0f && !hasExploded)
        {
            countdownText.text = "BOOM";
            countdownText.color = Color.red;
            Explode();
        }
    }

    private void UpdateCountdownText()
    {
        if (countdownText == null) return;

        if (explosionTime > 3f)
        {
            countdownText.text = "4";
            countdownText.color = Color.green;
        }
        else if (explosionTime > 2f)
        {
            countdownText.text = "3";
            countdownText.color = Color.green;
        }
        else if (explosionTime > 1f)
        {
            countdownText.text = "2";
            countdownText.color = Color.yellow;
        }
        else if (explosionTime > 0f)
        {
            countdownText.text = "1";
            countdownText.color = Color.red;
        }
    }

    private void ShowBlastPreview()
    {
        if (blastBeamPrefab == null) return;

        if (previewBeams == null) previewBeams = new GameObject[4];

        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 dir = directions[i];
            Vector3 endPoint = transform.position + dir * blastRange;

            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, blastRange))
            {
                if (hit.collider.CompareTag("Wall"))
                    endPoint = hit.point;
            }

            if (previewBeams[i] == null)
            {
                previewBeams[i] = Instantiate(PreviewBeamPrefab, transform.position, Quaternion.identity);
                LineRenderer lr = previewBeams[i].GetComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = previewStartWidth;
                lr.endWidth = previewEndWidth;
                lr.useWorldSpace = true;
            }

            LineRenderer line = previewBeams[i].GetComponent<LineRenderer>();
            line.SetPosition(0, transform.position);
            line.SetPosition(1, endPoint);
            line.colorGradient = beamColorGradient;
            line.gameObject.SetActive(previewVisible);
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Destroy preview beams
        if (previewBeams != null)
        {
            foreach (var beam in previewBeams)
                if (beam != null) Destroy(beam);
        }

        // Play appropriate explosion sound based on bomb type
        if (explosionAudio != null)
        {
            AudioClip explosionClip = null;
            
            if (CompareTag("FireBomb") && fireExplosionSound != null)
                explosionClip = fireExplosionSound;
            else if (CompareTag("WaterBomb") && waterExplosionSound != null)
                explosionClip = waterExplosionSound;
            else if (CompareTag("ElectricBomb") && electricExplosionSound != null)
                explosionClip = electricExplosionSound;
            
            if (explosionClip != null)
                explosionAudio.PlayOneShot(explosionClip);
            else
                explosionAudio.Play();
        }

        // FIXED: Spawn ground effects at discrete grid positions only
        SpawnGroundEffectsAlongBlast();

        DrawExplosionBeams();
        FlashBombMesh();

        GameObject explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);

        // Destroy bomb after short delay to allow audio/flash
        Destroy(gameObject, 0.6f);
    }

    // FIXED METHOD: Spawn ground effects at discrete grid positions only (1 per tile)
    // This prevents overlap and creates clean cross pattern
    private void SpawnGroundEffectsAlongBlast()
    {
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
        Collider selfCollider = GetComponent<Collider>();

        // Track which positions already have effects to prevent duplicates
        HashSet<Vector3> spawnedPositions = new HashSet<Vector3>();

        // Spawn at bomb position first (rounded to grid)
        Vector3 bombGridPos = new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y,
            Mathf.Round(transform.position.z)
        );
        SpawnGroundEffectAtPosition(bombGridPos);
        spawnedPositions.Add(bombGridPos);

        // Then spawn along each direction - ONE TILE AT A TIME
        foreach (var dir in directions)
        {
            // Check each discrete tile position along the blast range
            for (int i = 1; i <= Mathf.RoundToInt(blastRange); i++)
            {
                // Calculate grid position (whole numbers only - prevents fractional positions)
                Vector3 gridPosition = bombGridPos + (dir * i);
                
                // Skip if we already spawned here (prevents duplicates at corners)
                if (spawnedPositions.Contains(gridPosition))
                    continue;
                
                // Raycast to see if we hit a wall before this position
                RaycastHit[] hits = Physics.RaycastAll(bombGridPos, dir, i + 0.5f);
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                
                bool blocked = false;
                foreach (var hit in hits)
                {
                    if (hit.collider == null || hit.collider == selfCollider) continue;
                    
                    if (hit.collider.CompareTag("Wall"))
                    {
                        blocked = true;
                        break;
                    }
                }
                
                if (blocked) break; // Stop spawning in this direction
                
                // Spawn ground effect at this grid position
                SpawnGroundEffectAtPosition(gridPosition);
                spawnedPositions.Add(gridPosition);
            }
        }
    }

    // Helper method to spawn ground effect at specific position
    private void SpawnGroundEffectAtPosition(Vector3 position)
    {
        // Always use grid-aligned position for ground effects
        Vector3 groundPosition = new Vector3(
            Mathf.Round(position.x), 
            -0.412f,  // Ground level
            Mathf.Round(position.z)
        );

        // Fire Bomb Logic
        if (CompareTag("FireBomb") && burningGroundPrefab != null)
        {
            bool touchingWet = false;
            Collider[] hits = Physics.OverlapSphere(groundPosition, 0.45f);
            foreach (var h in hits)
            {
                if (h != null && h.CompareTag("WetGround"))
                {
                    touchingWet = true;
                    Destroy(h.gameObject); // Extinguish wet ground
                    break;
                }
            }

            if (!touchingWet)
            {
                GameObject burningGround = Instantiate(burningGroundPrefab, groundPosition, Quaternion.identity);
                
                // Add looping sound to burning ground
                if (burningGroundLoopSound != null)
                {
                    AudioSource groundAudio = burningGround.AddComponent<AudioSource>();
                    groundAudio.clip = burningGroundLoopSound;
                    groundAudio.loop = true;
                    groundAudio.spatialBlend = 1f; // 3D sound
                    groundAudio.volume = 0.3f;
                    groundAudio.Play();
                }
            }
        }
        // Water Bomb Logic
        else if (CompareTag("WaterBomb") && wetGroundPrefab != null)
        {
            // Don't spawn if there's already wet ground here
            bool alreadyWet = false;
            Collider[] hits = Physics.OverlapSphere(groundPosition, 0.45f);
            foreach (var h in hits)
            {
                if (h != null && (h.CompareTag("WetGround") || h.CompareTag("ElectrifiedWaterGround")))
                {
                    alreadyWet = true;
                    break;
                }
            }
            
            if (!alreadyWet)
            {
                GameObject wetGround = Instantiate(wetGroundPrefab, groundPosition, Quaternion.identity);
                
                // Add looping sound to wet ground
                if (wetGroundLoopSound != null)
                {
                    AudioSource groundAudio = wetGround.AddComponent<AudioSource>();
                    groundAudio.clip = wetGroundLoopSound;
                    groundAudio.loop = true;
                    groundAudio.spatialBlend = 1f;
                    groundAudio.volume = 0.2f;
                    groundAudio.Play();
                }
            }
        }
    }

    private void DrawExplosionBeams()
    {
        
        Collider selfCollider = GetComponent<Collider>();


         // Electrify any WetGround tiles in range for electric bombs
        if (CompareTag("ElectricBomb") && ElectrifiedWaterGroundPrefab != null)
        {
            Collider[] wetHits = Physics.OverlapSphere(transform.position, blastRange);
            foreach (var h in wetHits)
            {
                if (h != null && h.CompareTag("WetGround"))
                {
                    Vector3 pos = h.transform.position;
                    Vector3 groundPos = new Vector3(
                        Mathf.Round(pos.x),
                        -0.412f,               // same Y as WetGround
                        Mathf.Round(pos.z)
                    );

                    Destroy(h.gameObject);

                    GameObject electrifiedWater =
                        Instantiate(ElectrifiedWaterGroundPrefab, groundPos, Quaternion.identity);

                    if (electricFieldLoopSound != null)
                    {
                        AudioSource groundAudio = electrifiedWater.AddComponent<AudioSource>();
                        groundAudio.clip = electricFieldLoopSound;
                        groundAudio.loop = true;
                        groundAudio.spatialBlend = 1f;
                        groundAudio.volume = 0.4f;
                        groundAudio.Play();
                    }

                    Debug.Log($"WetGround electrified at {groundPos}");
                }
            }
        }



        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
        
        bool chainReactionOccurred = false;

        foreach (var dir in directions)
        {
            Vector3 endPoint = transform.position + dir * blastRange;
            Ray ray = new Ray(transform.position, dir);
            RaycastHit[] hits = Physics.RaycastAll(ray, blastRange);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (hit.collider == null || hit.collider == selfCollider) continue;

                if (hit.collider.CompareTag("Wall"))
                {
                    endPoint = hit.point;
                    break;
                }
                else if (hit.collider.CompareTag("Destructible"))
                {
                    PowerUpSpawner spawner = hit.collider.GetComponent<PowerUpSpawner>();
                    if (spawner != null)
                        spawner.SpawnPowerUp();

                    Destroy(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Bomb"))
                {
                    BombController other = hit.collider.GetComponent<BombController>();
                    if (other != null && other != this)
                    {
                        other.Explode();
                        chainReactionOccurred = true;
                    }
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    var hearts = hit.collider.GetComponentInParent<PlayerHearts>();
                    if (hearts) hearts.TakeDamage(damage);
                    Debug.Log("Player takes damage!");
                }
                else if (CompareTag("ElectricBomb") && hit.collider.CompareTag("WetGround"))
                {
                    Vector3 pos = hit.collider.transform.position;
                    Destroy(hit.collider.gameObject);

                    if (ElectrifiedWaterGroundPrefab != null)
                    {
                        GameObject electrifiedWater = Instantiate(ElectrifiedWaterGroundPrefab, pos, Quaternion.identity);
                        
                        // Add electric field sound
                        if (electricFieldLoopSound != null)
                        {
                            AudioSource groundAudio = electrifiedWater.AddComponent<AudioSource>();
                            groundAudio.clip = electricFieldLoopSound;
                            groundAudio.loop = true;
                            groundAudio.spatialBlend = 1f;
                            groundAudio.volume = 0.4f;
                            groundAudio.Play();
                        }
                        
                        Debug.Log("WetGround electrified!");
                    }
                }
                else if (CompareTag("WaterBomb") && hit.collider.CompareTag("BurningGround"))
                {
                    Destroy(hit.collider.gameObject);
                }
                else if (CompareTag("FireBomb") && hit.collider.CompareTag("IceBarrier"))
                {
                    Destroy(hit.collider.gameObject);
                }
                else if (CompareTag("FireBomb") && hit.collider.CompareTag("ElectrifiedWaterGround"))
                {
                    Destroy(hit.collider.gameObject);
                }
            }

            // Fallback for CharacterController-only players (WITH WALL CHECK)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
                float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
                
                // Only damage if player is within range AND in the same direction as this blast beam
                bool playerInThisDirection = Vector3.Dot(directionToPlayer, dir) > 0.8f;
                
                if (distToPlayer <= blastRange && playerInThisDirection)
                {
                    // Check if there's a wall between bomb and player
                    RaycastHit wallCheck;
                    bool wallBlocking = Physics.Raycast(transform.position, directionToPlayer, out wallCheck, distToPlayer);
                    
                    if (!wallBlocking || !wallCheck.collider.CompareTag("Wall"))
                    {
                        var hearts = player.GetComponent<PlayerHearts>();
                        if (hearts) hearts.TakeDamage(damage);
                    }
                }
            }

            // Create explosion beam
            if (blastBeamPrefab != null)
            {
                GameObject beam = Instantiate(blastBeamPrefab, transform.position, Quaternion.identity);
                LineRenderer lr = beam.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.positionCount = 2;
                    lr.SetPosition(0, transform.position);
                    lr.SetPosition(1, transform.position);
                    lr.startWidth = beamStartWidth;
                    lr.endWidth = beamEndWidth;
                    lr.colorGradient = beamColorGradient;
                    StartCoroutine(AnimateBeamGrowth(lr, endPoint, beamDuration));
                }
                Destroy(beam, beamDuration);
            }
        }
        
        // Play chain reaction sound if it occurred
        if (chainReactionOccurred && chainReactionSound != null && explosionAudio != null)
        {
            explosionAudio.PlayOneShot(chainReactionSound, 0.7f);
        }
    }

    private IEnumerator AnimateBeamGrowth(LineRenderer lr, Vector3 endPoint, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (lr == null)
                yield break;

            elapsed += Time.deltaTime;
            lr.SetPosition(1, Vector3.Lerp(transform.position, endPoint, elapsed / duration));
            yield return null;
        }
    }

    private void FlashBombMesh()
    {
        Renderer r = GetComponent<Renderer>();
        if (r != null && r.material.HasProperty("_EmissionColor"))
        {
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", Color.white * 10f);
        }
    }

    public void Defuse()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Remove preview beams
        if (previewBeams != null)
        {
            foreach (var beam in previewBeams)
                if (beam != null) Destroy(beam);
        }

        if (countdownText != null)
        {
            countdownText.text = "DEFUSED";
            countdownText.color = Color.cyan;
        }

        explosionAudio?.Stop();
        tickingAudioSource?.Stop();

        Destroy(gameObject, 0.2f);
    }
}