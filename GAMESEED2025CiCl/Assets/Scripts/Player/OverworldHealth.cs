using UnityEngine;
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
    public Image playerHealthBar; // Buat UI

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
        Debug.Log("Player has died! Calculating SoundChips and returning to menu.");

        // --- End of Run Calculation ---
        if (GameManager.Instance != null && playerStats != null)
        {
            // Soundchip = (Total Rupiah Terkumpul / 5000) + (Jumlah Boss Dikalahkan * 50)
            int soundChipsEarned = (playerStats.totalRupiahCollectedThisRun / 5000) + (playerStats.bossesKilledThisRun * 50);
            
            Debug.Log($"Rupiah Collected: {playerStats.totalRupiahCollectedThisRun}, Bosses Killed: {playerStats.bossesKilledThisRun}");
            Debug.Log($"SoundChips Earned: {soundChipsEarned}");

            GameManager.Instance.soundChips += soundChipsEarned;
            GameManager.Instance.SaveProgressToSlot(GameManager.Instance.currentSlot);
        }

        // Load the Main Menu scene (assuming it's at build index 0)
        SceneManager.LoadScene(0);
    }
}
