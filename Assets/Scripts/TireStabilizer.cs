using UnityEngine;

public class TireStabilizer : MonoBehaviour
{
    [Header("Tire Stabilizer References")]
    [SerializeField] private GameObject playerObject;
    
    [Header("Tire Stabilizer Settings")]
    [SerializeField] private float tireSize = 3f;
    [Range(0f, 1000f)]
    [SerializeField] private float springConstant = 100f;
    [Range(0f, 1000f)]
    [SerializeField] private float damperConstant = 10f;

    private Rigidbody playerRigidbody;
    
    private LayerMask layerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRigidbody = playerObject.GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        
        // This will show a ray that is meant to represent the tire comment this out when its not used
        Debug.DrawRay(transform.position, transform.TransformDirection(-transform.up) * tireSize, Color.yellow);
        
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(-transform.up), out hit, tireSize, layerMask))
        {
            Vector3 velocity = playerRigidbody.linearVelocity;
            Vector3 rayDir = transform.TransformDirection(-transform.up);
            
            float rayDirVel = Vector3.Dot(rayDir, velocity);
            
            float x = hit.distance - tireSize;
            
            float springForce = (x * springConstant) - (rayDirVel * damperConstant);
            
            playerRigidbody.AddForceAtPosition(rayDir * springForce, transform.position);
        }
        

        

        
    }
}

