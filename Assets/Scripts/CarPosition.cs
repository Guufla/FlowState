using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

// OLD SCRIPT DO NOT USE THIS!!!!

[System.Serializable]
public class tireInfo
{
    public GameObject tireObject;

    public Vector3 targetPosition;
    public Vector3 targetRotation;
    
    public Vector3 curPosition;
    public Vector3 curRotation;
    
    public float bufferTime;
    public bool isOnGround;
};


public enum tireSelect { RightFront, RightBack, LeftFront, LeftBack }

public class CarPosition : MonoBehaviour
{
    [Header("Tire Stabilizer References")]
    
    [SerializeField] private GameObject tireRF; // Right Front
    [SerializeField] private GameObject tireRB; // Right Back
    [SerializeField] private GameObject tireLF; // Left Front
    [SerializeField] private GameObject tireLB; // Left Back
    
    public List<tireInfo> tireInfos = new List<tireInfo>(); // RF, RB, LF, LB

    [Header("Tire Stabilizer Settings")]
    [SerializeField] private float tireRaySize = 3f;
    [Range(0f, 1000f)]
    [SerializeField] private float springConstant = 100f;
    [Range(0f, 1000f)]
    [SerializeField] private float damperConstant = 10f;
    
    [SerializeField] private float totalBufferTime = 1f;
    
    [SerializeField] private float curBufferTime = 0f;
    
    [Range(0f,1f)]
    [SerializeField] private float tireSizeOffset = 0.05f;
    
    private LayerMask layerMask;
    
    private Rigidbody playerRigidbody;
    
    private bool isGrounded = false;
    private float carToTireYOffset = 0f;
    
    
    private Quaternion targetRotation;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        layerMask = LayerMask.GetMask("Ground");
        playerRigidbody = GetComponent<Rigidbody>();
        tireInfos.Add(new tireInfo() { tireObject = tireRF });
        tireInfos.Add(new tireInfo() { tireObject = tireRB });
        tireInfos.Add(new tireInfo() { tireObject = tireLF });
        tireInfos.Add(new tireInfo() { tireObject = tireLB });
        
        float startingAverageTireY = ( tireRF.transform.position.y +
                                       tireRB.transform.position.y +
                                       tireLF.transform.position.y +
                                       tireLB.transform.position.y) / 4f;

        carToTireYOffset = transform.position.y - startingAverageTireY;
        
        
        curBufferTime = totalBufferTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for(int i = 0; i < tireInfos.Count; i++)
        {
            SetTirePositions(i);
        }
        
        if(tireInfos[0].isOnGround && tireInfos[1].isOnGround && tireInfos[2].isOnGround && tireInfos[3].isOnGround)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        
        
        if(isGrounded)
        {
            playerRigidbody.useGravity = false;
            
            if(curBufferTime < totalBufferTime)
            {
                curBufferTime += Time.fixedDeltaTime;
            }
            else
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                Vector3 averagePosition = (tireInfos[(int)tireSelect.RightFront].curPosition + 
                                            tireInfos[(int)tireSelect.RightBack].curPosition + 
                                            tireInfos[(int)tireSelect.LeftFront].curPosition + 
                                            tireInfos[(int)tireSelect.LeftBack].curPosition) / 4f;
                transform.position = new Vector3(transform.position.x, averagePosition.y + carToTireYOffset, transform.position.z);
            }
        }
        else
        {
            playerRigidbody.useGravity = true;
        }
        
        
        SetCarRotation();
    }
    
    void SetTirePositions(int tireIndex)
    {
        RaycastHit hit;
        Vector3 curPosition = tireInfos[tireIndex].tireObject.transform.position;
        
        // This will show a ray that is meant to represent the tire comment this out when its not used
        Debug.DrawRay(curPosition, -tireInfos[tireIndex].tireObject.transform.up * tireRaySize, Color.yellow);
        
        // Does the ray intersect any objects excluding the player layer
        
        
        if (Physics.Raycast(curPosition, -tireInfos[tireIndex].tireObject.transform.up, out hit, tireRaySize, layerMask))
        {
            
            if(tireInfos[tireIndex].isOnGround == false)
            {
                tireInfos[tireIndex].isOnGround = true;
                curBufferTime = 0f;
            }
            
            if(curBufferTime >= totalBufferTime)
            {

                Vector3 surfaceUpVector = hit.normal;
                tireInfos[tireIndex].targetPosition = hit.point + (surfaceUpVector * (tireRaySize - tireSizeOffset));
                tireInfos[tireIndex].curPosition = hit.point + (surfaceUpVector * (tireRaySize-tireSizeOffset));
                //tireInfos[tireIndex].curPosition = Vector3.Lerp(tireInfos[tireIndex].curPosition, tireInfos[tireIndex].targetPosition, Time.fixedDeltaTime * 2f);
                
                
                Debug.DrawRay(hit.point, surfaceUpVector * tireRaySize, Color.red);
            }
            else
            {
                tireInfos[tireIndex].curPosition = curPosition;
            }
        
        }
        else
        {
            tireInfos[tireIndex].isOnGround = false;
        }
        
        
        if (!isGrounded)
        {
            tireInfos[tireIndex].curPosition = curPosition;
            tireInfos[tireIndex].bufferTime = 0f;
        }
        
        
    }
    
    
    
    void SetCarRotation()
    {
        Vector3 right   =   tireInfos[(int)tireSelect.LeftFront].curPosition - 
                            tireInfos[(int)tireSelect.RightFront].curPosition +
                            tireInfos[(int)tireSelect.LeftBack].curPosition -
                            tireInfos[(int)tireSelect.RightBack].curPosition;
                        
        Vector3 forward =   tireInfos[(int)tireSelect.LeftFront].curPosition - 
                            tireInfos[(int)tireSelect.LeftBack].curPosition +
                            tireInfos[(int)tireSelect.RightFront].curPosition -
                            tireInfos[(int)tireSelect.RightBack].curPosition;
                            
        right.Normalize();
        forward.Normalize();
                        
        Vector3 up = Vector3.Cross(right,forward).normalized;
        
        if (Vector3.Dot(up, Vector3.up) < 0f)
        {
            up = -up;
        }
        
        targetRotation = Quaternion.LookRotation(forward,up);
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.fixedDeltaTime * 5f
        );
    }
}
