using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("TestScene");
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
