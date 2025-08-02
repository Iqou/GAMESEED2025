using UnityEngine;
using UnityEngine.AI;

public class npcBapakBapakPensiunan : MonoBehaviour, INPCDamageable
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
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] int attackDamage = 10;
    float nextAttackTime = 0f;
    bool playerInSight;

    //Atribut NPC
    [Range(70, 140)]
    public int sensitivitasDesibel = 100;

    [Range(0f, 100f)]
    public float currEmotion = 0f;

    [Range(0f, 100f)]
    public float aggroLevel = 0f;

    [Range(0f, 100f)]
    public float fearLevel = 0f;

    public int phoneCooldown = 120;

    public float nextPhoneTime = 0f;

    public int giveCoin = 5000;

    [Range(50, 500)]
    public int giveExperience = 100;

    [Header("Reward Prefabs")]
    public GameObject coinPrefab;

    bool isAggro = false;
    bool isFleeing = false;
    bool isFan = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        CheckLineOfSight();

        UpdateNPCState();

        if (isFleeing && playerInSight)
        {
            FleeFromPlayer();
        }
        else if (isAggro && playerInSight)
        {
            ChaseAndAttackPlayer();
            tryCallPolice();
        }
        else if (playerInSight)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    //emotionBar dan sesitivitasDesibel
    public void TakeDamage(float desibelDamage)
    {
        if (desibelDamage >= sensitivitasDesibel)
        {
            float reducePercent = Random.Range(2f, 10f);
            currEmotion -= reducePercent;
            currEmotion = Mathf.Clamp(currEmotion, 0f, 100f);

            Debug.Log($"{gameObject.name} TERGANGGU (besar)! Emotion turun {reducePercent:F1}% -> {currEmotion:F1}%");
        }
        else
        {
            float addPercent = Random.Range(2f, 5f);
            currEmotion += addPercent;
            currEmotion = Mathf.Clamp(currEmotion, 0f, 100f);

            Debug.Log($"{gameObject.name} TERGANGGU (kecil)! Emotion naik {addPercent:F1}% -> {currEmotion:F1}%");
        }

        UpdateNPCState();

        if (isFan)
        {
            spawnReward();
        }
    }

    void UpdateNPCState()
    {
        string oldState = isFleeing ? "Fleeing" : isAggro ? "Aggro" : "Normal";
        isFan = false;

        // Fear prioritas
        if (currEmotion < fearLevel)
        {
            if (!isFleeing) Debug.Log($"{gameObject.name} isFleeing=true");
            isFleeing = true;
            isAggro = false;
            return;
        }

        // Aggro
        if (currEmotion < aggroLevel && currEmotion > fearLevel)
        {
            if (!isAggro) Debug.Log($"{gameObject.name} isAggro=true)");
            isAggro = true;
            isFleeing = false;
            return;
        }

        // fan
        if (isAggro || isFleeing)
            Debug.Log($"{gameObject.name} bukan keduanya");
        isFan = true;
        isAggro = false;
        isFleeing = false;
    }

    void tryCallPolice()
    {
        if (nextPhoneTime <= 0)
            {
                CallPolice();
                nextPhoneTime = phoneCooldown;
            }
    }

    void CallPolice()
    {
        Debug.Log($"{gameObject.name} menelpon aparat!");
    }

    void spawnReward()
    {
        // Langsung aja ngasih player hpnya
        if (player != null)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddExperience(giveExperience);
            }
        }

        // Fungsi Spawn duit
        if (UniversalMoneySpawner.Instance != null)
        {
            UniversalMoneySpawner.Instance.SpawnMoney(transform.position, giveCoin);
        }

        Debug.Log($"{gameObject.name} gave {giveExperience} EXP and dropped {giveCoin} money!");

        isFan = false;
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
            player.GetComponent<OverworldHealth>()?.ChangeHealth(-attackDamage);
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
