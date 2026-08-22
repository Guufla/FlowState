using System;
using UnityEngine;

public class CameraTrackedObject : MonoBehaviour
{

    [SerializeField] GameObject playerObject;
    [SerializeField] CarStateMachine stateMachine;
    [Range(0f,20f)]
    [SerializeField] float positionFollow = 20f;
    [Range(0f,20f)]
    [SerializeField] float rotationFollow = 10f;
    [Range(0f,20f)]
    [SerializeField] float airRotationFollow = 10f;
    
    private float trnValue;
    
    void LateUpdate()
    {
        
        
        if(stateMachine.state == CarState.air || stateMachine.state == CarState.spiralMode)
        {
            transform.position = Vector3.Lerp(transform.position,playerObject.transform.position,Time.deltaTime * positionFollow);
            transform.rotation = Quaternion.Slerp(transform.rotation,playerObject.transform.rotation, Time.deltaTime * airRotationFollow);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position,playerObject.transform.position,Time.deltaTime * positionFollow);
            transform.rotation = Quaternion.Slerp(transform.rotation,playerObject.transform.rotation, Time.deltaTime * rotationFollow);
        }
        
        
    }
}
