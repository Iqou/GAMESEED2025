using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public GameObject homescreenPanel;
    public GameObject mainMenuPanel;
    public GameObject upgradePanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;
    
    public MenuKeyboardNavigation menuKeyboardNavigation;
    public float fadeDuration = 0.5f;

    void Start()
    {
        ShowPanel(homescreenPanel, true);
        ShowPanel(mainMenuPanel, false);
        ShowPanel(upgradePanel, false);
        ShowPanel(optionsPanel, false);
        ShowPanel(creditsPanel, false);
    }
    
    // Metode untuk memulai transisi fade
    public void ShowMainMenuWithFade()
    {
        StartCoroutine(FadeOutThenIn(homescreenPanel, mainMenuPanel));
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
    
    // Coroutine untuk transisi fade in dan fade out
    private IEnumerator FadeOutThenIn(GameObject panelToFadeOut, GameObject panelToFadeIn)
    {
        CanvasGroup fadeOutGroup = GetCanvasGroup(panelToFadeOut);
        CanvasGroup fadeInGroup = GetCanvasGroup(panelToFadeIn);

        if (fadeOutGroup != null)
        {
            float time = 0;
            while (time < fadeDuration)
            {
                fadeOutGroup.alpha = Mathf.Lerp(1, 0, time / fadeDuration);
                time += Time.deltaTime;
                yield return null;
            }
            fadeOutGroup.alpha = 0;
            panelToFadeOut.SetActive(false);
        }

        if (fadeInGroup != null)
        {
            panelToFadeIn.SetActive(true);
            float time = 0;
            while (time < fadeDuration)
            {
                fadeInGroup.alpha = Mathf.Lerp(0, 1, time / fadeDuration);
                time += Time.deltaTime;
                yield return null;
            }
            fadeInGroup.alpha = 1;
        }
    }
    
    private CanvasGroup GetCanvasGroup(GameObject panel)
    {
        if (panel == null) return null;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = panel.AddComponent<CanvasGroup>();
        }
        return cg;
    }
}