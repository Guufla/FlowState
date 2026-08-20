using System;
using UnityEngine;

public class CameraTrackedObject : MonoBehaviour
{

    [SerializeField] GameObject playerObject;
    
    private float trnValue;
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position,playerObject.transform.position,Time.deltaTime * 5f);
        transform.rotation = Quaternion.Slerp(transform.rotation,playerObject.transform.rotation, Time.deltaTime * 5f);
        
        
    }
}
