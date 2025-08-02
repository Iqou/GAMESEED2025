using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Variabel publik untuk menampung panel-panel UI
    public GameObject mainMenuPanel;
    public GameObject upgradePanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    // Metode Start() akan berjalan saat scene pertama kali dimuat
    void Start()
    {
        // Panggil metode untuk hanya menampilkan menu utama di awal
        ShowMainMenu();
    }

    // Metode untuk tombol Play
    public void PlayGame()
    {
        // Ganti nama scene permainan agar sesuai
        SceneManager.LoadScene("TestScene");
    }

    // Metode untuk tombol Quit
    public void QuitGame()
    {
        Application.Quit();
    }

    // Metode untuk mengalihkan ke panel Main Menu
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    // Metode untuk mengalihkan ke panel Upgrade
    public void ShowUpgrade()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
    
    // Metode untuk mengalihkan ke panel Options
    public void ShowOptions()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    // Metode untuk mengalihkan ke panel Credits
    public void ShowCredits()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }
}