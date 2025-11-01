using UnityEngine;
using TMPro; // Required if using TextMeshPro

public class BombController : MonoBehaviour
{
    public float explosionTime = 4f;     // Total time before boom
    public float blastRange = 2f;
    
    [Header("Pulse Animation")]
    public float pulseSpeed = 3f;        // Speed of the pulsing (higher = faster)
    public float pulseAmplitude = 0.05f;  // How much it swells/shrinks (0.1-0.2 recommended for subtle drama)

    private TextMeshProUGUI countdownText; // Reference to the child UI text
    private Vector3 initialScale;          // Store original scale for animation baseline

    void Start()
    {
        initialScale = transform.localScale; // Capture starting scale (usually 1,1,1)
        
        // Find the child canvas and the text component
        countdownText = GetComponentInChildren<TextMeshProUGUI>();

        if (countdownText != null)
        {
            // Position the text GameObject right above the bomb (adjust 1f based on your bomb's radius/scale)
            countdownText.transform.localPosition = new Vector3(0, 1f, 0); // 1 unit above center; e.g., for radius 0.5f sphere
            
            // Fix text scale to prevent it from pulsing with the bomb
            countdownText.transform.localScale = Vector3.one; // Or set to your desired fixed size, e.g., (0.01, 0.01, 0.01) for World Space Canvas
            
            countdownText.text = "";
            countdownText.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            Debug.LogError("No TextMeshProUGUI found as child! Check hierarchy."); // Debug: Missing component
        }
    }

    void Update()
    {
        // Pulsing animation: Sine wave for breathing effect only (no net growth)
        if (explosionTime > 0f)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            Vector3 scale = initialScale * (1f + pulse);
            transform.localScale = scale;
        }

        // Billboard effect (make the text face the camera)
        if (countdownText != null && Camera.main != null)
        {
            countdownText.transform.LookAt(Camera.main.transform);
            countdownText.transform.Rotate(0, 180, 0); // Correct the reversed facing
        }

        // Countdown timer
        explosionTime -= Time.deltaTime;

        // Fixed logic: Use descending thresholds for exclusivity
        if (explosionTime <= 0f)
        {
            if (countdownText != null)
            {
                countdownText.text = "BOOM";
                countdownText.color = Color.red;
            }
            // Optional: Trigger explosion (e.g., damage nearby, particles)
            Explode();
            enabled = false; // Stop this script after boom
        }
        else if (explosionTime < 1f)
        {
            if (countdownText != null)
            {
                countdownText.text = "1";
                countdownText.color = Color.yellow;
            }
        }
        else if (explosionTime < 2f)
        {
            if (countdownText != null)
            {
                countdownText.text = "2";
                countdownText.color = Color.green;
            }
        }
        else if (explosionTime < 3f)
        {
            if (countdownText != null)
            {
                countdownText.text = "3";
                countdownText.color = Color.white;
            }
        }
        else // Covers 4f to 3f
        {
            if (countdownText != null)
            {
                countdownText.text = "4";
                countdownText.color = Color.white;
            }
        }
    }

    private void Explode()
    {
        // Example explosion: Destroy nearby objects or add effects
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, blastRange);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Destructible")) // Tag your breakable objects
            {
                Destroy(hit.gameObject);
            }
        }
        // Optional: Add particle system or sound here
        Destroy(gameObject, 0.5f); // Self-destruct after delay
    }
}