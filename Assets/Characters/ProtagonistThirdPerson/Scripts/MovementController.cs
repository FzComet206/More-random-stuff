using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [Header("Input Settings")] 
    [SerializeField] private InputAction movement; // movement input

    [SerializeField] private InputAction jump; // jump input

    [Header("Adjustments")] 
    [SerializeField] private float movementFactor = 6f; // movement factor

    [SerializeField] private float jumpVelocity = 6f; // jumping velocity
    [SerializeField] private float groundedDistance = 0.01f; // used to determine when the player can jump
    [SerializeField] private float turnDampingTime = 0.1f; // time for the player to turn to its movement direction
    [SerializeField] private float jumpingVelocityLimit = 6f;
    [SerializeField] private float gravityModifier;

    [Header("Dependencies")] 
    [SerializeField] private Transform camPosition; // position of the camera

    private Rigidbody rigidbody; // reference to the character controller
    private bool isJumping;

    private float turnDampingVelocity; // a temp variable to store the current turning velocity
    
    public bool IsJumping => isJumping;

    public bool IsRunning
    {
        get;
        private set;
    }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        // enable the input actions
        movement.Enable();
        jump.Enable();
    }

    private void FixedUpdate()
    {
        if (isGrounded() && isJumping) isJumping = false;
        
        Jump();
        Move();
        LimitHeight();
        
        // copied from another script
        if (!isGrounded() || rigidbody.velocity.y > 0)
        {
            float gravity = 10f * gravityModifier;
            var velocity = rigidbody.velocity;
            velocity = new Vector3(velocity.x, velocity.y - gravity ,velocity.z);
            rigidbody.velocity = velocity;
        }
    }

    private void Update()
    {
        Rotate();
    }

    /// <summary>
    /// Moves the player relative to the camera. Also rotates the player according the moving direction.
    /// </summary>
    private void Move()
    {
        var horizontal = movement.ReadValue<Vector2>().x; // x value input
        var vertical = movement.ReadValue<Vector2>().y; // z value input

        // angle of the movement direction
        var targetAngle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg + camPosition.eulerAngles.y;

        // if there's input detected
        if (!(horizontal == 0f && vertical == 0f))
        {
            // create player's moving direction relative to the camera rotation
            var moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * movementFactor;
            
            // move the player
            rigidbody.velocity = new Vector3(moveDirection.x * Time.fixedDeltaTime, rigidbody.velocity.y,
                moveDirection.z * Time.fixedDeltaTime);

            IsRunning = true;
        }
        
        else if (isGrounded())
        {
            rigidbody.velocity = new Vector3(0f, rigidbody.velocity.y, 0f);
            IsRunning = false;
        }
        else
        {
            IsRunning = false;
        }
    }

    private void Rotate()
    {
        var horizontal = movement.ReadValue<Vector2>().x; // x value input
        var vertical = movement.ReadValue<Vector2>().y; // z value input

        // angle of the movement direction
        var targetAngle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg + camPosition.eulerAngles.y;

        // if there's input detected
        if (!(horizontal == 0f && vertical == 0f))
            // rotates the player by damping the angle (watched Brackey's video lol)
            transform.rotation = Quaternion.Euler(0f,
                Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnDampingVelocity, turnDampingTime),
                0f);
    }

    /// <summary>
    /// Causes the player to jump.
    /// </summary>
    private void Jump()
    {
        if (Mathf.Abs(jump.ReadValue<float>()) > 0f && isGrounded()) // if there's jump input and the player can jump
        {
            // jump
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);
            isJumping = true;
        }
    }

    private void LimitHeight()
    {
        if (!isJumping && rigidbody.velocity.y > jumpingVelocityLimit)
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpingVelocityLimit, rigidbody.velocity.z);
            Debug.Log("Limited");
        }
    }

    /// <summary>
    /// Uses raycast to determine whether player is currently on the ground. Change groundedDistance to adjust the
    /// jumping smoothness.
    /// </summary>
    /// <returns> whether the player can jump/on the ground </returns>
    private bool isGrounded()
    {
        return Physics.Raycast(GetComponent<Collider>().bounds.center, Vector3.down,
            GetComponent<Collider>().bounds.extents.y + groundedDistance);
    }

    private void OnDisable()
    {
        // disable the input actions
        movement.Disable();
        jump.Disable();
    }
}