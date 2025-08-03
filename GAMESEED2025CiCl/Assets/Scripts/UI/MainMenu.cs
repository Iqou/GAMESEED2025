using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Directly load the scene named "game". Ensure this scene is in your Build Settings.
        SceneManager.LoadScene("game");
    }

    public void UpgradeMenu()
    {
        Debug.Log("Upgrade button clicked.");
    }

    public void OptionsMenu()
    {
        Debug.Log("Options button clicked.");
    }

    public void CreditsMenu()
    {
        Debug.Log("Credits button clicked.");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

