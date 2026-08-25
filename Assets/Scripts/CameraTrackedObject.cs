using System;
using Unity.VisualScripting;
using UnityEngine;

public class CameraTrackedObject : MonoBehaviour
{

    [SerializeField] CarStateMachine stateMachine;
    [Range(0f,20f)]
    [SerializeField] float positionFollow = 20f;
    [Range(0f,20f)]
    [SerializeField] float rotationFollow = 10f;
    [Range(0f,20f)]
    [SerializeField] float airRotationFollow = 10f;

    private GameObject playerObject;
    
    private float trnValue;
    void Update()
    {
        if(playerObject == null  && GameManager.Instance.GetPlayer() != null)
        {
            playerObject = GameManager.Instance.GetPlayer();
        }
    }
    
    void LateUpdate()
    {
        if(playerObject == null) return;
        
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
