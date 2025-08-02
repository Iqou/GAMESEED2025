using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    [Header("UI References")]
    public Image playerHealthBar;
    public Image enemyHealthBar;

    [Header("Smoothing")]
    public float drainRate = 0f;
    private float smoothHealth;

    void Start()
    {
        currentHealth = maxHealth / 2;
        smoothHealth = currentHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        // Logika pengurangan health secara konstan (drain)
        if (drainRate > 0)
        {
            smoothHealth -= drainRate * Time.deltaTime;
            smoothHealth = Mathf.Clamp(smoothHealth, 0, maxHealth);
            UpdateHealthUI();
        }
        currentHealth = (int)smoothHealth;
    }

    public void ChangeHealth(int amount)
    {
        // Perubahan health akibat klik akan instan
        smoothHealth += amount;
        smoothHealth = Mathf.Clamp(smoothHealth, 0, maxHealth);
        currentHealth = (int)smoothHealth;

        UpdateHealthUI();
    }
    
    // Metode untuk mengatur kecepatan drain
    public void SetDrainRate(float rate)
    {
        drainRate = rate;
    }

    private void UpdateHealthUI()
    {
        float healthRatio = (float)currentHealth / maxHealth;

        if (playerHealthBar != null)
        {
            playerHealthBar.fillAmount = healthRatio;
        }

        if (enemyHealthBar != null)
        {
            enemyHealthBar.fillAmount = 1 - healthRatio;
        }
    }
}
