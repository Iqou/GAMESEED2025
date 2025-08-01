using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    [Header("UI References")]
    public Image playerHealthBar;
    public Image enemyHealthBar;

    void Start()
    {
        currentHealth = maxHealth / 2;
        UpdateHealthUI();
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
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