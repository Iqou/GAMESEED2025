using UnityEngine;
using System.Collections.Generic;

public class ToaRW : MonoBehaviour
{
    private string namaSpeaker = "Toa RW";
    private string tier = "Common";

    private float maxDesibelOutput = 80f;
    private float minDesibelOutput = 70f;
    private float baseDesibelOutput = 70f;
    private float desibelOutput;
    private float areaJangkauan = 5f;
    private float duration = 0.5f;
    private float cooldownTime = 1f;
    private float weight = 2f;
    private float saweranMultiplier = 1.2f;

    private float lastActiveTime = -999f;

    private int desibelLevel = 1;
    private int areaLevel = 1;
    private int cooldownLevel = 1;

    private bool isAttacking = false;

    private Vector3 attackPos;

    private GameObject aoeInstance;
    public GameObject aoePrefab;
    public Texture2D iconTexture; // Assign this in the Inspector

    private void Start()
    {

    }

    public Texture2D GetIconTexture()
    {
        return iconTexture;
    }

    public float GetCurrentCooldown(PlayerStats playerStats)
    {
        float cooldownReduction = playerStats != null ? playerStats.cooldownReduction : 0f;
        return Mathf.Max(0.1f, (1f - (cooldownLevel - 1) * 0.5f) * (1 - cooldownReduction));
    }

    public float GetMaxCooldown(PlayerStats playerStats)
    {
        float cooldownReduction = playerStats != null ? playerStats.cooldownReduction : 0f;
        return Mathf.Max(0.1f, (1f - (cooldownLevel - 1) * 0.5f) * (1 - cooldownReduction));
    }

    public float GetRemainingCooldown()
    {
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats not found when trying to get remaining cooldown for ToaRW.");
            return 0f; // Or handle appropriately
        }
        float currentCooldown = GetCurrentCooldown(playerStats);
        return Mathf.Max(0f, (lastActiveTime + currentCooldown) - Time.time);
    }

    public void Use(Transform owner, PlayerStats playerStats, HitQuality quality)
    {
        // --- RHYTHM-BASED MODIFIERS ---
        float hitMultiplier = 1.0f;
        bool applyKnockback = false;

        switch (quality)
        {
            case HitQuality.Perfect:
                hitMultiplier = 2.0f;
                applyKnockback = true;
                break;
            case HitQuality.OffBeat:
                hitMultiplier = 0.25f;
                break;
        }

        // Dynamic Stat Calculation
        float damageMultiplier = (playerStats != null ? playerStats.damageMultiplier : 1f) * hitMultiplier;
        float areaMultiplier = playerStats != null ? playerStats.areaOfEffectBonus : 1f;
        float cooldownReduction = playerStats != null ? playerStats.cooldownReduction : 0f;

        float currentCooldown = Mathf.Max(0.1f, (1f - (cooldownLevel - 1) * 0.5f) * (1 - cooldownReduction));
        float currentArea = (5f + (areaLevel - 1) * 1.5f) * areaMultiplier;
        float currentMaxDamage = (80f + (desibelLevel - 1) * 2f) * damageMultiplier;
        float currentMinDamage = (70f + (desibelLevel - 1) * 2f) * damageMultiplier;

        if (lastActiveTime > Time.time)
        {
            lastActiveTime = Time.time;
        }


        if (Time.time >= lastActiveTime + currentCooldown)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 flatMousePos = new Vector3(hit.point.x, owner.position.y, hit.point.z);
                Vector3 shootDir = (flatMousePos - owner.position).normalized;

                Vector3 spawnPos = owner.position + shootDir * 1.5f;
                Quaternion spawnRot = Quaternion.LookRotation(shootDir, Vector3.up);

                aoeInstance = Instantiate(aoePrefab, spawnPos, spawnRot);

                aoeInstance.transform.localScale = Vector3.one;
                aoeInstance.transform.localScale = new Vector3(currentArea, currentArea, currentArea);

                StaticAoe attribute = aoeInstance.GetComponent<StaticAoe>();
                attribute.areaJangkauan = currentArea;
                attribute.duration = duration;
                attribute.minDesibelOutput = currentMinDamage;
                attribute.maxDesibelOutput = currentMaxDamage;
                // Pass knockback info if the AOE script supports it
                // if (applyKnockback) { attribute.EnableKnockback(); }

                attackPos = spawnPos;
                lastActiveTime = Time.time;
                Destroy(aoeInstance, duration);
                GameHUD.Instance.UpdateWeaponSlot(0, GetIconTexture(), GetRemainingCooldown(), GetMaxCooldown(playerStats)); // Update UI for slot 0
            }
        }
        else
        {
            Debug.Log($"Masih cooldown sisa {(lastActiveTime + currentCooldown) - Time.time} lagi, waktu saat ini {Time.time}");
        }
    }
}

