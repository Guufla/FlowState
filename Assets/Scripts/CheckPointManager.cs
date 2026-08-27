using System;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    private GameObject playerObject;
    
    [SerializeField] private GameObject curCheckPointObject;
    
    [SerializeField] private float spawnTransition;
    
    

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
    }
    
    public void SetCheckpoint(GameObject checkPoint)
    {
        curCheckPointObject = checkPoint;
    }
    
    public void GotoCheckpoint()
    {
        playerObject.transform.position = Vector3.Lerp(playerObject.transform.position,curCheckPointObject.transform.position, spawnTransition * Time.fixedDeltaTime * 10f);
        
    }
}
