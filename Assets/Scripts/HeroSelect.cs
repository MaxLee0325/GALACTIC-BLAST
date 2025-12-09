using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroSelect : MonoBehaviour
{
    public enum Hero
    {
        Scout,
        Tanya,
        Mediv
    }

    public static Hero SelectedHero { get; private set; }

    // Function to store selected hero and load the correct level
    private void SelectHero(Hero hero)
    {
        // Store chosen hero for the next scene
        SelectedHero = hero;
        Debug.Log("Selected Hero: " + hero);

        // Load level according to selected level
        string levelToLoad = "Level 1";
        switch(PanelControl.SelectedLevel)
        {
            case PanelControl.Level.Level1:
                levelToLoad = "Level 1";
                break;
            case PanelControl.Level.Level2:
                levelToLoad = "Level 2";
                break;
            case PanelControl.Level.Level3:
                levelToLoad = "Level 3";
                break;
        }

        SceneManager.LoadScene(levelToLoad);
    }

    // Function to go back to main menu
    public void GoBack(){
        SceneManager.LoadScene("Main Menu");
    }

    // Button methods: select specific heroes
    public void SelectScout() => SelectHero(Hero.Scout);
    public void SelectTanya() => SelectHero(Hero.Tanya);
    public void SelectMediv() => SelectHero(Hero.Mediv);
}
