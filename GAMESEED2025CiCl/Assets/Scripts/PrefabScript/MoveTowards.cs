using UnityEngine;

public class MoveTowards : MonoBehaviour
{
    public float speed = 40.0f;
    private Vector3 moveDirection = Vector3.right;

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    void Update()
    {
        transform.Translate(moveDirection * Time.deltaTime * speed, Space.World);
    }
}