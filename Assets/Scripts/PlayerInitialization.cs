using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System;


public class PlayerInitialization : NetworkBehaviour
{
    public bool isPlayer;
    
    [SerializeField] GameObject NetworkedModelPrefab;
    private GameObject NetworkedModelHandle;
    
    private Rigidbody rigidbody;
    private CarMovement carMovement;
    
    private PlayerDeath playerDeath;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if(base.IsOwner)
        {
            isPlayer = true;
            GameManager.Instance.SetPlayer(gameObject);
            Debug.Log("Player set: " + gameObject.name);
        }
        else
        {
            isPlayer = false;
            
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            
            carMovement = GetComponent<CarMovement>();
            carMovement.enabled = false;

            playerDeath = GetComponent<PlayerDeath>();
            playerDeath.enabled = false;
            
            
            NetworkedModelHandle = Instantiate(NetworkedModelPrefab);
            NetworkedModelHandle.GetComponent<NetworkedModel>().SetGameObject(gameObject);
            NetworkedModelHandle.SetActive(true);
            
        }
    }
    
    [ServerRpc]
    public void TeleportToSpawnServerRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
