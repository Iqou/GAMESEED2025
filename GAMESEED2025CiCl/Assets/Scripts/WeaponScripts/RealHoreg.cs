using UnityEngine;

public class RealHoreg : MonoBehaviour, IWeaponCooldown
{
    private PlayerStats _playerStats;

    public float LastActiveTime => lastActiveTime;
    public float CurrentCooldown
    {
        get
        {
            float cooldownReduction = _playerStats != null ? _playerStats.cooldownReduction : 0f;
            return Mathf.Max(0.1f, (3f - (cooldownLevel - 1) * 0.5f) * (1 - cooldownReduction));
        }
    }
    public bool IsOnCooldown => Time.time < lastActiveTime + CurrentCooldown;
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
        _playerStats = playerStats;
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

        if (Time.time >= lastActiveTime + currentCooldown)
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Camera.main.WorldToScreenPoint(owner.position).z;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            Vector3 flatMousePos = new Vector3(mouseWorldPos.x, owner.position.y, mouseWorldPos.z);
            Vector3 shootDir = (flatMousePos - owner.position).normalized;

            Vector3 spawnPos = owner.position + shootDir * 1.5f;
            Quaternion spawnRot = Quaternion.LookRotation(shootDir, Vector3.up);

            aoeInstance = Instantiate(aoePrefab, spawnPos, spawnRot);

            // Reset scale dulu biar gak dikali scale prefab
            aoeInstance.transform.localScale = Vector3.one;

            // Scale sesuai area jangkauan
            aoeInstance.transform.localScale = new Vector3(currentArea / 2f, currentArea / 2f, currentArea);

            MoveTowards mover = aoeInstance.GetComponent<MoveTowards>();
            if (mover != null)
            {
                mover.areaJangkauan = currentArea;
                mover.duration = duration;
                mover.maxDesibelOutput = currentMaxDamage;
                mover.minDesibelOutput = currentMinDamage;
                mover.SetDirection(shootDir);
            }

            attackPos = spawnPos;
            lastActiveTime = Time.time;
            Destroy(aoeInstance, duration);
        } 
        else
        {
                return;
        }
    }
}
