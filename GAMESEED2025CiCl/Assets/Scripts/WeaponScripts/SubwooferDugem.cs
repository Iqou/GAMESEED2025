using UnityEngine;

public class SubwooferDugem : MonoBehaviour
{

    private string namaSpeaker = "Subwoofer Dugem";
    private string tier = "Mythic";

    private float maxDesibelOutput = 100f;
    private float minDesibelOutput = 90f;
    private float baseDesibelOutput = 100f;
    private float desibelOutput;
    private float areaJangkauan = 15f;
    private float duration = 2f;
    private float cooldownTime = 2f;
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
            aoeInstance.transform.localScale = new Vector3(areaJangkauan, 0.1f, areaJangkauan);

            attackPos = spawnPos;
            lastActiveTime = Time.time;
            ActivateDugem();
        }
        else
        {
            Debug.Log($"Masih cooldown sisa {(lastActiveTime + cooldownTime) - Time.time} lagi, waktu saat ini {Time.time}");
        }

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
        
        isAttacking = false;
        Destroy(aoeInstance, duration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.pink;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.5f, areaJangkauan);
    }

}
