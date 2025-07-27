using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject aoePrefab;
    public float aoeRadius = 7.0f;
    private float aoeDuration = 3.0f;
    private float nextAttack = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Tekan Spasi untuk Attack jika tidak cooldown
        if (Input.GetKeyDown(KeyCode.Space) & Time.time >= nextAttack)
        {
            TriggerAOE();
            nextAttack = Time.time + aoeDuration;
        }
    }

    // Untuk Trigger AOE
    void TriggerAOE()
    {
        // Instanstiate AOE
        GameObject aoeInstance = Instantiate(aoePrefab, transform.position, Quaternion.identity);

        // AOE Radius
        aoeInstance.transform.localScale = new Vector3(aoeRadius, 0.1f, aoeRadius);

        // Destroy AOE
        Destroy(aoeInstance, aoeDuration);
    }
}
