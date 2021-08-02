using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private TopDownMovementController topDownMovementController;
    private Animator animator;

    private void Start()
    {
        topDownMovementController = FindObjectOfType<TopDownMovementController>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        animator.SetBool("isRunning", topDownMovementController.IsRunning);
        animator.SetBool("isJumping", topDownMovementController.IsJumping);
    }
}
