using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.UI;
using UnityEngine;

public class RunItDown : MonoBehaviour
{
    [SerializeField] private Transform playerRef;
    [SerializeField] private Transform leftRay;
    [SerializeField] private Transform rightRay;
    
    [SerializeField] private float speed = 500;
    [SerializeField] private float stareRange = 25;
    [SerializeField] private float aggroRange = 18;
    [SerializeField] private float triggerRange = 4;

    [SerializeField] private float gravityModifier;
    [SerializeField] private float m_DistanceFromGround;
    [SerializeField] private float obstacleDetectionDistance;
    [SerializeField] private int followWaypointsNumber = 10;
    
    public enum triggerState
    {
        Calm,
        Stare,
        Aggro,
        Seek
    }

    public triggerState currState;
    public bool alive;
    
    // seek player stored memories
    private int currWayPointIndex;
    private int numberOfWayPoints;

    private Rigidbody rb;
    private MeshRenderer[] mat;
    private List<GameObject> pieces;
    private TopDownMovementController playerScript;
    private void Start()
    {
        playerRef = GameObject.FindWithTag("CharacterRig").transform;
        rb = GetComponent<Rigidbody>();
        mat = GetComponentsInChildren<MeshRenderer>();
        playerScript = FindObjectOfType<TopDownMovementController>();
        currState = triggerState.Calm;
        alive = true;
        numberOfWayPoints = playerScript.numberOfWayPoints;
    }

    private void FixedUpdate()
    {
        if (alive)
        {
            HandleMobStates();
            // custom gravity
            if (!OnTheGround() || rb.velocity.y > 0)
            {
                float gravity = 30f * gravityModifier;
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - gravity ,rb.velocity.z);
            }
        }
    }
    
    void HandleMobStates()
    {
        Vector3 dir = playerRef.position - transform.position;
        dir.y = transform.position.y;

        Vector3 leftEyeDir = playerRef.transform.position - leftRay.transform.position;
        Vector3 rightEyeDir= playerRef.transform.position - rightRay.transform.position;
        
        switch (currState)
        {
            case triggerState.Calm:
                HandleCalmState(dir, leftEyeDir, rightEyeDir);
                return;
            case triggerState.Stare:
                HandleStareState(dir, leftEyeDir, rightEyeDir);
                return;
            case triggerState.Aggro:
                HandleAggroState(dir, leftEyeDir, rightEyeDir);
                return;
            case triggerState.Seek:
                HandleSeekState(dir, leftEyeDir, rightEyeDir);
                return;
        }
    }

    void HandleCalmState(Vector3 dir, Vector3 left, Vector3 right)
    {
        if (dir.magnitude < stareRange) 
        {
            if (CheckRayCastHitPlayer(left, right))
            {
                currState = triggerState.Stare;
            }
        }
    }

    void HandleStareState(Vector3 dir, Vector3 left, Vector3 right)
    {
        if (dir.magnitude < aggroRange)
        {
            if (CheckRayCastHitPlayer(left, right))
            {
                currState = triggerState.Aggro;
                return;
            }
        }

        if (dir.magnitude > stareRange)
        {
            currState = triggerState.Calm;
            return;
        }
    
        HandleSmoothRotation(dir);
    }

    void HandleAggroState(Vector3 dir, Vector3 left, Vector3 right)
    {
        if (dir.magnitude < triggerRange && CheckRayCastHitPlayer(left, right))
        {
            StartCoroutine(DeathCoroutine());
            alive = false;
            return;
        }

        if (dir.magnitude > stareRange)
        {
            currState = triggerState.Calm;
            return;
        }
        
        HandleRunItDown(dir, left, right);
        HandleRotation();
    }

    void HandleSeekState(Vector3 dir, Vector3 left, Vector3 right)
    {
        // looping through previous player waypoints and run it down until raycast hit
        if (dir.magnitude < aggroRange && CheckRayCastHitPlayer(left, right))
        {
            currState = triggerState.Aggro;
            return;
        }

        if (dir.magnitude > stareRange)
        {
            currState = triggerState.Calm;
            return;
        }
        
        if (currWayPointIndex >= numberOfWayPoints - 1)
        {
            // cannot seek player
            currState = triggerState.Aggro;
            return;
        }
        
        // looping through previous player waypoints and run it down until raycast hit
        // just pass in the waypoint dir in handle run it down
        
        Vector3 wayPoint = playerScript.wayPoints[currWayPointIndex];
        Vector3 newDir = wayPoint - transform.position;
        newDir.y = transform.position.y;
        
        HandleRunItDown(newDir, left, right);
        HandleRotation();

        if ((wayPoint - transform.position).magnitude <= 2)
        {
            // this index should also be incremented when the player updates its waypoints
            currWayPointIndex += 1;
        }
        
    }
    
    void HandleRunItDown(Vector3 dir, Vector3 leftEye, Vector3 rightEye)
    {
        // todo
        // ====================================================================
        // get the direction from mob to player, make a raycast toward player with a set distance.
        
        // if the raycast hit player, then move straight toward player 
        
        // if hit wall, adjust the raycast angle by certain degrees on both sides.
        
        // ====================================================================
        // if the adjusted degree is less than 180
        
        // if the raycast does not hit anything in a set distance,
        // then move toward that direction then repeat until reach player
        
        // if the raycast does hit, keep adjusting until more than 180 degrees

        if (CheckRayCastHitPlayer(leftEye, rightEye))
        {
            rb.velocity = Vector3.Slerp(rb.velocity, dir, 0.1f).normalized * (speed * Time.fixedDeltaTime);
        }
        else
        {
            float angle = 0;
            while (angle < 45)
            {
                angle += 3;
                Vector3 nextDirLeft = Quaternion.AngleAxis(angle, Vector3.up) * dir;
                Vector3 nextDirRight = Quaternion.AngleAxis(-angle, Vector3.up) * dir;

                var left = CheckRayCastHit(nextDirLeft);
                var right = CheckRayCastHit(nextDirRight);

                if (!left)
                {
                    Vector3 goLeft = Quaternion.AngleAxis(Mathf.Clamp(3f * angle, 30f, 180f), Vector3.up) * dir;
                    rb.velocity = Vector3.Slerp(rb.velocity, goLeft, 0.1f).normalized * (speed * Time.fixedDeltaTime);
                    return;
                }

                if (!right)
                {
                    Vector3 goRight = Quaternion.AngleAxis(Mathf.Clamp(-angle * 3f, -180f, -30f), Vector3.up) * dir;
                    rb.velocity = Vector3.Slerp(rb.velocity, goRight, 0.1f).normalized * (speed * Time.fixedDeltaTime);
                    return;
                }
            }
            
            // this happens when a bigger wall is straight in front of the player 
            // mob goes to seek state if reached this, and set WayPointIndex to a variable number
            if (currState == triggerState.Seek)
            {
                if (currWayPointIndex < numberOfWayPoints - 1)
                {
                    currWayPointIndex += 1;
                }
                return;
            }
            
            currState = triggerState.Seek;
            currWayPointIndex = numberOfWayPoints - followWaypointsNumber;
        }
    }
    
    void HandleSmoothRotation(Vector3 dir)
    {
        var neededRotation = Quaternion.LookRotation(
            new Vector3(dir.x, transform.position.y, dir.z)
        );
        
        var rotation = 
            Quaternion.RotateTowards(
                transform.rotation, 
                neededRotation,
                6f);

        gameObject.transform.rotation = rotation;
    }
    
    void HandleRotation()
    {
        var rotation = Quaternion.LookRotation(
            new Vector3(rb.velocity.x, 0, rb.velocity.z)
        );

        gameObject.transform.rotation = rotation;
    }
    
    bool CheckRayCastHitPlayer(Vector3 leftEye, Vector3 rightEye)
    {
        RaycastHit leftHit;
        RaycastHit rightHit;
        
        bool left = Physics.Raycast(leftRay.position, leftEye, out leftHit, leftEye.magnitude + 10);
        bool right = Physics.Raycast(rightRay.position, rightEye, out rightHit, rightEye.magnitude + 10);

        if (left && right)
        {
            if (leftHit.transform.name == "CharacterRigTopDown" && rightHit.transform.name == "CharacterRigTopDown")
            {
                return true;
            }
        }
        return false;
    }

    bool CheckRayCastHit(Vector3 dir)
    {
        // only returns false if both not hit
        return (
            Physics.Raycast(leftRay.position, dir, obstacleDetectionDistance)
            ||
            Physics.Raycast(rightRay.position, dir, obstacleDetectionDistance)
        );
    }
    
    bool OnTheGround()
    {
        return Physics.Raycast(transform.position, -Vector3.up, m_DistanceFromGround);
    }

    IEnumerator DeathCoroutine()
    {
        Color original = mat[0].material.color;

        StartCoroutine(Spin());
        
        for (int i = 0; i < 5; i++)
        {
            foreach (var mesh in mat)
            {
                mesh.material.color = Color.white;
            }

            yield return new WaitForSeconds(0.1f);
            
            foreach (var mesh in mat)
            {
                mesh.material.color = original;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        Explode();
    }

    IEnumerator Spin()
    {
        while (true)
        {
            transform.Rotate(0,15,0);
            yield return new WaitForFixedUpdate();
        }
    }

    void Explode()
    {
        DeathAnimation();
        StopAllCoroutines();
        Destroy(gameObject);
    }
    
    void DeathAnimation()
    {
        var explosionForce = 2000;
        var explosionRadius = 15;

        Vector3 explosionPos = new Vector3(transform.position.x, transform.position.y - 10, transform.position.z);
        
        // this finds all colliders in a sphere with explosionRadius at explosionPos
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        foreach (Collider c in colliders)
        {
            Rigidbody rb = c.GetComponent<Rigidbody>();
            if (rb)
            {
                // boom
                if (rb.transform.name == "CharacterRigTopDown")
                {
                    playerScript.paralyzed = true;
                    playerScript.UnParalyze();
                }
                rb.AddForce((rb.position - explosionPos).normalized * explosionForce);
            }
        }
    }
}
