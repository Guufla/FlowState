using System.Collections.Generic;
using UnityEngine;



public class CarMain : MonoBehaviour
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
    
    private Quaternion targetRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("Ground");
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        
        // This will show a ray that is meant to represent the tire comment this out when its not used
        Debug.DrawRay(rayStart.transform.position, -transform.up * tireRaySize, Color.yellow);
        
        // Does the ray intersect any objects excluding the player layer
        
        
        
        if (Physics.Raycast(rayStart.transform.position, -transform.up, out hit, tireRaySize, layerMask))
        {
            Vector3 velocity = rigidbody.linearVelocity;
            Vector3 rayDir = -transform.up;
            
            float rayDirVel = Vector3.Dot(rayDir,velocity);
            
            float x = hit.distance - tireSize;
            
            float springForce = (x * springConstant) - (rayDirVel * damperConstant);
            rigidbody.AddForce(rayDir * springForce);
            
            Vector3 groundNormal = hit.normal;

            // Project the car's current forward direction onto the ground plane
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

            // Build rotation using forward + ground normal as up
            targetRotation = Quaternion.LookRotation(forward, groundNormal);

            //transform.rotation = targetRotation;
        }else
        {
            Vector3 flatForward =Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;

            if (flatForward.sqrMagnitude > 0.001f)
            {
                targetRotation = Quaternion.LookRotation(flatForward,Vector3.up);
            }
        }
        
        transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,Time.fixedDeltaTime * 5f);

        
    }
}
