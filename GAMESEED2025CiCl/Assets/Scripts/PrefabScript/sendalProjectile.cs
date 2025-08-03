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
            OverworldHealth playerHealth = other.GetComponent<OverworldHealth>();
            if (playerHealth != null)
            {
                playerHealth.ChangeHealth(-damage);
                Debug.Log($"Player terkena pentungan! Damage: {damage}");
            }

            Destroy(gameObject);
        }

        if (!other.CompareTag("NPC"))
        {
            Destroy(gameObject);
        }
    }


}
