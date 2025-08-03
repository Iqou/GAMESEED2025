using UnityEngine;

public class soundBulletforSoundTank : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;
    public float moveSpeed = 12f;
    private Vector3 moveDirection;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector3 dir)
    {
        moveDirection = dir.normalized;

        if (moveDirection != Vector3.zero)
            transform.forward = moveDirection;
    }

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
                Debug.Log($"Player terkena sound bullet! Damage: {damage}");
            }
            Destroy(gameObject);
        }
        
        if (!other.CompareTag("NPC"))
        {
            Destroy(gameObject);
        }
    }
}
