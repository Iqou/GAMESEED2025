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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateStats();
    }

    void UpdateStats()
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
            Vector3 spawnPos = owner.position + owner.forward * 1.5f;
            Quaternion spawnRot = Quaternion.LookRotation(owner.forward);
            aoeInstance = GameObject.Instantiate(aoePrefab, spawnPos, spawnRot);
            aoeInstance.transform.localScale = new Vector3(areaJangkauan, 1f, areaJangkauan);

            StaticAoe attribute = aoeInstance.GetComponent<StaticAoe>();

            attribute.areaJangkauan = areaJangkauan;
            attribute.duration = duration;
            attribute.minDesibelOutput = minDesibelOutput;
            attribute.maxDesibelOutput = maxDesibelOutput;

            attackPos = spawnPos;
            lastActiveTime = Time.time;
            Destroy(aoeInstance, duration);
        }
        else
        {
            Debug.Log($"Masih cooldown sisa {(lastActiveTime + cooldownTime) - Time.time} lagi, waktu saat ini {Time.time}");
        }
    }
}
