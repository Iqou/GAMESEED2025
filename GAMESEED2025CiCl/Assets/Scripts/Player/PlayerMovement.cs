using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 4f;
    public LayerMask obstacleMask;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Update()
    {
        HandleClick();
        MoveToTarget();
    }

    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                // Check if the direct path is blocked
                Vector3 direction = (hit.point - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, hit.point);

                if (!Physics.Raycast(transform.position, direction, distance, obstacleMask))
                {
                    targetPosition = hit.point;
                    isMoving = true;
                }
                else
                {
                    Debug.Log("Path blocked by obstacle.");
                    isMoving = false;
                }
            }
        }
    }

    void MoveToTarget()
    {
        if (!isMoving) return;

        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }
}
