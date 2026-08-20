using UnityEngine;

public class CarVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InputManager inputManager;
    [SerializeField] CarMovement movementScript;
    [SerializeField] GameObject TireFL;
    [SerializeField] GameObject TireFR;
    [SerializeField] GameObject TireRL;
    [SerializeField] GameObject TireRR;
    
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
        CarModelRotation();
        CarFrontTireRotation();
        
    }
    
    void CarModelRotation()
    {
        maxTrnValue = movementScript.GetMaxTurn();
        curTrnValue = movementScript.GetCurrentTurn();
        //Debug.Log("Max Value:" + maxTrnValue + "Current Value: "+ curTrnValue);
        
        curTrnPercent = Mathf.InverseLerp(-maxTrnValue,maxTrnValue,curTrnValue);
        
        targetRotation = Mathf.Lerp(-maxTrnAngle,maxTrnAngle , curTrnPercent);
        transform.localEulerAngles  = new Vector3(0f,-90f + targetRotation, 0f);
    }
    
    void CarFrontTireRotation()
    {
        float trnValue = inputManager.GetTurn();
        
        float adjustedTrnValue = Mathf.InverseLerp(-1,1,trnValue);
        float frontTireRotation = Mathf.Lerp(-40f,40f,adjustedTrnValue);
        
        TireFL.transform.localEulerAngles = new Vector3(TireFL.transform.localEulerAngles.x,frontTireRotation,TireFL.transform.localEulerAngles.z);
        TireFR.transform.localEulerAngles = new Vector3(TireFR.transform.localEulerAngles.x,frontTireRotation,TireFR.transform.localEulerAngles.z);
    }
}
