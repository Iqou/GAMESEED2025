using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 100;

    [Header("UI References")]
    public Image healthBarFillImage;
    public Image healthIconImage;
    public Sprite lowHealthIcon;
    public Sprite defaultHealthIcon;

    void Start()
    {
        // Set initial health to 50
        currentHealth = 50; 
        UpdateHealthUI();
    }

    void Update()
    {
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBarFillImage != null)
        {
            float healthRatio = (float)currentHealth / maxHealth;
            healthBarFillImage.fillAmount = healthRatio;
        }

        if (healthIconImage != null)
        {
            if (currentHealth <= (maxHealth * 0.2f) && lowHealthIcon != null)
            {
                healthIconImage.sprite = lowHealthIcon;
            }
            else if (defaultHealthIcon != null)
            {
                healthIconImage.sprite = defaultHealthIcon;
            }
        }
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        // Clamp the current health to be within the 0 to maxHealth range
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 
    }
}