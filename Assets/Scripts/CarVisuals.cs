using UnityEngine;

public class CarVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CarMovement movementScript;
    
    [Header("Variables")]
    [SerializeField] float maxTrnAngle;
    
    private float maxTrnValue;
    private float curTrnValue;
    
    private float curTrnPercent;
    private float curTrnAngle;
    
    private float targetRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        maxTrnValue = movementScript.GetMaxTurn();
        curTrnValue = movementScript.GetCurrentTurn();
        //Debug.Log("Max Value:" + maxTrnValue + "Current Value: "+ curTrnValue);
        
        curTrnPercent = Mathf.InverseLerp(-maxTrnValue,maxTrnValue,curTrnValue);
        
        targetRotation = Mathf.Lerp(-maxTrnAngle,maxTrnAngle , curTrnPercent);
        transform.localEulerAngles  = new Vector3(0f,-90f + targetRotation, 0f);
        
    }
}
