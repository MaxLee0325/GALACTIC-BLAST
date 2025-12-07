using UnityEngine;
using TMPro;
using System.Collections;

public class SkillVisualization : MonoBehaviour
{
    public int coolDown;
    public bool isCoolingDown = false;
    public TMP_Text statusText;
    private Transform icon; 
    private Vector3 originalScale;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadHero();
        icon = transform;
        originalScale = icon.localScale;
        Transform statusTransform = transform.Find("StatusText");
        statusText = statusTransform.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadHero(){
        switch (HeroSelect.SelectedHero)
        {
            case HeroSelect.Hero.Scout:
                coolDown = 15;
                showIcon("DashIcon");
                break;

            case HeroSelect.Hero.Tanya:
                coolDown = 15;
                showIcon("MegaBombIcon");
                break;

            case HeroSelect.Hero.Mediv:
                coolDown = 20;
                showIcon("ProtectIcon");
                break;
        }
    }

    private void showIcon(string iconName){
        Transform dashIconTransform = transform.Find("SkillIcon/" + iconName);
        dashIconTransform.gameObject.SetActive(true);
    }

    public void startCountDown(){
        StartCoroutine(CountdownRoutine());
    }

    public void Pop(){
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        
        
        Vector3 popScale = originalScale * 1.2f;   // 20% bigger

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

    private IEnumerator CountdownRoutine()
    {
        isCoolingDown = true;
        int remainingTime = coolDown;

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
