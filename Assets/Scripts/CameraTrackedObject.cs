using System;
using UnityEngine;

public class CameraTrackedObject : MonoBehaviour
{
    [SerializeField]GameObject playerObject;
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position,playerObject.transform.position,Time.deltaTime * 20f);
        transform.rotation = Quaternion.Slerp(transform.rotation,playerObject.transform.rotation, Time.deltaTime * 20f);
        
    }
}
