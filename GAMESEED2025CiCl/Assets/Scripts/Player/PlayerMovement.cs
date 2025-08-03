using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 4f;
    public LayerMask obstacleMask;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on the player!");
        }
    }

    public Animator animator;
    public SpriteRenderer playerRenderer;

    void Update()
    {
        Vector3 startPos = transform.position;
        transform.position = new Vector3(startPos.x, 0.2f, startPos.z);
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
                    //isMoving = false;
                }
            }
        }
    }

    void MoveToTarget()
    {
        if (!isMoving) return;

        // Implementasi upgrade player speed
        float finalSpeed = speed * playerStats.moveSpeedMultiplier;

        Vector3 target = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * finalSpeed * Time.deltaTime;

        AnimationHandler(direction);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            isMoving = false;
            animator.SetBool("Moving", false);
        }
    }

    void AnimationHandler(Vector3 direction)
    {
        animator.SetBool("Moving", true);

        PlayerStats playerStatus = GetComponent<PlayerStats>();

        int horegLevel = playerStatus.unlockedHoregSlots;

        animator.SetInteger("Level", horegLevel);

        animator.ResetTrigger("Right");
        animator.ResetTrigger("Left");
        animator.ResetTrigger("Up");
        animator.ResetTrigger("Back");

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            if (direction.x > 0)
            {
                animator.SetTrigger("Right");
            }else
            {
                animator.SetTrigger("Left");
            }
        }
        else
        {
            if (direction.z > 0)
            {
                animator.SetTrigger("Back");
            }
            else
            {
                animator.SetTrigger("Up");
            }
        }
    }
}
