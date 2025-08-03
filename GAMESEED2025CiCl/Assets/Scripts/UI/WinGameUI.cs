using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class WinGameUI : MonoBehaviour
{
    public static WinGameUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject winGamePanel;
    public TextMeshProUGUI rupiahCollectedText;
    public TextMeshProUGUI bossesKilledText;
    public TextMeshProUGUI soundChipsEarnedText;
    public TextMeshProUGUI timePlayedText;
    public TextMeshProUGUI levelReachedText;
    public Button returnToMenuButton;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Start with the panel hidden and fully transparent
        canvasGroup.alpha = 0f;
        winGamePanel.SetActive(false);
    }

    public void ShowGameOverScreen(int rupiah, int bosses, int soundChips, float time, int level)
    {
        // Update the text fields with the final stats
        if (rupiahCollectedText != null) rupiahCollectedText.text = $"Rupiah Collected: {rupiah}";
        if (bossesKilledText != null) bossesKilledText.text = $"Bosses Defeated: {bosses}";
        if (soundChipsEarnedText != null) soundChipsEarnedText.text = $"SoundChips Earned: {soundChips}";
        if (levelReachedText != null) levelReachedText.text = $"Level Reached: {level}";

        // Format and display the time
        if (timePlayedText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timePlayedText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
        }

        // Show the panel and start the fade-in
        winGamePanel.SetActive(true);
        StartCoroutine(FadeIn(1.5f)); // 3-second fade duration
    }

    public IEnumerator FadeIn(float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public void ReturnToMainMenu()
    {
        // Unpause the game before changing scenes
        Time.timeScale = 1f;
        // Load the Main Menu scene (assuming it's at build index 0)
        SceneManager.LoadScene(0);
    }
}
