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
            Debug.Log($"Player mendapatkan {expValue} EXP!");

            // Tambahkan logika menambah EXP di script Player di sini
            // other.GetComponent<PlayerStats>()?.AddExperience(expValue);

            Destroy(gameObject);
        }
    }
}
