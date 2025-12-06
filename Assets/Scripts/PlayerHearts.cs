using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHearts : MonoBehaviour
{
    private int maxHearts = 5;
    private int currentHearts = 5;

    [Header("UI Prefab & Parent")]
    public GameObject heartPrefab; // prefab with Image component
    public Transform heartsParent; // panel or empty object to hold hearts
    public Sprite fullHeart;       // sprite for filled heart
    public Sprite emptyHeart;      // sprite for empty heart

    [Header("Optional")]
    public bool clampToMaxList = true; // unused now, kept for compatibility

    [Header("Damage")]
    public float damageCooldown = 0.4f;
    float lastHitTime = -999f;

    [Header("Lose Panel")]
    public GameObject youLostPanel;

    public bool isInvincible = false;
    public float invincibleEndTime = 0f;

    private List<Image> heartImages = new List<Image>();

    [SerializeField] private PlayerControl playerControl;

    void Awake()
    {
        // Example: Tanya has 7 hearts
        if(HeroSelect.SelectedHero == HeroSelect.Hero.Tanya)
        {
            maxHearts = 7;
            currentHearts = 7;
        }

        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        // Generate hearts dynamically
        GenerateHearts();
        RefreshUI();
    }

    void Update()
    {
        if (isInvincible && Time.time >= invincibleEndTime)
        {
            isInvincible = false;
            playerControl.protectionShield.SetActive(false); 
            Debug.Log("Invincibility ended.");
        } 
    }

    public void Protect()
    {
        isInvincible = true;
        playerControl.protectionShield.SetActive(true);
        invincibleEndTime = Time.time + 3f; // 3 seconds
        Debug.Log("Player is now invincible for 3 seconds!");
    }

    public void TakeDamage(int amount = 1)
    {
        if (Time.time - lastHitTime < damageCooldown || isInvincible) return;
        lastHitTime = Time.time;

        if (currentHearts <= 0) return;

        currentHearts = Mathf.Max(0, currentHearts - Mathf.Abs(amount));
        RefreshUI();

        if (currentHearts <= 0)
        {
            Debug.Log("Player died (hearts reached zero).");
            if (youLostPanel != null)
            {
                youLostPanel.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    public void Heal(int amount = 1)
    {
        if (currentHearts >= maxHearts) return;
        currentHearts = Mathf.Min(maxHearts, currentHearts + Mathf.Abs(amount));
        RefreshUI();
    }

    public void SetHealth(int hearts)
    {
        currentHearts = Mathf.Clamp(hearts, 0, maxHearts);
        RefreshUI();
    }

    public void PickupHeart()
    {
        if (currentHearts < maxHearts)
        {
            Heal(1);
        }
    }

    private void GenerateHearts()
    {
        // Clear old hearts if any
        foreach (Transform child in heartsParent)
            Destroy(child.gameObject);
        heartImages.Clear();

        // Instantiate new hearts
        for (int i = 0; i < maxHearts; i++)
        {
            GameObject heartGO = Instantiate(heartPrefab, heartsParent);
            Image heartImage = heartGO.GetComponent<Image>();
            if (heartImage != null)
                heartImages.Add(heartImage);
        }
    }

    private void RefreshUI()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < maxHearts - currentHearts)
            {
                heartImages[i].sprite = emptyHeart; // lost heart
                heartImages[i].enabled = true;
            }
            else
            {
                heartImages[i].sprite = fullHeart;  // remaining heart
                heartImages[i].enabled = true;
            }
        }
    }

}
