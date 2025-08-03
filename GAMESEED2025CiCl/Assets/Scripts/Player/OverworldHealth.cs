using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerStats))]
public class OverworldHealth : MonoBehaviour
{
    [Header("Base Stats")]
    public int baseMaxHealth = 100;
    public int currentHealth;
    public int maxHealth;

    [Header("UI References")]
    // The health bar is now managed by GameHUD using UI Toolkit.
    // Ensure your UIDocument has a VisualElement named "HealthBar_Fill".

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        
        PlayerStats.OnStatsChanged += UpdateMaxHealth;
        UpdateMaxHealth();
        
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            PlayerStats.OnStatsChanged -= UpdateMaxHealth;
        }
    }

    void Update()
    {
        // For debug purposes: Press F3 to decrease health by 5%.
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("F3 pressed. Decreasing health by 5%.");
            ChangeHealth(-maxHealth / 20);
        }

        // For debug purposes: Press F4 to trigger the game over sequence.
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("F4 pressed. Forcing game over.");
            // Set health to 0 to trigger death sequence. 
            // We pass a large negative number to ensure health becomes 0.
            ChangeHealth(-maxHealth);
        }
    }

    void UpdateMaxHealth()
    {
        int oldMaxHealth = maxHealth;
        maxHealth = baseMaxHealth + (int)playerStats.maxHealthBonus;
        
        if (maxHealth > oldMaxHealth)
        {
            currentHealth += maxHealth - oldMaxHealth;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.SetHealth(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        // Prevent the Die method from being called multiple times
        if (currentHealth > 0) return;

        Debug.Log("Player has died! Starting game over sequence.");

        // --- Disable Player Controls ---
        // Find and disable movement and attack scripts.
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null) attack.enabled = false;

        // Start the fade-out/fade-in sequence
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // --- Start Fades ---
        if (GameHUD.Instance != null) StartCoroutine(GameHUD.Instance.FadeOut(3.0f));
        if (GameOverUI.Instance != null)
        {
            // Calculate stats before showing the screen
            int soundChipsEarned = 0;
            if (GameManager.Instance != null && playerStats != null)
            {
                soundChipsEarned = (playerStats.money / 5000) + (playerStats.bossesKilledThisRun * 50);
                GameManager.Instance.soundChips += soundChipsEarned;
                GameManager.Instance.SaveProgressToSlot(GameManager.Instance.currentSlot);
            }

            GameOverUI.Instance.ShowGameOverScreen(
                playerStats.money,
                playerStats.bossesKilledThisRun,
                soundChipsEarned,
                playerStats.timePlayedThisRun,
                playerStats.level
            );
        }

        // Wait for the fade to complete
        yield return new WaitForSeconds(3.0f);

        // --- Pause Game ---
        Debug.Log("Fades complete. Pausing game.");
        Time.timeScale = 0f;
    }
}
