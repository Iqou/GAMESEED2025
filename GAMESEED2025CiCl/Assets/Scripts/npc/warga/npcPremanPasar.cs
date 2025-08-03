using UnityEngine;
using UnityEngine.AI;

public class npcPremanPasar : MonoBehaviour, INPCDamageable
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

    [Header("Copet Settings")]
    public float copetRadius = 5f;
    public float copetDelay = 10f;
    public int copetAmountPerSecond = 100;
    private float playerStayTime = 0f;
    private float nextCopetTime = 0f;

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

        if (isFan)
        {
            HandleCopet();
        }
        else
        {
            // reset jika bukan fan
            playerStayTime = 0f;
        }

        if (isFleeing && playerInSight)
        {
            FleeFromPlayer();
        }
        else if (isAggro && playerInSight)
        {
            ChasePlayer();
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

    void HandleCopet()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= copetRadius)
        {
            // Hitung berapa lama pemain berada di area
            playerStayTime += Time.deltaTime;

            // Jika sudah 10 detik berada di area, mulai mencopet
            if (playerStayTime >= copetDelay)
            {
                if (Time.time >= nextCopetTime)
                {
                    // Ambil uang pemain
                    //PlayerWallet playerWallet = player.GetComponent<PlayerWallet>();
                    //if (playerWallet != null)
                    {
                       // playerWallet.TakeMoney(copetAmountPerSecond);
                        Debug.Log($"{gameObject.name} mencopet Rp{copetAmountPerSecond} dari Player!");
                    }

                    // delay 1 detik per copet
                    nextCopetTime = Time.time + 1f;
                }
            }
        }
        else
        {
            // Keluar area => reset timer
            playerStayTime = 0f;
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
            return; // ⬅ Hentikan di sini
        }

        // Aggro
        if (currEmotion < aggroLevel && currEmotion > fearLevel)
        {
            if (!isAggro) Debug.Log($"{gameObject.name} isAggro=true)");
            isAggro = true;
            isFleeing = false;
            return; // ⬅ Hentikan di sini
        }

        // fan
        if (isAggro || isFleeing)
            Debug.Log($"{gameObject.name} bukan keduanya");
        isFan = true;
        isAggro = false;
        isFleeing = false;
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
