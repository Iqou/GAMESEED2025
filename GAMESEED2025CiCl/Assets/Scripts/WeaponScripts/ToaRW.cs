using UnityEngine;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateStatus();   
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && Time.time >= lastActiveTime + cooldownTime && !isAttacking)
        {
            TriggerAOE();
        } 
        else if(Input.GetKeyDown(KeyCode.W) && Time.time >= lastActiveTime + cooldownTime && isAttacking)
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
        Vector3 spawnPos = owner.position + owner.forward * 1.5f;
        Quaternion spawnRot = Quaternion.LookRotation(owner.forward);
        aoeInstance = GameObject.Instantiate(aoePrefab, spawnPos, spawnRot);
        aoeInstance.transform.localScale = new Vector3(areaJangkauan, 0.1f, areaJangkauan);

        attackPos = spawnPos;
    }


    public void TriggerAOE()
    {
        aoeInstance.SetActive(true);

        Collider[] hit = Physics.OverlapSphere(attackPos, areaJangkauan);

        foreach (Collider hits in hit)
        {
            if (hits.CompareTag("NPC"))
            {
                desibelOutput = Random.Range(minDesibelOutput, maxDesibelOutput);
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
            Destroy(aoeInstance);
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
