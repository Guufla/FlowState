using System;
using UnityEngine;


public class CarStateMachine : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] InputManager inputManager;
    
    
    [Header("State Machine")]

    public CarState state { get; private set; }
    public CarState prevState{ get; set; }

    public bool isGrounded { get; set; }
    public bool isSpiral { get; set; }
    public bool isDrifting { get; set; }
    public bool isMoving { get; set; }
    public bool isBraking { get; set; }
    public bool isDead { get; set; }

    // public event Action<CarState> OnStateChanged;

    #region State Handler

    void Awake()
    {
        state = CarState.idle;
        prevState = CarState.idle;
    }

    void FixedUpdate()
    {
        prevState = state;
        state = true switch
        {
            var _ when isDead => CarState.dead,
            var _ when !isGrounded && isSpiral => CarState.spiralModeAir,
            var _ when !isGrounded => CarState.air,
            var _ when isSpiral => CarState.spiralMode,
            var _ when isMoving && isDrifting => CarState.drifting,
            var _ when isBraking => CarState.braking,
            var _ when isMoving => CarState.driving,
            _ => CarState.idle,
        };

        // Is grounded is set in the car movement script
        isBraking = inputManager.GetBrake() > 0 ? true : false;
        isDrifting = inputManager.GetDrift() > 0 ? true : false;
        isMoving = inputManager.GetThrottle() > 0 ? true : false;
        isSpiral = inputManager.GetSpecial() > 0 ? true : false;
        
    }
    
    

    #endregion State Handler
}
