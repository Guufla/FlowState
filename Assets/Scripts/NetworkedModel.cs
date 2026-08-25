using FishNet.Object;
using UnityEngine;

public class NetworkedModel : MonoBehaviour
{
    private GameObject playerObject;
    [Range(0f,20f)]
    [SerializeField] float positionFollow = 20f;
    [Range(0f,20f)]
    [SerializeField] float rotationFollow = 10f;

    // Update is called once per frame
    void Update()
    {
        if (playerObject == null) return;
        
        transform.position = Vector3.Lerp(transform.position,playerObject.transform.position,Time.deltaTime * positionFollow);
        transform.rotation = Quaternion.Slerp(transform.rotation,playerObject.transform.rotation, Time.deltaTime * rotationFollow);
    }
    
    public void SetGameObject(GameObject playerObject)
    {
        this.playerObject = playerObject;
    }
}
