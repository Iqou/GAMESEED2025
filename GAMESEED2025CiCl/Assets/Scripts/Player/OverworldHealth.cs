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
        if (playerHealthBar != null)
        {
            playerHealthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Player has died! Reloading scene.");
        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
