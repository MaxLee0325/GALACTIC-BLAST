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

    private void SelectHero(Hero hero)
    {
        SelectedHero = hero;
        Debug.Log("Selected Hero: " + hero);

        // Load Level 1 after picking the character
        SceneManager.LoadScene("Level 1");
    }

    public void GoBack(){
        SceneManager.LoadScene("Main Menu");
    }

    // Button events
    public void SelectScout() => SelectHero(Hero.Scout);
    public void SelectTanya() => SelectHero(Hero.Tanya);
    public void SelectMediv() => SelectHero(Hero.Mediv);
}
