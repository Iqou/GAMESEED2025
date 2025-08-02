using UnityEngine;
using UnityEngine.AI;

public class npcRandomWalk : MonoBehaviour
{
    GameObject player;
    NavMeshAgent Agent;

    [SerializeField] LayerMask groundLayer, playerLayer;

    Vector3 destPoint;
    bool walkPointSet;
    [SerializeField] float range;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        if (!walkPointSet) searchForDest();
        if (walkPointSet) Agent.SetDestination(destPoint);
        if (Vector3.Distance(transform.position, destPoint) < 10) walkPointSet = false;
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
}
