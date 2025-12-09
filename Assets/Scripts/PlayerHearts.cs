using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHearts : MonoBehaviour
{
    private int maxHearts = 5;
    private int currentHearts = 5;

    [Header("UI Prefab & Parent")]
    public GameObject heartPrefab; 
    public Transform heartsParent; 
    public Sprite fullHeart;       
    public Sprite emptyHeart;      

    [Header("Optional")]
    public bool clampToMaxList = true; 

    [Header("Damage")]
    public float damageCooldown = 0.4f;
    float lastHitTime = -999f;

    [Header("Lose Panel")]
    public GameObject youLostPanel;

    public bool isInvincible = false;
    public float invincibleEndTime = 0f;

    private List<Image> heartImages = new List<Image>();

    [SerializeField] private PlayerControl playerControl;

    [SerializeField] 
    public AudioSource hurtAudio;
    public AudioSource healAudio;

    //Initializes hearts and adjusts for Tanya hero
    void Awake()
    {
        // Tanya has 7 hearts
        if(HeroSelect.SelectedHero == HeroSelect.Hero.Tanya)
        {
            maxHearts = 7;
            currentHearts = 7;
        }

        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        GenerateHearts();
        RefreshUI();
    }

    //Handles invincibility timer expiration
    void Update()
    {
        if (isInvincible && Time.time >= invincibleEndTime)
        {
            isInvincible = false;
            playerControl.protectionShield.SetActive(false); 
            Debug.Log("Invincibility ended.");
        } 
    }

    //Makes the player invincible for 3 seconds for medic skill
    public void Protect()
    {
        isInvincible = true;
        playerControl.protectionShield.SetActive(true);
        invincibleEndTime = Time.time + 3f;
        Debug.Log("Player is now invincible for 3 seconds!");
    }

    //Reduces hearts, updates UI, triggers lose panel if 0
    public void TakeDamage(int amount = 1)
    {
        if (Time.time - lastHitTime < damageCooldown || isInvincible) return;
        lastHitTime = Time.time;

        if (currentHearts <= 0) return;

        currentHearts = Mathf.Max(0, currentHearts - Mathf.Abs(amount));
        hurtAudio.Play();
        RefreshUI();

        if (currentHearts <= 0)
        {
            youLostPanel.SetActive(true);
            GameManager.Instance.SetGameState(GameManager.GameState.Lost);
            Time.timeScale = 0;
        }
    }

    //Restores hearts up to maximum and refreshes UI
    public void Heal(int amount = 1)
    {
        if (currentHearts >= maxHearts) return;
        currentHearts = Mathf.Min(maxHearts, currentHearts + Mathf.Abs(amount));
        healAudio.Play();
        RefreshUI();
    }

    //Directly sets the player's heart count
    public void SetHealth(int hearts)
    {
        currentHearts = Mathf.Clamp(hearts, 0, maxHearts);
        RefreshUI();
    }

    //Heals the player if not already full
    public void PickupHeart()
    {
        if (currentHearts < maxHearts)
        {
            Heal(1);
        }
    }

    //Creates UI heart icons dynamically
    private void GenerateHearts()
    {
        // Remove old icon
        foreach (Transform child in heartsParent)
            Destroy(child.gameObject);
        heartImages.Clear();

        // Create new hearts
        for (int i = 0; i < maxHearts; i++)
        {
            GameObject heartGO = Instantiate(heartPrefab, heartsParent);
            Image heartImage = heartGO.GetComponent<Image>();
            if (heartImage != null)
                heartImages.Add(heartImage);
        }
    }

    // Updates each heart icon based on current health
    private void RefreshUI()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < maxHearts - currentHearts)
            {
                heartImages[i].sprite = emptyHeart;
                heartImages[i].enabled = true;
            }
            else
            {
                heartImages[i].sprite = fullHeart;
                heartImages[i].enabled = true;
            }
        }
    }

}
