using UnityEngine;

public class ToaRW : MonoBehaviour
{
    private string namaSpeaker = "Toa RW";
    private string tier = "Common";

    private float maxDesibelOutput = 70f;
    private float minDesibelOutput = 80f;
    private float baseDesibelOutput = 70f;
    private float desibelOutput;
    private float areaJangkauan = 5f;
    private float duration = 0.5f;
    private float cooldownTime = 4f;
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

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null) Debug.LogError("PlayerStats component not found on parent!");
        UpdateStatus();   
    }

    void Update()
    {
        float finalCooldown = cooldownTime * (1 - playerStats.cooldownReduction);
        if (Input.GetKeyDown(KeyCode.W) && Time.time >= lastActiveTime + finalCooldown && !isAttacking)
        {
            TriggerAOE();
        } 
        else if(Input.GetKeyDown(KeyCode.W) && Time.time >= lastActiveTime + finalCooldown && isAttacking)
        {
            StopAOE();
        }

        if (isAttacking)
        {
            MouseRotator();
        }
    }

    void UpdateStatus()
    {
        desibelOutput = baseDesibelOutput + (desibelLevel - 1) * 2f;
        areaJangkauan += (areaLevel - 1) * 1.5f;
        cooldownTime = Mathf.Max(1f, cooldownTime - (cooldownLevel - 1) * 0.5f);
    }

    public void Use(Transform owner)
    {
        float finalCooldown = cooldownTime * (1 - playerStats.cooldownReduction);
        if (Time.time < lastActiveTime + finalCooldown) return;

        Vector3 spawnPos = owner.position + owner.forward * 1.5f;
        Quaternion spawnRot = Quaternion.LookRotation(owner.forward);
        aoeInstance = GameObject.Instantiate(aoePrefab, spawnPos, spawnRot);

        attackPos = spawnPos;
    }


    public void TriggerAOE()
    {
        aoeInstance.SetActive(true);

        float finalArea = areaJangkauan * playerStats.areaOfEffectBonus;
        Collider[] hit = Physics.OverlapSphere(attackPos, finalArea);

        foreach (Collider hits in hit)
        {
            if (hits.CompareTag("NPC"))
            {
                float randomBase = Random.Range(minDesibelOutput, maxDesibelOutput);
                desibelOutput = randomBase * playerStats.damageMultiplier;
                Debug.Log($"{hits.name} Duarr kena damage dari TOA kena damage {desibelOutput} db");
            }
        }

        isAttacking = true;
        lastActiveTime = Time.time;
        Destroy(aoeInstance, duration);
    }

    void StopAOE()
    {
        if (aoeInstance != null)
        {
            aoeInstance.SetActive(false);
        }
        isAttacking = false;
    }

    void MouseRotator()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }


}   
