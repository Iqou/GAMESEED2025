using UnityEngine;

public class BassKondangan : MonoBehaviour
{
    private string namaSpeaker = "Bass Kondangan";
    private string tier = "Epic";

    private float maxDesibelOutput = 100f;
    private float minDesibelOutput = 90f;
    private float baseDesibelOutput = 90f;
    private float desibelOutput;
    private float areaJangkauan = 9f;
    private float duration = 1f;
    private float cooldownTime = 1.5f;
    private float weight = 6f;
    private float saweranMultiplier = 2.2f;
    private float knockbackForce = 5f;

    private float boomDelay = 0.25f;
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

        float currentCooldown = Mathf.Max(0.1f, (1.5f - (cooldownLevel - 1) * 0.5f) * (1 - cooldownReduction));
        float currentArea = (9f + (areaLevel - 1) * 1.5f) * areaMultiplier;
        float currentMaxDamage = (100f + (desibelLevel - 1) * 2f) * damageMultiplier;
        float currentMinDamage = (90f + (desibelLevel - 1) * 2f) * damageMultiplier;

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

                Vector3 spawnPos = owner.position + shootDir;
                Quaternion spawnRot = Quaternion.LookRotation(shootDir, Vector3.up);

                aoeInstance = Instantiate(aoePrefab, spawnPos, spawnRot);

                aoeInstance.transform.localScale = Vector3.one;
                aoeInstance.transform.localScale = new Vector3(currentArea/2, 0.1f, currentArea);

                StaticAoe attribute = aoeInstance.GetComponent<StaticAoe>();
                attribute.areaJangkauan = currentArea;
                attribute.duration = duration;
                attribute.minDesibelOutput = currentMinDamage;
                attribute.maxDesibelOutput = currentMaxDamage;

                attackPos = spawnPos;
                lastActiveTime = Time.time;
                Destroy(aoeInstance, duration);
            }
        }
        else
        {
            Debug.Log($"Masih cooldown sisa {(lastActiveTime + currentCooldown) - Time.time} lagi, waktu saat ini {Time.time}");
        }
    }
}
