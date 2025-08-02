using UnityEngine;

public class sendalProjectile : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;
    public float moveSpeed = 10f;
    private Vector3 moveDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player terkena sendal! Damage: {damage}");

            // Kalau ada script health di player:
            // other.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            Destroy(gameObject);
        }

        if (!other.CompareTag("NPC"))
        {
            Destroy(gameObject);
        }
    }


}
