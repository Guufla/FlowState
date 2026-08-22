using System;
using UnityEngine;

public class CameraTrackedObject : MonoBehaviour
{

    [SerializeField] GameObject playerObject;
    [Range(0f,20f)]
    [SerializeField] float positionFollow = 20f;
    [Range(0f,20f)]
    [SerializeField] float rotationFollow = 10f;
    
    private float trnValue;
    
    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position,playerObject.transform.position,Time.deltaTime * 20f);
        transform.rotation = Quaternion.Slerp(transform.rotation,playerObject.transform.rotation, Time.deltaTime * 10f);
        
        
    }
}
