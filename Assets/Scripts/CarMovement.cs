using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;



public class CarMovement : MonoBehaviour
{

    [Header("Tire Stabilizer References")]
    
    [SerializeField] private GameObject tireRF; // Right Front
    [SerializeField] private GameObject tireRB; // Right Back
    [SerializeField] private GameObject tireLF; // Left Front
    [SerializeField] private GameObject tireLB; // Left Back
    
    [SerializeField] private GameObject rayStart; 
    
    
    
    public List<tireInfo> tireInfos = new List<tireInfo>(); // RF, RB, LF, LB
    
    
    [Header("Tire Stabilizer Settings")]
    [SerializeField] private float tireRaySize = 3f;
    [SerializeField] private float tireSize = 1f;
    [Range(0f, 1000f)]
    [SerializeField] private float springConstant = 100f;
    [Range(0f, 1000f)]
    [SerializeField] private float damperConstant = 10f;
    
    private LayerMask layerMask;
    
    private Rigidbody rigidbody;
    
    private Quaternion targetInitialRotation;
    
    private Quaternion targetFinalRotation;
    
    
    [Header("Input Variables")]
    [SerializeField] InputManager inputManager;
    private float trnValue;     // Turn value
    private float trtlValue;    // Throttle value
    private float brkValue;     // Brake Value
    private float splValue;     // Special Value
    private float splTurnValue; // Special Turn Value
    
    [Header("Movement Variables")]
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float groundDrag;
    [SerializeField] private float strSensitvity; // Steering sensitivity
    private Vector3 groundNormal;
    
    private float targetSpeed = 0f;
    public float curSpeed;
    
    private Vector3 moveDirection;
    
    private bool isGrounded;
    
    public bool debugTurn;
    public bool debugMove;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("Ground");
        
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        
        targetInitialRotation = rigidbody.rotation;
        targetFinalRotation = rigidbody.rotation;
    }
    void Update()
    {
        GetInput();
    }

    void FixedUpdate()
    {
        PlayerStabilization();
        PlayerRotation();
        PlayerMovement();
        
        ApplyDrag();
        SpeedControl();
    }
    
    public void GetInput()
    {
        trnValue = inputManager.GetTurn();
        trtlValue = inputManager.GetThrottle();
        brkValue = inputManager.Getbrake();
        splValue = inputManager.GetSpecial();
        splTurnValue = inputManager.GetSpecialTurn();
        
        if(debugMove)
        {
            trtlValue = 1f;
        }
        if(debugTurn)
        {
            trnValue = 1f;
        }
    }
    
    #region Movement
    // movement code goes here
    void ApplyDrag()
    {
        if(isGrounded)
        {
            rigidbody.linearDamping = groundDrag;
        }
        else
        {
            rigidbody.linearDamping = 0f;
        }
    }
    
    void PlayerMovement()
    {
        if(trtlValue > 0)
        {
            targetSpeed = maxSpeed;
        }
        else
        {
            targetSpeed = 0f;
        }
        
        if(isGrounded)
        {
            Vector3 targetForward = targetFinalRotation * Vector3.forward;
            float forwardSpeed = Vector3.Dot(rigidbody.linearVelocity,targetForward);
            curSpeed = Mathf.Lerp(forwardSpeed,targetSpeed,Time.fixedDeltaTime * 10f);
            
            //Debug.DrawRay(rayStart.transform.position, targetForward * 10f, Color.blue);
            moveDirection = targetForward * trtlValue;
            
            Debug.DrawRay(rayStart.transform.position, moveDirection.normalized * 10f, Color.red);
            
            rigidbody.AddForce(moveDirection.normalized * curSpeed * 10f,ForceMode.Force);
        }
    }
    void PlayerRotation()
    {
        float turnAmount = 0f;

        if (curSpeed > 0.01f || debugTurn)
        {
            turnAmount = strSensitvity * trnValue * 100f * Time.fixedDeltaTime;
        }

        Vector3 targetUp = targetInitialRotation * Vector3.up;

        Quaternion steeringRotation = Quaternion.AngleAxis(turnAmount,targetUp);

        targetFinalRotation = steeringRotation * targetInitialRotation;

        Quaternion newRotation = Quaternion.Slerp(rigidbody.rotation,targetFinalRotation,Time.fixedDeltaTime * 5f);

        rigidbody.MoveRotation(newRotation);
    }
    
    void SpeedControl()
    {
        Vector3 flatVel = rigidbody.linearVelocity;
        
        if(flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rigidbody.linearVelocity = limitedVel;
        }
    
    }
    
    
    #endregion Movement
    
    #region Player Stabilization
    void PlayerStabilization()
    {
        RaycastHit hit;
        
        // This will show a ray that is meant to represent the tire comment this out when its not used
        Debug.DrawRay(rayStart.transform.position, -rayStart.transform.up * tireRaySize, Color.yellow);
        
        // Does the ray intersect any objects excluding the player layer
        
        
        
        if (Physics.Raycast(rayStart.transform.position, -rayStart.transform.up, out hit, tireRaySize, layerMask))
        {
            Vector3 velocity = rigidbody.linearVelocity;
            Vector3 rayDir = -rayStart.transform.up;
            
            float rayDirVel = Vector3.Dot(rayDir,velocity);
            
            float x = hit.distance - tireSize;
            
            float springForce = (x * springConstant) - (rayDirVel * damperConstant);
            rigidbody.AddForce(rayDir * springForce);
            
            groundNormal = hit.normal;

            // Project the car's current forward direction onto the ground plane
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

            // Build rotation using forward + ground normal as up
            targetInitialRotation = Quaternion.LookRotation(forward, groundNormal);
            
            isGrounded = true;

            //transform.rotation = targetRotation;
        }else
        {
            // Vector3 flatForward =Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;

            // if (flatForward.sqrMagnitude > 0.001f)
            // {
            //     targetRotation = Quaternion.LookRotation(flatForward,Vector3.up);
            // }
            
            isGrounded = false;
        }
        
        //transform.rotation = Quaternion.Slerp(transform.rotation,targetInitialRotation,Time.fixedDeltaTime * 5f);

    }
    #endregion Player Stabilization
}
