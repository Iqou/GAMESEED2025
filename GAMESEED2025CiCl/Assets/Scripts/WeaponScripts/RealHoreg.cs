using UnityEngine;

public class RealHoreg : MonoBehaviour
{
    private string namaSpeaker = "Real Horeg";
    private string tier = "Mythic";

    private float maxDesibelOutput = 100f;
    private float minDesibelOutput = 110f;
    private float baseDesibelOutput = 100f;
    private float desibelOutput;
    private float areaJangkauan = 20f;
    private float duration = 2f;
    private float cooldownTime = 6f;
    private float weight = 6f;
    private float saweranMultiplier = 1.5f;
    private float projectileSpeed = 20f;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) && Time.time >= lastActiveTime + cooldownTime)
        {
            ShootDaHoreg();
        }
    }

    void UpdateStats()
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

        attackPos = spawnPos;
    }


    public void ShootDaHoreg()
    {

        Collider[] hit = Physics.OverlapSphere(attackPos, areaJangkauan);

        foreach (Collider hits in hit)
        {
            if (hits.CompareTag("NPC"))
            {
                Debug.Log($"{hits.name} Duarr kena damage dari Real Horeg");
            }
        }

        Rigidbody rigidbody = aoeInstance.GetComponent<Rigidbody>();

        if (rigidbody != null)
        {
            rigidbody.angularVelocity = transform.forward * projectileSpeed;
        }

        lastActiveTime = Time.time;
        Destroy(aoeInstance, duration);
    }


}
