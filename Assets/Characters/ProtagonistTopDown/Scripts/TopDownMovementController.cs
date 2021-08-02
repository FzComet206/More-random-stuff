using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownMovementController : MonoBehaviour
{
    [Header("Movement Tuning")] [SerializeField]
    private float playerVelocity = 800f;

    [SerializeField] private float gravityModifier;
    [SerializeField] private float jumpVelocity = 20f;

    [Header("Dashing")] 
    [SerializeField] private float dashVelocityFactor = 2.5f;
    [SerializeField] private float dashCooldown = 1f; // dash cooldown time
    [SerializeField] private int dashIterations = 8;
    private float nextDashTime; // dash cooldown counter
    private bool isDashing = false;
    

    [Header("Input System")] 
    [SerializeField] private InputAction movement;
    [SerializeField] private InputAction jump;
    [SerializeField] private InputAction dash;
    // mouse vector keep tracks of the mouse position
    [SerializeField] private InputAction mouseVector;

    // handle ray trace to the ground
    [SerializeField] private float m_DistanceFromGround;
    // number of waypoints to be stored
    public int numberOfWayPoints = 20;
    // distance for which each waypoints is defined
    public float wayPointsDistance = 4f;

    // handle player and pet smooth rotation
    
    // rigidbody controls movement
    [SerializeField] private Rigidbody rb;
    // modelRef controls model rotation
    [SerializeField] private Transform modelRef;
    // rigRef controls outer rig transform related raycast
    [SerializeField] private Transform rigRef;
    // cam controls raycast alone with rigRef
    [SerializeField] private Camera cam;

    // if this is set to true from other scripts, the player movement only depends on rigidbody's force
    public bool paralyzed;

    // animator states
    public bool IsJumping;
    public bool IsRunning;

    // public waypoints on player path
    public List<Vector3> wayPoints;

    private void Awake()
    {
        wayPoints = new List<Vector3>();
        wayPoints.Add(transform.position);
    }

    void FixedUpdate()
    {
        UpdateWayPointsPathPerFrame();

        if (!HandleDashInput() && !isDashing)
        {
            HandleMovementInput(); // if player is dashing, they shouldn't be able to move
            HandleJump();
        }

        // custom gravity
        if (!OnTheGround() || rb.velocity.y > 0)
        {
            float gravity = 30f * gravityModifier;
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - gravity ,rb.velocity.z);
        }
    }

    /// <summary>
    /// Handles dashing and its cooldown time
    /// </summary>
    /// <returns> whether player dashed. Used to determine whether player can move when dashing.</returns>
    bool HandleDashInput()
    {
        if (dash.ReadValue<float>() != 0 && Time.time > nextDashTime)
        {
            // reset cooldown
            nextDashTime = Time.time + dashCooldown;

            // dash
            isDashing = true;
            StartCoroutine(Dash());

            return true;
        }

        return false;
    }

    IEnumerator Dash()
    {
        Vector3 vel = new Vector3(rb.velocity.x, 0, rb.velocity.z) * dashVelocityFactor;
        
        for (int i = 0; i < dashIterations; i++)
        {
            rb.velocity = vel;
            // wait for the next frame
            yield return new WaitForFixedUpdate();
        }
        isDashing = false; // no longer dashing
    }

    void HandleMovementInput()
    {
        if (paralyzed) { return; }
        
        // fixed deltatime on fixed update
        float frameTime = Time.fixedDeltaTime;

        // capture inputs as 2D vectors
        float throwX = movement.ReadValue<Vector2>().x;
        float throwZ = movement.ReadValue<Vector2>().y;

        // set velocity values
        var transform1 = transform;
        var speedX = transform1.right * (throwX * frameTime * playerVelocity);
        var speedZ = transform1.forward * (throwZ * frameTime * playerVelocity);

        // add speedX and speedY for full velocity, replace y values with original velocity so it doesn't affect gravity
        var velocityPerFrame = new Vector3(speedX.x + speedZ.x, rb.velocity.y, speedX.z + speedZ.z);

        // set mag and dir of velocity for rb
        rb.velocity = velocityPerFrame;

        // Handles player smooth rotation
        // Handles player smooth rotation
        if (!(throwX == 0 && throwZ == 0))
        {
            IsRunning = true;
            // player should rotate to this direction when moving
            Quaternion lookDirection =
                Quaternion.LookRotation(- (new Vector3(velocityPerFrame.x, 0, velocityPerFrame.z)));

            // use slerp for smooth transition
            modelRef.rotation = Quaternion.Slerp(modelRef.rotation, lookDirection, frameTime * 12f);
        }
        else
        {
            IsRunning = false;
        }
    }

    void HandleJump()
    {
        var jumping = jump.ReadValue<float>();
        // allowed to jump only if object has already 
        if (OnTheGround() && jumping > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);

        }
        
        if (!OnTheGround())
        {
            IsJumping = true;
        }
        else
        {
            IsJumping = false;
        }
    }

    bool OnTheGround()
    {
        return Physics.Raycast(transform.position, -Vector3.up, m_DistanceFromGround);
    }

    void LookCursorDirecton()
    {
        // need to update turret height so that it rotates ignore y value
        var turretHeight = rigRef.position.y;

        if (cam)
        {
            // get ray from camera to cursor position
            Ray cursorRay = cam.ScreenPointToRay(mouseVector.ReadValue<Vector2>());

            // fixed y value to player head in order to created a plane that is fixed to player's head
            Vector3 fixedPoint = new Vector3(rigRef.position.x, turretHeight, rigRef.position.z);

            // create a plane with 0,1,0 normal vector with point defined above
            var mathematicalPlane = new Plane(Vector3.up, fixedPoint);

            // if ray hit on the plane, out variable hit, then get the hit position
            float hit;
            if (mathematicalPlane.Raycast(cursorRay, out hit))
            {
                Vector3 hitPos = cursorRay.GetPoint(hit);
                // draws a cool line idk why i'm adding this
                Debug.DrawLine(cam.transform.position, hitPos, new Color(0.02f, 1f, 0.84f), 0.5f);
                Debug.DrawLine(rigRef.position, hitPos, new Color(1f, 0.2f, 0.45f), 0.5f);
                Debug.DrawRay(cursorRay.origin, cursorRay.direction * 200, Color.red);

                rigRef.transform.LookAt(hitPos);
            }
        }
    }

    public void UnParalyze()
    {
        Invoke("UnP", 1f);
    }

    void UnP()
    { paralyzed = false; }

    private void UpdateWayPointsPathPerFrame()
    {
        if (CheckValidNewWayPoint())
        {
            wayPoints.Add(transform.position);
        }
        
        if (wayPoints.Count > numberOfWayPoints)
        {
            wayPoints.RemoveAt(0);
        }
    }

    bool CheckValidNewWayPoint()
    {
        // whenever displacement reach the threshold
        return (transform.position - wayPoints[wayPoints.Count - 1]).magnitude > wayPointsDistance;
    }
    
    private void OnEnable()
    {
        movement.Enable();
        jump.Enable();
        mouseVector.Enable();
        dash.Enable();
    }

    private void OnDisable()
    {
        movement.Disable();
        jump.Disable();
        mouseVector.Disable();
        dash.Disable();
    }
}