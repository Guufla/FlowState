using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;



public class CarMovement : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private CarStateMachine stateMachine;
    
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
    public float trnValue;     // Turn value
    public float trtlValue;    // Throttle value
    public float brkValue;     // Brake Value
    public float splValue;     // Special Value
    public float splTurnValue; // Special Turn Value
    
    [Header("Movement Variables")]
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float groundDrag;
    [SerializeField] private float strSensitvity; // Steering sensitivity
    [SerializeField] private float strSpeed;      // Steering speed
    [SerializeField] private float acceleration;
    private Vector3 groundNormal;
    
    private float targetSpeed = 0f;
    public float curSpeed;
    
    private Vector3 moveDirection;
    
    private float turnAmount = 0f;
    
    
    [Header("Drifting Variables")]
    [SerializeField] private float maxDriftSpeed = 3f;
    [SerializeField] private float strDriftSensitvity; // Steering sensitivity
    [SerializeField] private float strDriftSpeed;      // Steering speed
    
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
        
        targetSpeed = maxSpeed;
    }
    void Update()
    {
        GetInput();
        
        //Debug.Log(stateMachine.state.ToString());
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
    
    #region Basic Movement
    // movement code goes here
    void ApplyDrag()
    {
        if(stateMachine.isGrounded)
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
        if(stateMachine.state == CarState.drifting)
        {
            // targetSpeed = maxDriftSpeed;
            targetSpeed = Mathf.Lerp(targetSpeed,maxDriftSpeed,Time.fixedDeltaTime * 0.5f);
        }
        else
        {
            targetSpeed = Mathf.Lerp(targetSpeed,maxSpeed,Time.fixedDeltaTime * 2f);
        }
        
        if(stateMachine.isGrounded)
        {
            Vector3 targetForward = targetFinalRotation * Vector3.forward;
            float forwardSpeed = Vector3.Dot(rigidbody.linearVelocity,targetForward);
            curSpeed = Mathf.Lerp(forwardSpeed,targetSpeed,Time.fixedDeltaTime * 10f);
            
            //Debug.DrawRay(rayStart.transform.position, targetForward * 10f, Color.blue);
            moveDirection = targetForward * trtlValue;
            
            Debug.DrawRay(rayStart.transform.position, moveDirection.normalized * 10f, Color.red);
            
            if(curSpeed >= 0 && stateMachine.state == CarState.braking)
            {
                rigidbody.AddForce(moveDirection.normalized * curSpeed * acceleration * (-brkValue/2f),ForceMode.Force);
            }
            else if(curSpeed >= 0 && stateMachine.state == CarState.driving)
            {
                rigidbody.AddForce(moveDirection.normalized * curSpeed * acceleration * trtlValue,ForceMode.Force);
            }
            else if(curSpeed >= 0 && stateMachine.state == CarState.drifting)
            {
                rigidbody.AddForce(moveDirection.normalized * curSpeed * acceleration * trtlValue,ForceMode.Force);
            }
            else if(debugMove)
            {
                rigidbody.AddForce(moveDirection.normalized * curSpeed * acceleration * trtlValue,ForceMode.Force);
            }
            else
            {
                rigidbody.AddForce(moveDirection.normalized * (maxSpeed - curSpeed) * (acceleration/1.5f),ForceMode.Force);
            }
            
        }
    }
    void PlayerRotation()
    {

        // NEED TO CHANGE THESE TO ALL BE SLIGHTLY DIFFERENT LATER ON
        if (stateMachine.state == CarState.driving || debugTurn)
        {
            turnAmount = Mathf.Lerp(turnAmount,strSensitvity * trnValue, Time.fixedDeltaTime * strSpeed);
        }
        else if (stateMachine.state == CarState.drifting)
        {
            turnAmount = Mathf.Lerp(turnAmount,strDriftSensitvity * trnValue, Time.fixedDeltaTime * strDriftSpeed);
        }
        else if(stateMachine.state == CarState.air)
        {
            turnAmount = Mathf.Lerp(turnAmount,strSensitvity * trnValue, Time.fixedDeltaTime * strSpeed);
        }
        else
        {
            turnAmount = Mathf.Lerp(turnAmount,0f, Time.fixedDeltaTime *1.4f);;
        }

        Vector3 targetUp = targetInitialRotation * Vector3.up;

        Quaternion steeringRotation = Quaternion.AngleAxis(turnAmount,targetUp);

        targetFinalRotation = steeringRotation * targetInitialRotation; 

        //Quaternion newRotation = Quaternion.Slerp(rigidbody.rotation,targetFinalRotation,Time.fixedDeltaTime * 5f);
        Quaternion newRotation = targetFinalRotation;

        rigidbody.MoveRotation(newRotation);
    }
    
    void SpeedControl()
    {
        Vector3 flatVel = rigidbody.linearVelocity;
        
        if(flatVel.magnitude > targetSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * targetSpeed;
            rigidbody.linearVelocity = limitedVel;
        }
    
    }
    
    
    #endregion Basic
    
    #region Drift
    void PlayerDrift()
    {
        
    }
    
    
    #endregion Drift
    
    
    
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
            
            stateMachine.isGrounded = true;

            //transform.rotation = targetRotation;
        }else
        {
            // Vector3 flatForward =Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;

            // if (flatForward.sqrMagnitude > 0.001f)
            // {
            //     targetRotation = Quaternion.LookRotation(flatForward,Vector3.up);
            // }
            
            stateMachine.isGrounded = false;
        }
        
        //transform.rotation = Quaternion.Slerp(transform.rotation,targetInitialRotation,Time.fixedDeltaTime * 5f);

    }
    #endregion Player Stabilization
    
    #region Public functions
    public float GetCurrentTurn()
    {
        return turnAmount;
    }   
    public float GetMaxTurn()
    {
        return strSensitvity*2;
    } 
    #endregion Public functions
}
