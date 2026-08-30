using FishNet.Demo.AdditiveScenes;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] CheckPointManager checkPointManager;
    private GameObject playerObject;
    
    
    void Update()
    {
        if(playerObject == null)
        {
            playerObject = GameManager.Instance.GetPlayer();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(playerObject != null && other.gameObject == playerObject)
        {
            checkPointManager.SetCheckpoint(gameObject);
        }
    }
    
}
