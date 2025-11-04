using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHearts : MonoBehaviour
{
    [Header("Health")]
    public int maxHearts = 5;         // fixed to 5 for your case
    [Range(0, 5)] public int currentHearts = 5;

    [Header("UI")]
    public List<Image> heartImages;   // 5 Image components in order (left to right)
    public Sprite fullHeart;          // sprite for filled heart
    public Sprite emptyHeart;         // sprite for empty heart

    [Header("Optional")]
    public bool clampToMaxList = true; // if true, only first N images are used

    void Awake()
    {
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);
        RefreshUI();
    }

    public void TakeDamage(int amount = 1)
    {
        if (currentHearts <= 0) return;
        currentHearts = Mathf.Max(0, currentHearts - Mathf.Abs(amount));
        RefreshUI();

        if (currentHearts <= 0)
        {
            // TODO: handle death (disable control, reload menu, etc.)
            // For now just log:
            Debug.Log("Player died (hearts reached zero).");
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

    private void RefreshUI()
    {
        if (heartImages == null || heartImages.Count == 0) return;

        int slots = clampToMaxList ? Mathf.Min(maxHearts, heartImages.Count) : heartImages.Count;

        for (int i = 0; i < slots; i++)
        {
            bool filled = i < currentHearts;
            if (heartImages[i] != null)
                heartImages[i].sprite = filled ? fullHeart : emptyHeart;
        }

        for (int i = maxHearts; i < heartImages.Count; i++)
            if (heartImages[i] != null) heartImages[i].enabled = false;
    }
}
