using UnityEngine;

public class pickUpUang : MonoBehaviour
{
    public int coinValue = 5000;     // Nilai koin yang diberikan
    public float lifeTime = 10f;     // Berapa lama sebelum hilang otomatis
    public float rotationSpeed = 90f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddMoney(coinValue);
                Debug.Log($"Player mendapatkan {coinValue} koin!");
            }
            else
            {
                Debug.LogWarning("Player tidak memiliki komponen PlayerStats!");
            }

            Destroy(gameObject);
        }
    }
}
