using UnityEngine;
using TMPro;
using System.Collections;

public class SkillVisualization : MonoBehaviour
{
    private float coolDown = 8f;
    public bool isCoolingDown = false;
    public TMP_Text statusText;
    private Transform icon; 
    private Vector3 originalScale;

    //Initializes the hero-specific icon, scale, and UI
    void Start()
    {
        LoadHero();
        icon = transform;
        originalScale = icon.localScale;
        Transform statusTransform = transform.Find("StatusText");
        statusText = statusTransform.GetComponent<TMP_Text>();
    }

    //Shows the correct skill icon based on hero choice
    private void LoadHero(){
        switch (HeroSelect.SelectedHero)
        {
            case HeroSelect.Hero.Scout:
                showIcon("DashIcon");
                break;

            case HeroSelect.Hero.Tanya:
                showIcon("MegaBombIcon");
                break;

            case HeroSelect.Hero.Mediv:
                showIcon("ProtectIcon");
                break;
        }
    }

    //Activates the icon GameObject for the selected hero
    private void showIcon(string iconName){
        Transform dashIconTransform = transform.Find("SkillIcon/" + iconName);
        dashIconTransform.gameObject.SetActive(true);
    }

    //Begins the cooldown text countdown
    public void startCountDown(){
        StartCoroutine(CountdownRoutine());
    }

    //Plays a quick bounce animation when skill is unavailable
    public void Pop(){
        StartCoroutine(PopRoutine());
    }

    //Handles the bounce animation for "skill on cooldown"
    private IEnumerator PopRoutine()
    {
        
        
        Vector3 popScale = originalScale * 1.2f; 

        float popTime = 0.1f;
        float returnTime = 0.1f;

        // Scale up
        float t = 0f;
        while (t < popTime)
        {
            t += Time.deltaTime;
            float progress = t / popTime;
            icon.localScale = Vector3.Lerp(originalScale, popScale, progress);
            yield return null;
        }

        // Scale back
        t = 0f;
        while (t < returnTime)
        {
            t += Time.deltaTime;
            float progress = t / returnTime;
            icon.localScale = Vector3.Lerp(popScale, originalScale, progress);
            yield return null;
        }

        icon.localScale = originalScale; // ensure perfect reset
    }

    //Counts down from cooldown to 0 and updates UI text
    private IEnumerator CountdownRoutine()
    {
        isCoolingDown = true;
        float remainingTime = coolDown;

        while (remainingTime > 0)
        {
            statusText.text = remainingTime.ToString();
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }
        statusText.text = "Skill Ready";
    
        isCoolingDown = false;
    }


}
