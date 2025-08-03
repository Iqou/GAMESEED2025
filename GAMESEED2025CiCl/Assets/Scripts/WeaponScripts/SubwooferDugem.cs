using UnityEngine;

public class SubwooferDugem : MonoBehaviour, IHoregWeapon
{

    private string namaSpeaker = "Subwoofer Dugem";
    private string tier = "Mythic";

    private float maxDesibelOutput = 100f;
    private float minDesibelOutput = 90f;
    private float baseDesibelOutput = 100f;
    private float desibelOutput;
    private float areaJangkauan = 15f;
    private float duration = 2f;
    private float cooldownTime = 2f;
    private float weight = 4f;
    private float saweranMultiplier = 2.2f;

    private float lastActiveTime = -999f;

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

        float currentCooldown = Mathf.Max(0.1f, (2f - (cooldownLevel - 1) * 0.5f) * (1 - cooldownReduction));
        float currentArea = (15f + (areaLevel - 1) * 1.5f) * areaMultiplier;
        float currentMaxDamage = (100f + (desibelLevel - 1) * 2f) * damageMultiplier;
        float currentMinDamage = (90f + (desibelLevel - 1) * 2f) * damageMultiplier;

        if (lastActiveTime > Time.time)
        {
            lastActiveTime = Time.time;
        }


        if (Time.time >= lastActiveTime + currentCooldown)
        {
            Vector3 spawnPos = owner.position;
            Quaternion spawnRot = Quaternion.LookRotation(owner.forward);
            aoeInstance = GameObject.Instantiate(aoePrefab, spawnPos, spawnRot);
            aoeInstance.transform.localScale = new Vector3(currentArea, 1f, currentArea);

            StaticAoe attribute = aoeInstance.GetComponent<StaticAoe>();

            attribute.areaJangkauan = currentArea;
            attribute.duration = duration;
            attribute.minDesibelOutput = currentMinDamage;
            attribute.maxDesibelOutput = currentMaxDamage;

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
