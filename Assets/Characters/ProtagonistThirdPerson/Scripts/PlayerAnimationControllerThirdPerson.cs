using UnityEngine;

public class PlayerAnimationControllerThirdPerson : MonoBehaviour
{
    private MovementController movementController;
    private Animator animator;

    private void Start()
    {
        movementController = GetComponent<MovementController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        animator.SetBool("isRunning", movementController.IsRunning);
        animator.SetBool("isJumping", movementController.IsJumping);
    }
}
