using UnityEngine;

public class SoundAnimation : MonoBehaviour
{
    public Animator animator;

    void Update()
    {
        animator.SetTrigger("AoE");
    }
}
