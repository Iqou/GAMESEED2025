using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject homescreenPanel;
    public GameObject mainMenuPanel;
    public GameObject upgradePanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    public MenuKeyboardNavigation menuKeyboardNavigation;

    void Start()
    {
        ShowPanel(homescreenPanel, true);
        ShowPanel(mainMenuPanel, false);
        ShowPanel(upgradePanel, false);
        ShowPanel(optionsPanel, false);
        ShowPanel(creditsPanel, false);
    }
    
    public void ShowMainMenuWithFade()
    {
        ShowPanel(homescreenPanel, false);
        ShowMainMenu();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("TestScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel, true);
        ShowPanel(upgradePanel, false);
        ShowPanel(optionsPanel, false);
        ShowPanel(creditsPanel, false);
        
        if (menuKeyboardNavigation != null)
        {
            menuKeyboardNavigation.Initialize();
        }
    }

    public void ShowUpgrade()
    {
        ShowPanel(mainMenuPanel, false);
        ShowPanel(upgradePanel, true);
        ShowPanel(optionsPanel, false);
        ShowPanel(creditsPanel, false);
    }
    
    public void ShowOptions()
    {
        ShowPanel(mainMenuPanel, false);
        ShowPanel(upgradePanel, false);
        ShowPanel(optionsPanel, true);
        ShowPanel(creditsPanel, false);
    }

    public void ShowCredits()
    {
        ShowPanel(mainMenuPanel, false);
        ShowPanel(upgradePanel, false);
        ShowPanel(optionsPanel, false);
        ShowPanel(creditsPanel, true);
    }

    private void ShowPanel(GameObject panel, bool state)
    {
        if (panel != null) panel.SetActive(state);
    }
}