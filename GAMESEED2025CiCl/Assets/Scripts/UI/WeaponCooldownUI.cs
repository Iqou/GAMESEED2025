using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponCooldownUI : MonoBehaviour
{
    public Image weaponIcon; // Assign the UI Image for the weapon icon
    public IWeaponCooldown weapon; // The weapon script implementing IWeaponCooldown

    [Header("Alpha Fade Settings")]
    public float minAlpha = 0.3f; // Minimum alpha when on cooldown
    public float maxAlpha = 1.0f; // Maximum alpha when ready

    [Header("Pop Animation Settings")]
    public float popScale = 1.2f; // How much the icon scales up
    public float popDuration = 0.1f; // Duration of the pop animation

    private Vector3 originalScale;
    private bool wasOnCooldown = true; // To track state change for pop animation

    void Awake()
    {
        originalScale = weaponIcon.transform.localScale;
    }

    void Update()
    {
        if (weapon == null || weaponIcon == null)
        {
            return;
        }

        UpdateCooldownUI();
    }

    void UpdateCooldownUI()
    {
        float cooldownProgress = 1f; // 1 means ready, 0 means just used

        if (weapon.IsOnCooldown)
        {
            float timeSinceLastUse = Time.time - weapon.LastActiveTime;
            cooldownProgress = Mathf.Clamp01(timeSinceLastUse / weapon.CurrentCooldown);
            wasOnCooldown = true;
        }
        else
        {
            // Weapon is ready
            cooldownProgress = 1f;
            if (wasOnCooldown)
            {
                // Only pop if it just became ready
                StartCoroutine(PopAnimation());
                wasOnCooldown = false;
            }
        }

        // Update alpha based on cooldown progress
        Color currentColor = weaponIcon.color;
        currentColor.a = Mathf.Lerp(minAlpha, maxAlpha, cooldownProgress);
        weaponIcon.color = currentColor;
    }

    IEnumerator PopAnimation()
    {
        Vector3 targetScale = originalScale * popScale;
        float timer = 0f;

        // Scale up
        while (timer < popDuration)
        {
            weaponIcon.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / popDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        weaponIcon.transform.localScale = targetScale; // Ensure it reaches target

        timer = 0f;
        // Scale down
        while (timer < popDuration)
        {
            weaponIcon.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / popDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        weaponIcon.transform.localScale = originalScale; // Ensure it returns to original
    }
}
