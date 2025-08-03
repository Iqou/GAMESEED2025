using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;



public class npcWIlliamShakeSpeaker : MonoBehaviour
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
    bool playerInSight;

    //Atribut boss
    [Range(500, 10000)] public int Tolerance = 500;

    public int giveCoin = 5000;

    [Range(50, 50000)] public int giveExperience = 100;

    [Header("Reward Prefabs")]
    public GameObject coinPrefab;

    [Header("Battle Settings")]
    public string battleSceneName = "BeatScene"; // Nama scene pertarungan 2D
    public float triggerBattleDistance = 2f;        // Jarak minimal untuk transisi

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
            ChasePlayer();
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= triggerBattleDistance)
            {
                TransitionToBattle();
            }
        }
        else
        {
            Patrol();
        }
    }

    //emotionBar dan sesitivitasDesibel
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

    void spawnReward()
    {

        if (UniversalMoneySpawner.Instance != null)
        {
            UniversalMoneySpawner.Instance.SpawnMoney(transform.position, giveCoin);
        }

        Debug.Log($"{gameObject.name} gave {giveExperience} EXP and dropped {giveCoin} money!");
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
                spawnReward();
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
    
    void TransitionToBattle()
    {
        Debug.Log($"{gameObject.name} memulai pertarungan boss!");
        SceneManager.LoadScene(battleSceneName);
    }
}
