using UnityEngine;
using UnityEngine.AI;

public class npcSatpamKomplek : MonoBehaviour, INPCDamageable
{
    GameObject player;
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
    [SerializeField] float attackRange = 2f;      // Melee
    [SerializeField] float rangedRange = 10f;     // Ranged mulai menyerang
    [SerializeField] float attackCooldown = 2f;
    [Range(10, 40)] public int attackDamage = 10;
    float nextAttackTime = 0f;

    //atribut npc
    [Range(500, 1000)] public int Tolerance = 500;
    [Range(50, 500)] public int giveExperience = 50;
    public int wantedLevel = 1;

    [Header("Reward Prefabs")]
    public GameObject expPrefab;

    [Header("Attack Prefabs")]
    public GameObject bulletPrefab;
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

        if (distanceToPlayer > attackRange)
        {
            Agent.SetDestination(player.transform.position);

            // 🔹 Ranged attack jika dalam jarak tertentu
            if (distanceToPlayer <= rangedRange)
            {
                TryRangedAttack();
            }
        }
        else
        {
            Agent.ResetPath();
            TryMeleeAttack();
        }
    }

    void TryMeleeAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            Debug.Log($"{gameObject.name} menyerang melee player dengan damage {attackDamage}!");
            // player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
    }

    void TryRangedAttack()
    {
        if (Time.time >= nextAttackTime && bulletPrefab != null && firePoint != null)
        {
            nextAttackTime = Time.time + attackCooldown;

            // Spawn peluru
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (player.transform.position - firePoint.position).normalized;
                rb.linearVelocity = dir * 10f; // Kecepatan peluru
            }

            Destroy(bullet, 5f);
            Debug.Log($"{gameObject.name} menembak player!");
        }
    }


    void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} mati dan drop exp {giveExperience}!");
        spawnReward();
        Destroy(gameObject, 0.5f);
    }

    void spawnReward()
    {

        if (expPrefab != null)
        {
            GameObject exp = Instantiate(expPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            Rigidbody rb = exp.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)) * 3f, ForceMode.Impulse);
            }
            // 🔹 Hapus otomatis setelah 10 detik
            Destroy(exp, 10f);
        }

        Debug.Log($"{gameObject.name} melempar koin dan exp {giveExperience}!");
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

    void ChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer > stopDistance)
        {
            Agent.SetDestination(player.transform.position);
        }
        else
        {
            Agent.ResetPath(); // berhenti bergerak jika sudah cukup dekat
        }
    }

    void ChaseAndAttackPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer > attackRange)
        {
            Agent.SetDestination(player.transform.position);
        }
        else
        {
            Agent.ResetPath();
            TryAttackPlayer();
        }
    }

    void TryAttackPlayer()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            // 🔹 Lakukan serangan melee
            Debug.Log($"{gameObject.name} menyerang player dengan damage {attackDamage}!");

            // Kalau player punya script Health:
            // player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
    }

    void CheckLineOfSight()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= sightRange)
        {
            Ray ray = new Ray(transform.position + Vector3.up, directionToPlayer.normalized); // +Vector3.up supaya ray keluar dari kepala NPC
            if (Physics.Raycast(ray, out RaycastHit hit, sightRange, ~obstacleLayer)) // obstacleLayer diatur agar mengandung layer yang memblokir pandangan
            {
                if (hit.collider.gameObject == player)
                {
                    playerInSight = true;
                    return;
                }
            }
        }

        playerInSight = false;
    }

    void FleeFromPlayer()
    {
        Vector3 direction = (transform.position - player.transform.position).normalized;
        Vector3 fleePoint = transform.position + direction * range;

        if (NavMesh.SamplePosition(fleePoint, out NavMeshHit hit, range, NavMesh.AllAreas))
        {
            Agent.SetDestination(hit.position);
        }
    }
}
