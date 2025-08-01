using UnityEngine;

public class SubwooferDugem : MonoBehaviour
{

    private string namaSpeaker = "Subwoofer Dugem";
    private string tier = "Mythic";

    private float maxDesibelOutput = 90f;
    private float minDesibelOutput = 100f;
    private float baseDesibelOutput = 100f;
    private float desibelOutput;
    private float areaJangkauan = 15f;
    private float duration = 1f;
    private float cooldownTime = 12f;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateStats();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && Time.time >= lastActiveTime + cooldownTime && !isAttacking)
        {
            isAttacking = true;
            ActivateDugem();
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
        aoeInstance.transform.localScale = new Vector3(areaJangkauan, 0.1f, areaJangkauan);

        attackPos = spawnPos;
    }


    public void ActivateDugem()
    {
        Collider[] hit = Physics.OverlapSphere(attackPos, areaJangkauan);

        foreach (Collider hits in hit)
        {
            if (hits.CompareTag("NPC"))
            {
                desibelOutput = Random.Range(minDesibelOutput, maxDesibelOutput);
                Debug.Log($"{hits.name} Duarr kena damage dari subwoofer dugem damage {desibelOutput} dB");

                INPCDamageable npc = hits.GetComponent<INPCDamageable>();
                if (npc != null)
                {
                    npc.TakeDamage(desibelOutput);
                }
            }
        }

        lastActiveTime = Time.time;
        isAttacking = false;
        Destroy(aoeInstance, duration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.pink;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.5f, areaJangkauan);
    }

}
