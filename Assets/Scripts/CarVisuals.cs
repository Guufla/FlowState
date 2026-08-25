using UnityEngine;

public class CarVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InputManager inputManager;
    [SerializeField] GameObject tireFL;
    [SerializeField] GameObject tireFR;
    [SerializeField] GameObject tireRL;
    [SerializeField] GameObject tireRR;
    
    private GameObject playerObject;
    private CarMovement movementScript;
    
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
        if(playerObject == null && GameManager.Instance.GetPlayer() != null)
        {
            playerObject = GameManager.Instance.GetPlayer();
            movementScript = playerObject.GetComponent<CarMovement>();
        }

        if(playerObject == null) return;
        
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
        
        tireFL.transform.localEulerAngles = new Vector3(tireFL.transform.localEulerAngles.x,frontTireRotation,tireFL.transform.localEulerAngles.z);
        tireFR.transform.localEulerAngles = new Vector3(tireFR.transform.localEulerAngles.x,frontTireRotation,tireFR.transform.localEulerAngles.z);
    }
}
