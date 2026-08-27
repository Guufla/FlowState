using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    
    [Header("InputValues")]
    public float trnValue;     // Turn value
    private float trtlValue;    // Throttle value
    private float brkValue;     // Brake Value
    private float splValue;     // Special Value
    private float splTurnValue; // Special Turn Value
    
    private float trnLeftKeyboard;
    private float trnRightKeyboard;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnTurn(InputValue value)
    {
        trnValue = value.Get<float>();
        //Debug.Log("Turn Value: " + finalValue);
    }
    
    void OnThrottle(InputValue value)
    {
        trtlValue = value.Get<float>();
        //Debug.Log("Throttle Value: " + finalValue);
    }
    void OnBrake(InputValue value)
    {
        brkValue = value.Get<float>();
        //Debug.Log("Brake Value: " + finalValue);
    }
    void OnSpecial(InputValue value)
    {
        splValue = value.Get<float>();
        //Debug.Log("Special Value: " + finalValue);
    }
    
    void OnSpecialTurn(InputValue value)
    {
        splTurnValue = value.Get<float>();
        //Debug.Log("Special Turn Value: " + finalValue);
    }
    
    
    void OnTurnLeftKeyboard(InputValue value)
    {
        trnLeftKeyboard = value.Get<float>();
        trnValue = trnRightKeyboard - trnLeftKeyboard;
    }
    
    // Keyboard D key
    void OnTurnRightKeyboard(InputValue value)
    {
        trnRightKeyboard = value.Get<float>();
        trnValue = trnRightKeyboard - trnLeftKeyboard;
    }
    
    void OnBrakeKeyboard(InputValue value)
    {
        brkValue = value.Get<float>();
    }
    
    void OnThrottleKeyboard(InputValue value)
    {
        trtlValue = value.Get<float>();
    }
    
    public float GetTurn()
    {
        return trnValue;
    }
    public float GetThrottle()
    {
        return trtlValue;
    }
    public float Getbrake()
    {
        return brkValue;
    }
    public float GetSpecial()
    {
        return splValue;
    }
    public float GetSpecialTurn()
    {
        return splTurnValue;
    }
}
