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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateStats();
    }

    void Update()
    {
       
    }

    void UpdateStats()
    {
        desibelLevel = Mathf.Max(1, desibelLevel);
        areaLevel = Mathf.Max(1, areaLevel);
        cooldownLevel = Mathf.Max(1, cooldownLevel);

        desibelOutput = baseDesibelOutput + (desibelLevel - 1) * 2f;
        areaJangkauan = 5f + (areaLevel - 1) * 1.5f;
        cooldownTime = Mathf.Max(1f, baseCooldownTime - (cooldownLevel - 1) * 0.5f);
    }

    public void Use(Transform owner)
    {
        if (lastActiveTime > Time.time)
        {
            lastActiveTime = Time.time - cooldownTime;
        }

        if (Time.time >= lastActiveTime + cooldownTime)
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
            aoeInstance.transform.localScale = new Vector3(areaJangkauan / 2f, areaJangkauan / 2f, areaJangkauan);

            MoveTowards mover = aoeInstance.GetComponent<MoveTowards>();
            if (mover != null)
            {
                mover.SetDirection(shootDir);
            }

            attackPos = spawnPos;
            lastActiveTime = Time.time;
            ShootDaHoreg();
        } 
        else
        {
            Debug.Log($"Masih cooldown sisa {(lastActiveTime + cooldownTime) - Time.time} lagi, waktu saat ini {Time.time}");
        }
    }


    public void ShootDaHoreg()
    {

        Collider[] hit = Physics.OverlapSphere(attackPos, areaJangkauan);

        foreach (Collider hits in hit)
        {
            if (hits.CompareTag("NPC"))
            {
                desibelOutput = Random.Range(minDesibelOutput, maxDesibelOutput);
                Debug.Log($"{hits.name} Duarr kena damage dari Real Horeg, damage {desibelOutput} dB");
            }
        }
        Destroy(aoeInstance, duration);
    }


}
