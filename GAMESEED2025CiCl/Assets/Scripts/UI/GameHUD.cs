using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [Header("Player Info")]
    public TextMeshProUGUI playerInfoText; // For "Rp. xxx Lv. xxx"
    public Image healthBar;
    public Image mamadPortrait;
    public TextMeshProUGUI wantedLevelText; // Using text for angry emotes

    [Header("Weapon Slots")]
    public List<Image> speakerSlotIcons;
    public List<TextMeshProUGUI> speakerCooldownTexts;

    [Header("Player Level")]
    public RectTransform expBar; // Assign the RectTransform of the screen-width image
    public float expAnimationDuration = 0.5f;
    public float levelUpAnimationDuration = 0.4f;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;

    private PlayerStats playerStats;
    private Coroutine expAnimationCoroutine;
    private StringBuilder wantedStringBuilder = new StringBuilder();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
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
    }

    void OnDestroy()
    {
        PlayerStats.OnStatsChanged -= UpdateUI;
        PlayerStats.OnPlayerLevelUp -= TriggerLevelUpAnimation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
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

    public void UpdateWeaponSlot(int slotIndex, Sprite icon, float cooldown)
    {
        if (slotIndex < speakerSlotIcons.Count)
        {
            speakerSlotIcons[slotIndex].sprite = icon;
            speakerCooldownTexts[slotIndex].text = cooldown > 0 ? cooldown.ToString("F1") : "";
        }
    }

    public void TogglePauseMenu()
    {
        pauseMenuPanel.SetActive(!pauseMenuPanel.activeSelf);
        Time.timeScale = pauseMenuPanel.activeSelf ? 0f : 1f;
    }
}