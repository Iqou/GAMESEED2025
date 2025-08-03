using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        Vector3 target = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 direction = target.normalized;

        AnimationHandler(direction);
    }

    void AnimationHandler(Vector3 direction)
    {
        animator.ResetTrigger("Right");
        animator.ResetTrigger("Left");
        animator.ResetTrigger("Front");
        animator.ResetTrigger("Back");

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            if (direction.x > 0)
            {
                animator.SetTrigger("Right");
            }
            else
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
                animator.SetTrigger("Front");
            }
        }
    }
}
