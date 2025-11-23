using UnityEngine;
using TMPro;
using System.Collections;

public class BombController : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionTime = 4f;       // Time before explosion
    public float blastRange = 2f;
    public GameObject blastBeamPrefab;     // Prefab with LineRenderer
    public float beamDuration = 0.3f;      // Explosion beam duration

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

    [SerializeField] private AudioSource explosionAudio;

    private TextMeshProUGUI countdownText;
    private Vector3 initialScale;
    private bool hasExploded = false;

    // Separate arrays for preview and explosion
    private GameObject[] previewBeams;
    public GameObject PreviewBeamPrefab;     // Prefab with LineRenderer
    private float blinkTimer = 0f;
    private bool previewVisible = true;

    [Header("Ground Effects")]
    public GameObject burningGroundPrefab;
    public GameObject wetGroundPrefab;
    public GameObject ElectrifiedWaterGroundPrefab;

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

        // Preview blinking speed
        float blinkSpeed = explosionTime <= 0.6f ? 10f : (explosionTime <= 2f ? 4f : 2f);
        blinkTimer += Time.deltaTime * blinkSpeed;
        previewVisible = Mathf.Sin(blinkTimer) > 0;

        // Show blast preview
        if(!hasExploded)
        {
            ShowBlastPreview();
        }

        // Make countdown face camera
        if (countdownText != null && Camera.main != null)
        {
            countdownText.transform.LookAt(Camera.main.transform);
            countdownText.transform.Rotate(0, 180, 0);
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

        // Instantiate ground effect based on tag
        Vector3 groundPosition = new Vector3(transform.position.x, -0.412f, transform.position.z);

        // This ensures burning ground will not be created if the bomb is on wet ground
        if (CompareTag("FireBomb"))
        {
            bool touchingWet = false;

            Collider[] hits = Physics.OverlapSphere(transform.position, 0.45f);
            foreach (var h in hits)
            {
                if (h != null && h.CompareTag("WetGround"))
                {
                    touchingWet = true;
                    break;
                }
            }

            if (!touchingWet)
            {
                Instantiate(burningGroundPrefab, groundPosition, Quaternion.identity);
            }
        }


        else if (CompareTag("WaterBomb"))
        {
            Instantiate(wetGroundPrefab, groundPosition, Quaternion.identity);
        }


        DrawExplosionBeams();
        FlashBombMesh();

        if (previewBeams != null)
        {
            foreach (GameObject beam in previewBeams)
            {
                if (beam != null)
                {
                    Destroy(beam);
                }
            }
        }

        explosionAudio?.Play();

        // Destroy bomb after short delay to allow audio/flash
        Destroy(gameObject, 0.5f);
        
    }

    private void DrawExplosionBeams()
    {
        Collider selfCollider = GetComponent<Collider>();
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };

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
                    Destroy(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Bomb"))
                {
                    BombController other = hit.collider.GetComponent<BombController>();
                    if (other != null && other != this) other.Explode();
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    var hearts = hit.collider.GetComponentInParent<PlayerHearts>();
                    if (hearts) hearts.TakeDamage(1);
                    Debug.Log("Player takes damage!");
                }
                else if (CompareTag("ElectricBomb") && hit.collider.CompareTag("WetGround"))
                {
                    // Save position before destroying
                    Vector3 pos = hit.collider.transform.position;

                    // Destroy the wet ground
                    Destroy(hit.collider.gameObject);

                    // Instantiate electrified version
                    if (ElectrifiedWaterGroundPrefab != null) // Make sure you have assigned this prefab
                    {
                        Instantiate(ElectrifiedWaterGroundPrefab, pos, Quaternion.identity);
                        Debug.Log("WetGround electrified!");
                    }
                }
                // Water counters fire
                else if (CompareTag("WaterBomb") && hit.collider.CompareTag("BurningGround"))
                {
                    Destroy(hit.collider.gameObject);
                }
                // Ice barrier can only be destroyed by fire bomb
                else if (CompareTag("FireBomb") && hit.collider.CompareTag("IceBarrier"))
                {
                    Destroy(hit.collider.gameObject);
                }

                //TODO: Wait for these objects to be created
                // // Metal barrier has 2 health
                // else if (hit.collider.CompareTag("MetalLocker"))
                // {
                //     var mt = hit.collider.GetComponentInParent<MetalLocker>();
                //     if (mt) mt.TakeDamage(1);
                //     Debug.Log("MetalLocker Got hit!");
                // }
                // // Electronic door can be opened by electric bomb
                // else if (CompareTag("ElectricBomb") && hit.collider.CompareTag("ElectronicDoor"))
                // {
                //     var ed = hit.collider.GetComponentInParent<ElectricDoor>();
                //     if (ed) mt.Open();
                //     Debug.Log("Electronic Door Opened!");
                // }
                // else if (CompareTag("ElectricBomb") && hit.collider.CompareTag("Enemy"))
                // {
                //     var em = hit.collider.GetComponentInParent<Enemy>();
                //     if (em) em.takeElectricDamage();
                //     Debug.Log("ElectricChainDamage to enemy!");
                // }
            }

            // Fallback (for CharacterController-only players without a Collider):
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist <= blastRange)
                {
                    var hearts = player.GetComponent<PlayerHearts>();
                    if (hearts) hearts.TakeDamage(1);
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
                    lr.SetPosition(1, transform.position); // Start at bomb
                    lr.startWidth = beamStartWidth;
                    lr.endWidth = beamEndWidth;
                    lr.colorGradient = beamColorGradient;
                    StartCoroutine(AnimateBeamGrowth(lr, endPoint, beamDuration));
                }
                Destroy(beam, beamDuration);
            }
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
}
