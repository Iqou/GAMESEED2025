using UnityEngine;

public class MoveSpawn : MonoBehaviour
{
    public Rigidbody2D rb;

    public void SetSpeed(float newSpeed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * newSpeed;
    }

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }
}