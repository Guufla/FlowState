using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    private GameObject playerObject;
    
    [SerializeField] private GameObject curCheckPointObject;
    [SerializeField] private float spawnTransition;
    [SerializeField] private List<GameObject> checkPoints;
    
    public int curCheckPointIndex;

    private float curDistToLastPoint;

    private float curPlacementValue;

    public float finalPlacementValue;
    
    private CarMovement carMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curPlacementValue = 0;
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
            curDistToLastPoint = Vector3.Distance(playerObject.transform.position, checkPoints[curCheckPointIndex].transform.position);
            finalPlacementValue = curDistToLastPoint + curPlacementValue;
        }
    }
    
    public void SetCheckpoint(GameObject checkPoint)
    {
        if(curCheckPointIndex + 1 >= checkPoints.Count)
        {
            GameManager.Instance.PlayerFinished();
        }
        else if(checkPoint == checkPoints[curCheckPointIndex + 1])
        {
            curCheckPointIndex++;
            curCheckPointObject = checkPoint;
            curPlacementValue += 1000;
        }
    }
    
    public void GotoCheckpoint()
    {
        playerObject.transform.position = Vector3.Lerp(playerObject.transform.position,curCheckPointObject.transform.position, spawnTransition * Time.fixedDeltaTime * 10f);
        playerObject.transform.rotation = Quaternion.Slerp(playerObject.transform.rotation,curCheckPointObject.transform.rotation, spawnTransition * Time.fixedDeltaTime * 10f);
        carMovement.SetTargetInitialRotation(curCheckPointObject.transform.rotation);
    }
    
    public bool IsAtCheckpoint()
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
    
    public float GetPlacementWeight()
    {
        return finalPlacementValue;
    }
    public void ResetCheckpoints()
    {
        curCheckPointIndex = 0;
    }
}
