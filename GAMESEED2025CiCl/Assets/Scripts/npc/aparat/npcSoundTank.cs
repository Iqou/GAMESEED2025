using UnityEngine;
using UnityEngine.AI;

public class npcSoundTank : MonoBehaviour
{
    GameObject player;
    OverworldHealth playerHealth;
    NavMeshAgent Agent;

    [Header("Layer Settings")]
    [SerializeField] LayerMask groundLayer, playerLayer, obstacleLayer;

    //patrol
    Vector3 destPoint;
    bool walkPointSet;
    [SerializeField] float range = 10f;

    //chase
    [SerializeField] float sightRange = 20f;
    [SerializeField] float stopDistance = 3f;
    [SerializeField] float rangedRange = 10f;     // Ranged mulai menyerang
    [SerializeField] float attackCooldown = 2f;
    [Range(10, 100)] public int attackDamage = 10;
    float nextAttackTime = 0f;

    //atribut npc
    [Range(500, 1250)] public int Tolerance = 500;
    [Range(50, 500)] public int giveExperience = 50;
    public int wantedLevel = 1;

    [Header("Reward Prefabs")]
    public GameObject expPrefab;

    [Header("Attack Prefabs")]
    public GameObject sonicWavePrefab;
    public Transform firePoint;

    bool playerInSight;
    bool isDead = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        CheckLineOfSight();

        if (playerInSight)
        {
            HandleCombat();
        }
        else
        {
            Patrol();
        }
    }

    public void TakeDamage(float desibelDamage)
    {
        if (isDead) return;

        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(desibelDamage));

        Tolerance -= finalDamage;
        Debug.Log($"{gameObject.name} terkena serangan {finalDamage}! Sisa Tolerance: {Tolerance}");

        if (Tolerance <= 0)
        {
            Die();
        }
    }

    //combat
    void HandleCombat()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Selalu bergerak mendekati player sampai dalam jarak rangedRange
        if (distanceToPlayer > stopDistance)
        {
            Agent.SetDestination(player.transform.position);
        }
        else
        {
            Agent.ResetPath();
        }
        if (distanceToPlayer <= rangedRange)
        {
            TryRangedAttack();
        }
    }

    void TryRangedAttack()
    {
        if (Time.time >= nextAttackTime && sonicWavePrefab != null && firePoint != null)
        {
            nextAttackTime = Time.time + attackCooldown;

            // Spawn bullet
            GameObject bullet = Instantiate(sonicWavePrefab, firePoint.position, firePoint.rotation);
            Vector3 dir = (player.transform.position - firePoint.position).normalized;

            // Set arah ke script SendalProjectile
            //bullet.GetComponent<sendalProjectile>()?.SetDirection(dir);
            bullet.GetComponent<soundBulletforSoundTank>()?.SetDirection(dir);

            Debug.Log($"{gameObject.name} melempar soundwave ke player!");
        }
    }


    void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} mati dan drop exp {giveExperience}!");
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddExperience(giveExperience);
            }
        }
        Destroy(gameObject, 0.5f);
    }


    //behavior
    void Patrol()
    {
        if (!walkPointSet) searchForDest();
        if (walkPointSet) Agent.SetDestination(destPoint);
        if (Vector3.Distance(transform.position, destPoint) < 1f) walkPointSet = false;
    }

    void searchForDest()
    {
        float z = Random.Range(-range, range);
        float x = Random.Range(-range, range);

        destPoint = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        if (Physics.Raycast(destPoint + Vector3.up * 2, Vector3.down, out RaycastHit hit, 5f, groundLayer))
        {
            walkPointSet = true;
            destPoint = hit.point; // Agar tepat di atas tanah
        }
    }

    void CheckLineOfSight()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= sightRange)
        {
            Ray ray = new Ray(transform.position + Vector3.up*0.5f, directionToPlayer.normalized); // +Vector3.up supaya ray keluar dari kepala NPC
            if (Physics.Raycast(ray, out RaycastHit hit, sightRange, ~obstacleLayer)) // obstacleLayer diatur agar mengandung layer yang memblokir pandangan
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow);
                if (hit.collider.gameObject == player)
                {
                    playerInSight = true;
                    return;
                }
            }
        }

        playerInSight = false;
    }
}
