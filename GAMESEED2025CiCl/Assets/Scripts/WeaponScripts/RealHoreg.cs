using UnityEngine;

public class RealHoreg : MonoBehaviour
{
    private string namaSpeaker = "Real Horeg";
    private string tier = "Mythic";

    private float maxDesibelOutput = 110f;
    private float minDesibelOutput = 100f;
    private float baseDesibelOutput = 100f;
    private float desibelOutput;
    private float areaJangkauan = 5f;
    private float duration = 2f;
    private float cooldownTime = 3f;
    private float weight = 6f;
    private float saweranMultiplier = 1.5f;
    private float baseCooldownTime = 1;

    private float lastActiveTime = 0f;

    private int desibelLevel = 1;
    private int areaLevel = 1;
    private int cooldownLevel = 1;

    private bool isAttacking = false;

    private Vector3 attackPos;

    private GameObject aoeInstance;
    
    public GameObject aoePrefab;

    public void Use(Transform owner, PlayerStats playerStats)
    {
        // Dynamic Stat Calculation
        float damageMultiplier = playerStats != null ? playerStats.damageMultiplier : 1f;
        float areaMultiplier = playerStats != null ? playerStats.areaOfEffectBonus : 1f;
        float cooldownReduction = playerStats != null ? playerStats.cooldownReduction : 0f;

        float currentCooldown = Mathf.Max(0.1f, (3f - (cooldownLevel - 1) * 0.5f) * (1 - cooldownReduction));
        float currentArea = (5f + (areaLevel - 1) * 1.5f) * areaMultiplier;
        float currentMaxDamage = (110f + (desibelLevel - 1) * 2f) * damageMultiplier;
        float currentMinDamage = (100f + (desibelLevel - 1) * 2f) * damageMultiplier;

        if (lastActiveTime > Time.time)
        {
            // Dynamic Stat Calculation
            float damageMultiplier = playerStats != null ? playerStats.damageMultiplier : 1f;
            float areaMultiplier = playerStats != null ? playerStats.areaOfEffectBonus : 1f;
            float cooldownReduction = playerStats != null ? playerStats.cooldownReduction : 0f;

            float currentCooldown = Mathf.Max(0.1f, (3f - (cooldownLevel - 1) * 0.5f) * (1 - cooldownReduction));
            float currentArea = (5f + (areaLevel - 1) * 1.5f) * areaMultiplier;
            float currentMaxDamage = (110f + (desibelLevel - 1) * 2f) * damageMultiplier;
            float currentMinDamage = (100f + (desibelLevel - 1) * 2f) * damageMultiplier;

            if (lastActiveTime > Time.time)
            {
                lastActiveTime = Time.time;
            }

            attackPos = spawnPos;
            lastActiveTime = Time.time;
            Destroy(aoeInstance, duration);
        } 
        else
        {
            Debug.Log($"Masih cooldown sisa {(lastActiveTime + currentCooldown) - Time.time} lagi, waktu saat ini {Time.time}");
        }
    }
}
