using System;
using FishNet.Object;
using UnityEngine;


public class NetworkedGameManager : NetworkBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    
    public void StartGameForEveryone()
    {
        if (!IsServerInitialized)
            return;

        StartGameObserversRpc();
    }

    // Update is called once per frame
    
    [ObserversRpc]
    private void StartGameObserversRpc()
    {
        Debug.Log("Game Started!");
        
        // Hide lobby ai
        mainMenuUI.SetActive(false);
        
        // Start Race Logic
    }
    
    
    // Implement a game end loop so we go back to the lobby
    // Maybe show the racing placements before going back to the lobby
}
