using UnityEngine;
using System.Collections.Generic;

public class ToaRW : MonoBehaviour, IWeaponCooldown
{
    private PlayerStats _playerStats;

    public float LastActiveTime => lastActiveTime;
    public float CurrentCooldown
    {
        get
        {
            float cooldownReduction = _playerStats != null ? _playerStats.cooldownReduction : 0f;
            return Mathf.Max(0.1f, (1f - (cooldownLevel - 1) * 0.5f) * (1 - cooldownReduction));
        }
    }
    public bool IsOnCooldown => Time.time < lastActiveTime + CurrentCooldown;

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

    private void Start()
    {

    }

    public void Use(Transform owner, PlayerStats playerStats)
    {
        _playerStats = playerStats;

        // Dynamic Stat Calculation
        float damageMultiplier = playerStats != null ? playerStats.damageMultiplier : 1f;
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

