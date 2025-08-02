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

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null) Debug.LogError("PlayerStats component not found on parent!");
        UpdateStatus();   
    }
    
    void UpdateStatus()
    {
        desibelOutput = baseDesibelOutput + (desibelLevel - 1) * 2f;
        areaJangkauan += (areaLevel - 1) * 1.5f;
        cooldownTime = Mathf.Max(1f, cooldownTime - (cooldownLevel - 1) * 0.5f);
    }

    public void Use(Transform owner)
    {
        if (lastActiveTime > Time.time)
        {
            lastActiveTime = Time.time - cooldownTime;
        }

        if (Time.time >= lastActiveTime + cooldownTime)
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
                aoeInstance.transform.localScale = new Vector3(areaJangkauan, areaJangkauan, areaJangkauan);

                StaticAoe attribute = aoeInstance.GetComponent<StaticAoe>();
                attribute.areaJangkauan = areaJangkauan;
                attribute.duration = duration;
                attribute.minDesibelOutput = minDesibelOutput;
                attribute.maxDesibelOutput = maxDesibelOutput;

                attackPos = spawnPos;
                lastActiveTime = Time.time;
                Destroy(aoeInstance, duration);
            }
        }
        else
        {
            Debug.Log($"Masih cooldown sisa {(lastActiveTime + cooldownTime) - Time.time} lagi, waktu saat ini {Time.time}");
        }
    }
}

