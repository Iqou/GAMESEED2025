using UnityEngine;
using UnityEngine.AI;

public class npcMobilPolisiElit : MonoBehaviour, INPCDamageable
{
    enum State { Patrol, Chase, MeleeAttack, RangedAttack }
    State currentState = State.Patrol;

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
    [SerializeField] float attackRange = 3f;      // Melee
    [SerializeField] float meleeStopRange = 3.5f;
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
    public GameObject sendalPrefab;
    public Transform firePoint;

    bool playerInSight;
    bool isDead = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is createdd
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
        float dist = Vector3.Distance(transform.position, player.transform.position);

       switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (playerInSight) currentState = State.Chase;
                break;

            case State.Chase:
                ChasePlayer(dist);
                break;

            case State.MeleeAttack:
                TryMeleeAttack();
                if (dist > meleeStopRange) currentState = State.Chase;
                break;
        }
    }

    public void TakeDamage(float desibelDamage)
    {
        if (isDead) return;

        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(desibelDamage));

        Tolerance -= finalDamage;
        Debug.Log($"{gameObject.name} terkena serangan {finalDamage}! Sisa Tolerance: {Tolerance}");

        if (Tolerance <= 0) Die();
    }

    //combat
    void ChasePlayer(float dist)
    {
        Agent.stoppingDistance = stopDistance;
        Agent.SetDestination(player.transform.position);

        if (dist <= attackRange)
        {
            currentState = State.MeleeAttack;
        }
        else
        {
            if (dist <= rangedRange)
            {
                TryRangedAttack();
            }
        }
        
        if (!playerInSight)
        {
            currentState = State.Patrol;
        } 
    }

    void TryMeleeAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            Debug.Log($"{gameObject.name} menyerang melee player dengan damage {attackDamage}!");
            player.GetComponent<OverworldHealth>()?.ChangeHealth(-attackDamage);
        }
    }

    void TryRangedAttack()
    {
        if (Time.time >= nextAttackTime && sendalPrefab && firePoint)
        {
            nextAttackTime = Time.time + attackCooldown;

            GameObject bullet = Instantiate(sendalPrefab, firePoint.position, firePoint.rotation);
            Vector3 dir = (player.transform.position - firePoint.position).normalized;
            bullet.GetComponent<peluruKaretProjectile>()?.SetDirection(dir);

            Debug.Log($"{gameObject.name} melempar peluru karet!");
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} mati dan memberikan {giveExperience} EXP!");
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
