using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;
using FishNet.Connection;
using FishNet.Object;


public class CarMovement : NetworkBehaviour
{
 
    [Header("Network Initializer")]
    private PlayerInitialization playerinitialization;
    
    [Header("StateMachine")]
    private CarStateMachine stateMachine;

    [Header("Tire Stabilizer Settings")]
    [SerializeField] private GameObject rayStart;
    [SerializeField] private float tireRaySize = 3f;
    [Range(0f, 1000f)]
    [SerializeField] private float springConstant = 100f;
    [Range(0f, 1000f)]
    [SerializeField] private float damperConstant = 10f;
    
    private LayerMask layerMask;
    private Rigidbody rigidbody;
    private Quaternion targetInitialRotation;
    private Quaternion targetFinalRotation;


    [Header("Input Variables")]
    private InputManager inputManager;
    private float trnValue;     // Turn value
    private float trtlValue;    // Throttle value
    private float brkValue;     // Brake Value
    private float splValue;     // Special Value
    private float splTurnValue; // Special Turn Value

    [Header("Road Variables")]
    [SerializeField] private int maxResolution = 10;
    [SerializeField] private float roadOffsetY;
    
    private SplineContainer splines;
    private Vector3 carWorldPoint;
    private Spline curSpline;
    private bool isSplineSet;

    [Header("Movement Variables")]
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float groundDrag;
    [SerializeField] private float strSensitvity; // Steering sensitivity
    [SerializeField] private float strSpeed;      // Steering speed
    [SerializeField] private float acceleration;
    
    private Vector3 groundNormal;
    private Vector3 moveDirection;
    private float targetSpeed = 0f;
    private float turnAmount = 0f;
    private float curSpeed; // Make this public to see how fast the player is going
    
    [Header("Drifting Variables")]
    [SerializeField] private float maxDriftSpeed = 3f;
    [SerializeField] private float strDriftSensitvity; // Steering sensitivity
    [SerializeField] private float strDriftSpeed;      // Steering speed
    [Range(0,1)] [SerializeField] private float minDrift;

    private float startDriftDir = 0;

    [Header("Rotation Variables")]
    [Range(0,25)]
    [SerializeField] float maxAirRotation;


    [Header("Debug Variables")]
    public bool debugTurn;
    public bool debugMove;

    #region Unity Lifecycle

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("Ground");

        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        targetInitialRotation = rigidbody.rotation;
        targetFinalRotation = rigidbody.rotation;

        targetSpeed = maxSpeed;
        
        stateMachine = GameManager.Instance.GetStateMachine();
        inputManager = GameManager.Instance.GetInputManager();
        splines = GameManager.Instance.GetSplineContainer();
        playerinitialization = GetComponent<PlayerInitialization>();
    }

    void Update()
    {
        GetInput();

        //Debug.Log(stateMachine.state.ToString());
    }

    void FixedUpdate()
    {
        stateMachine.isGrounded = CheckGrounded();

        if(stateMachine.isGrounded && !isSplineSet)
        {
            SetCurrentSpline();
        }

        SplineCoordinates();
        PlayerRotation();
        if(stateMachine.isGrounded)
        {
            PlayerStabilization();
        }
        else
        {
            isSplineSet = false;
            PlayerAirStabilization();
        }
        PlayerMovement();

        ApplyDrag();
        SpeedControl();
    }

    #endregion Unity Lifecycle

    #region Input

    public void GetInput()
    {
        if (playerinitialization.isPlayer == false) return;
        
        
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

    #endregion Input

    #region Ground Detection

    bool CheckGrounded()
    {
        if(Physics.Raycast(rayStart.transform.position, -rayStart.transform.up, tireRaySize, layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion Ground Detection

    #region Spline Setup

    void SetCurrentSpline()
    {

        if(splines == null) return;

        float closestSplineDistance = float.PositiveInfinity;
        int ClosestSpline = 0;

        for(int i = 0; i < splines.Splines.Count; i++)
        {
            Vector3 worldPos = transform.position;
            Vector3 localPos = splines.transform.InverseTransformPoint(worldPos);

            Spline spline = splines[i];

            SplineUtility.GetNearestPoint(spline,(float3)localPos,out float3 nearestLocalPointFloat,out float tValue,SplineUtility.PickResolutionDefault,5);

            float currentDistance = Vector3.Distance(localPos,nearestLocalPointFloat);

            if(closestSplineDistance > currentDistance)
            {
                closestSplineDistance = currentDistance;
                ClosestSpline = i;
            }

        }

        curSpline = splines[ClosestSpline];

        isSplineSet = true;
    }

    #endregion Spline Setup

    #region Spline Coordinates And Rotation

    void SplineCoordinates()
    {
        if(splines == null) return;
        if(!isSplineSet) return;

        // if(stateMachine.prevState == CarState.air || stateMachine.prevState == CarState.spiralModeAir)
        // {
        //     rigidbody.linearVelocity = new Vector3(0,0,0);
        // }
        Vector3 worldPos = transform.position;
        Vector3 localPos = splines.transform.InverseTransformPoint(worldPos);

        SplineUtility.GetNearestPoint(curSpline,(float3)localPos,out float3 nearestLocalPointFloat,out float tValue,SplineUtility.PickResolutionDefault,5);

        Vector3 nearestLocalPoint = nearestLocalPointFloat;

        // Gives the tangent which basically is the forward direction of the spline
        Vector3 tangentLocal =((Vector3)SplineUtility.EvaluateTangent(curSpline, tValue)).normalized;

        // Gives the local up direction of the spline
        Vector3 upLocal =((Vector3)SplineUtility.EvaluateUpVector(curSpline, tValue)).normalized;
        groundNormal = splines.transform.TransformDirection(upLocal).normalized;

        // Cross product of the up and tangent is the sideways direction
        Vector3 rightLocal = Vector3.Cross(upLocal, tangentLocal).normalized;


        // Vector from spline center → car
        Vector3 centerToCar = localPos - nearestLocalPoint;

        // Gives the right/left offset from the car to the spline point
        float lateralOffset = Vector3.Dot(centerToCar, rightLocal);

        // Reconstruct point underneath car
        Vector3 adjustedLocalPoint = nearestLocalPoint + (rightLocal * lateralOffset);

        // Add height above road
        Vector3 carLocalPoint = adjustedLocalPoint;
        carLocalPoint +=  upLocal * roadOffsetY;


        // Convert back into world space
        Vector3 adjustedWorldPoint = splines.transform.TransformPoint(adjustedLocalPoint);
        carWorldPoint = splines.transform.TransformPoint(carLocalPoint);

        //Ray ray = new Ray(adjustedWorldPoint, carWorldPoint);
        Debug.DrawLine(adjustedWorldPoint, carWorldPoint, Color.green);
        //transform.position = new Vector3(transform.position.x, nearestWorldPoint.y + roadOffsetY, transform.position.z);

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

        // Build rotation using forward + ground normal as up
        if(stateMachine.isGrounded)
        {
            targetInitialRotation = Quaternion.Slerp(targetInitialRotation,Quaternion.LookRotation(forward, groundNormal),Time.fixedDeltaTime * 20f);
        }
    }

    void PlayerRotation()
    {
        SetDriftDirection();

        // NEED TO CHANGE THESE TO ALL BE SLIGHTLY DIFFERENT LATER ON
        if (stateMachine.state == CarState.driving || debugTurn)
        {
            turnAmount = Mathf.Lerp(turnAmount,strSensitvity * trnValue, Time.fixedDeltaTime * strSpeed);
        }
        else if (stateMachine.state == CarState.drifting)
        {
            if(startDriftDir > 0)
            {
                float adjustedTrnValue = Mathf.InverseLerp(-1,1,trnValue);
                float finalTrnValue = Mathf.Lerp(0.2f,1f, adjustedTrnValue);
                //float finalTrnValue = Mathf.Clamp(trnValue,0 , 1);
                turnAmount = Mathf.Lerp(turnAmount,strDriftSensitvity * finalTrnValue, Time.fixedDeltaTime * strDriftSpeed);
            }
            else
            {
                float adjustedTrnValue = Mathf.InverseLerp(-1,1,trnValue);
                float finalTrnValue = Mathf.Lerp(1f,0.2f, adjustedTrnValue);
                turnAmount = Mathf.Lerp(turnAmount,strDriftSensitvity * -finalTrnValue, Time.fixedDeltaTime * strDriftSpeed);
            }
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

    void SetDriftDirection()
    {
        if(stateMachine.state == CarState.drifting)
        {
            if(startDriftDir == 0 && trnValue != 0)
            {
                startDriftDir = trnValue;
            }
        }
        else
        {
            startDriftDir = 0;
        }
    }

    #endregion Spline Coordinates And Rotation

    #region Stabilization

    void PlayerStabilization()
    {

        Vector3 velocity = rigidbody.linearVelocity;
        Vector3 rayDir = groundNormal;

        float currentHeight = Vector3.Dot(rigidbody.position - carWorldPoint , rayDir);

        float rayDirVel = Vector3.Dot(rayDir,velocity);

        float springForce = (-currentHeight * springConstant) - (rayDirVel * damperConstant);
        rigidbody.AddForce(rayDir * springForce);


    }

    void PlayerAirStabilization()
    {
        Quaternion targetAirRotation = Quaternion.LookRotation(rigidbody.linearVelocity);
        Vector3 rotationClamp = targetAirRotation.eulerAngles;
        if(rotationClamp.x > maxAirRotation)
        {
            rotationClamp.x = maxAirRotation;
        }


        targetAirRotation = Quaternion.Euler(rotationClamp);
        targetInitialRotation = Quaternion.Slerp(targetInitialRotation,targetAirRotation,Time.fixedDeltaTime * 5f);
    }

    #endregion Stabilization

    #region Movement

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

    void SpeedControl()
    {
        Vector3 flatVel = rigidbody.linearVelocity;

        if(flatVel.magnitude > targetSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * targetSpeed;
            rigidbody.linearVelocity = limitedVel;
        }

    }

    #endregion Movement

    #region Public Functions

    public float GetCurrentTurn()
    {
        return turnAmount;
    }

    public float GetMaxTurn()
    {
        return strSensitvity*2;
    }

    #endregion Public Functions
}
