using UnityEngine;
using TMPro;
using System.Collections;

public class SkillVisualization : MonoBehaviour
{
    public int coolDown;
    public bool isCoolingDown = false;
    public TMP_Text statusText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadHero();
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

    private IEnumerator CountdownRoutine()
    {
        isCoolingDown = true;
        int remainingTime = coolDown;

        while (remainingTime > 0)
        {
            statusText.text = remainingTime.ToString();
            Debug.Log("Count Down: " + statusText.text);
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }
        statusText.text = "Skill Ready";
    
        isCoolingDown = false;
    }


}
