using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[RequireComponent(typeof(CanvasGroup))]
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [Header("Player Info")]
    public TextMeshProUGUI playerInfoText; // For "Rp. xxx Lv. xxx"
    public TextMeshProUGUI timeText;
    public Image healthBar;
    public Image mamadPortrait;
    public TextMeshProUGUI wantedLevelText; // Using text for angry emotes

    [Header("Weapon Slots")]
    public List<Image> speakerSlotIcons;
    public List<TextMeshProUGUI> speakerCooldownTexts;
    public List<Image> speakerCooldownFills; // For the white fill overlay
    public Sprite lockedSlotSprite; // Assign a sprite for locked slots

    [Header("Rhythm Elements")]
    public Image beatIndicator;
    public TextMeshProUGUI hitFeedbackText;
    public float beatIndicatorPulseScale = 1.2f;
    public float pulseDuration = 0.1f;
    public float feedbackTextFadeDuration = 0.5f;

    [Header("Cooldown Animation")]
    // No animation properties needed for the new alpha-based cooldown.

    [Header("Player Level")]
    public RectTransform expBar; // Assign the RectTransform of the screen-width image
    public float expAnimationDuration = 0.5f;
    public float levelUpAnimationDuration = 0.4f;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;

    private PlayerStats playerStats;
    private Coroutine expAnimationCoroutine;
    private Coroutine beatIndicatorCoroutine;
    private Coroutine hitFeedbackCoroutine;
    private StringBuilder wantedStringBuilder = new StringBuilder();
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
    }

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found in scene!");
            return;
        }

        PlayerStats.OnStatsChanged += UpdateUI;
        PlayerStats.OnPlayerLevelUp += TriggerLevelUpAnimation;
        UpdateUI();

        // Subscribe to the beat event from the RhythmManager
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnBeat.AddListener(PulseBeatIndicator);
        }
        hitFeedbackText.text = ""; // Start with empty feedback text
    }

    void OnDestroy()
    {
        PlayerStats.OnStatsChanged -= UpdateUI;
        PlayerStats.OnPlayerLevelUp -= TriggerLevelUpAnimation;

        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnBeat.RemoveListener(PulseBeatIndicator);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        // Update the timer every frame
        if (playerStats != null && timeText != null)
        {
            float time = playerStats.timePlayedThisRun;
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void UpdateUI()
    {
        if (playerStats == null) return;

        // Combined Player Info Text
        playerInfoText.text = $"Rp. {playerStats.money}  Lv. {playerStats.level}";

        // Set initial EXP bar state without animation
        float initialExpScale = (float)playerStats.currentExperience / playerStats.maxExperience;
        expBar.localScale = new Vector3(initialExpScale, 1, 1);
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        healthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    public void SetWantedLevel(int level)
    {
        wantedStringBuilder.Clear();
        for (int i = 0; i < level; i++)
        {
            wantedStringBuilder.Append("😠"); // Add one angry emote per level
        }
        wantedLevelText.text = wantedStringBuilder.ToString();
    }
    
    public void AnimateExperienceBar(int fromExp, int toExp, int maxExp)
    {
        float fromScale = (float)fromExp / maxExp;
        float toScale = (float)toExp / maxExp;

        if (expAnimationCoroutine != null)
        {
            StopCoroutine(expAnimationCoroutine);
        }
        expAnimationCoroutine = StartCoroutine(AnimateExpBarCoroutine(fromScale, toScale, expAnimationDuration));
    }

    private void TriggerLevelUpAnimation()
    {
        if (expAnimationCoroutine != null)
        {
            StopCoroutine(expAnimationCoroutine);
        }
        expAnimationCoroutine = StartCoroutine(LevelUpAnimationCoroutine());
    }

    private IEnumerator AnimateExpBarCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        Vector3 scale = expBar.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newX = Mathf.Lerp(from, to, elapsed / duration);
            expBar.localScale = new Vector3(newX, scale.y, scale.z);
            yield return null;
        }
        expBar.localScale = new Vector3(to, scale.y, scale.z);
    }

    private IEnumerator LevelUpAnimationCoroutine()
    {
        // Animate from current fill to full
        yield return AnimateExpBarCoroutine(expBar.localScale.x, 1f, levelUpAnimationDuration);
        
        // Wait a moment
        yield return new WaitForSeconds(0.2f);

        // Animate from full to empty
        yield return AnimateExpBarCoroutine(1f, 0f, levelUpAnimationDuration);
    }

    public void UpdateWeaponSlot(int slotIndex, Texture2D icon, float currentCooldown, float maxCooldown)
    {
        if (slotIndex >= speakerSlotIcons.Count) return;

        Image iconImage = speakerSlotIcons[slotIndex];
        Color iconColor = iconImage.color;

        // Deactivate the text and fill elements permanently
        speakerCooldownTexts[slotIndex].gameObject.SetActive(false);
        speakerCooldownFills[slotIndex].gameObject.SetActive(false);

        if (icon == null)
        {
            iconImage.sprite = lockedSlotSprite;
            iconColor.a = 1f; // Make sure locked icon is visible
            iconImage.color = iconColor;
            return;
        }

        iconImage.sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2(0.5f, 0.5f));

        if (currentCooldown > 0 && maxCooldown > 0)
        {
            // Cooldown is active, set alpha based on progress
            iconColor.a = 1f - (currentCooldown / maxCooldown);
        }
        else
        {
            // Cooldown is finished, icon is fully visible
            iconColor.a = 1f;
        }
        iconImage.color = iconColor;
    }

    public void PulseBeatIndicator()
    {
        if (beatIndicatorCoroutine != null)
        {
            StopCoroutine(beatIndicatorCoroutine);
        }
        beatIndicatorCoroutine = StartCoroutine(PulseCoroutine(beatIndicator.transform));
    }

    public void ShowHitFeedback(HitQuality quality)
    {
        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);
        }
        hitFeedbackCoroutine = StartCoroutine(HitFeedbackCoroutine(quality));
    }

    private IEnumerator HitFeedbackCoroutine(HitQuality quality)
    {
        hitFeedbackText.text = quality.ToString();
        hitFeedbackText.alpha = 1f;

        // You can set colors based on quality here
        switch (quality)
        {
            case HitQuality.Perfect:
                hitFeedbackText.color = Color.yellow;
                break;
            case HitQuality.Good:
                hitFeedbackText.color = Color.green;
                break;
            default:
                hitFeedbackText.color = Color.gray;
                break;
        }

        yield return new WaitForSeconds(0.1f); // Show text briefly
        
        float timer = 0;
        while (timer < feedbackTextFadeDuration)
        {
            timer += Time.deltaTime;
            hitFeedbackText.alpha = Mathf.Lerp(1f, 0f, timer / feedbackTextFadeDuration);
            yield return null;
        }
        hitFeedbackText.text = "";
    }

    private IEnumerator PulseCoroutine(Transform target)
    {
        target.localScale = Vector3.one;
        float timer = 0;
        while (timer < pulseDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(beatIndicatorPulseScale, 1f, timer / pulseDuration);
            target.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    public void TogglePauseMenu()
    {
        pauseMenuPanel.SetActive(!pauseMenuPanel.activeSelf);
        Time.timeScale = pauseMenuPanel.activeSelf ? 0f : 1f;
    }

    public IEnumerator FadeOut(float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < 1)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / 1);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}