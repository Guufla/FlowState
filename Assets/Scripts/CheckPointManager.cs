using System;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    private GameObject playerObject;
    
    [SerializeField] private GameObject curCheckPointObject;
    
    [SerializeField] private float spawnTransition;
    
    
    private CarMovement carMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(playerObject == null)
        {
            playerObject = GameManager.Instance.GetPlayer();
        }
        else
        {
            carMovement = playerObject.GetComponent<CarMovement>();
        }
    }
    
    public void SetCheckpoint(GameObject checkPoint)
    {
        curCheckPointObject = checkPoint;
    }
    
    public void GotoCheckpoint()
    {
        playerObject.transform.position = Vector3.Lerp(playerObject.transform.position,curCheckPointObject.transform.position, spawnTransition * Time.fixedDeltaTime * 10f);
        playerObject.transform.rotation = Quaternion.Slerp(playerObject.transform.rotation,curCheckPointObject.transform.rotation, spawnTransition * Time.fixedDeltaTime * 10f);
        carMovement.SetTargetInitialRotation(curCheckPointObject.transform.rotation);
    }
    
    public bool isAtCheckpoint()
    {
        if(Vector3.Distance(playerObject.transform.position,curCheckPointObject.transform.position) < 2f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
