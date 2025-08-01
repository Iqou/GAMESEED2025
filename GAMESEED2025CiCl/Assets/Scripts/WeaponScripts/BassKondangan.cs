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
    private float duration = 0.5f;
    private float cooldownTime = 10f;
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

    private PlayerStats playerStats; 

    void Start()
    {
        playerStats = GetComponentInParent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found");
        }
        UpdateStats();
    }

    void Update()
    {
        // Untuk memudahkan implementasi upgrade, semua logika attack ada di PlayerAttack.cs
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


    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.pink;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.5f, areaJangkauan);
    }
}
