using UnityEngine;

public class pickUpXp : MonoBehaviour
{
    public int expValue = 100;       // Nilai EXP yang diberikan
    public float lifeTime = 10f;     // Hilang otomatis
    public float floatSpeed = 0.5f;  // Sedikit melayang ke atas
    public float rotationSpeed = 60f;// Rotasi untuk efek visual
    private Vector3 startPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        // Efek melayang ke atas
        transform.position = new Vector3(transform.position.x, startPos.y + Mathf.Sin(Time.time * 2f) * floatSpeed, transform.position.z);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddExperience(expValue);
                Debug.Log($"Player mendapatkan {expValue} EXP!");
            }
            else
            {
                Debug.LogWarning("Player tidak memiliki komponen PlayerStats!");
            }

            Destroy(gameObject);
        }
    }
}
