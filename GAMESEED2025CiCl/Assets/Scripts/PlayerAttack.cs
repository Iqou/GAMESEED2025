using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject aoePrefab;
    public float aoeRadius = 7.0f;
    public float beatInterval = 0.5f;
    public float beatWindow = 0.15f;

    private float attackCooldownHoreg1 = 1.0f;
    private float attackCooldownHoreg2 = 3.0f;
    private float nextBeatTime = 0.0f;
    private float nextAttackHoreg1 = 0.0f;
    private float nextAttackHoreg2 = 0.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextBeatTime = Time.time * beatInterval;
    }

    // Update is called once per frame
    void Update()
    {
        Metronome();

        // Horeg 1
        if ((Input.GetKeyDown(KeyCode.A)|| Input.GetKeyDown(KeyCode.D)) && Time.time >= nextAttackHoreg1)
        {
            TriggerAOE("Horeg1", attackCooldownHoreg1);
            nextAttackHoreg1 = Time.time + attackCooldownHoreg1;
        }

        // Horeg 2
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)) && Time.time >= nextAttackHoreg2)
        {
            TriggerAOE("Horeg2", attackCooldownHoreg2);
            nextAttackHoreg2 = Time.time + attackCooldownHoreg2;
        }
    }

    // Untuk Trigger AOE
    void TriggerAOE(string horeg, float duration)
    {
        bool isOnBeat = Mathf.Abs(Time.time - nextBeatTime + beatInterval) <= beatWindow;

        // Instanstiate AOE
        GameObject aoeInstance = Instantiate(aoePrefab, transform.position, Quaternion.identity);

        // AOE Radius
        aoeInstance.transform.localScale = new Vector3(aoeRadius, 0.1f, aoeRadius);

        // Kalau On Beat (Dugem)
        Renderer aoeRenderer = aoeInstance.GetComponent<Renderer>();

        if (isOnBeat)
        {
            aoeRenderer.material.color = Color.gold;
            Debug.Log($"{horeg} On Beat Dapat Bonus");
        }
        else
        {
            if (horeg == "Horeg1")
            {
                aoeRenderer.material.color = Color.red;
            } 
            else if (horeg == "Horeg2")
            {
                aoeRenderer.material.color = Color.blue;
            }

            Debug.Log($"{horeg} Normal Attack");
        }

        // Destroy AOE
        Destroy(aoeInstance, duration);
    }

    void Metronome()
    {
        if (Time.time >= nextBeatTime)
        {
            nextBeatTime += beatInterval;
        }
    }
}
