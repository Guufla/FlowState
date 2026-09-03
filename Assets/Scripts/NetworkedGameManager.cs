using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkedGameManager : NetworkBehaviour
{

    private int playersFinished;
    private int curPlayers = 1;

    private readonly SyncList<float> playerPlacements = new();    
    
    private readonly SyncList<string> playerFinalRanks = new();    

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        
        for (int i = 0; i < GameManager.Instance.GetMaxPlayers(); i++)
        {
            playerPlacements.Add(0f);
            playerFinalRanks.Add("");
        }

        GameManager.Instance.SetServerInitialized();
    }
    
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
        //Debug.Log("Game Started!");

        // Hide lobby ai
        GameManager.Instance.GetUIManager().ChangeUIState(UIState.Start);
        curPlayers = GameManager.Instance.GetCurPlayers();
        GameManager.Instance.SetGameStarting(true);
        
        
        // Start Race Logic
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlacementServerRpc(int playerIndex,float placementValue,NetworkConnection sender = null)
    {
        if (playerIndex < 0 || playerIndex >= playerPlacements.Count)
            return;

        //Debug.Log(playerIndex);
        playerPlacements[playerIndex] = placementValue;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayersFinishedServerRpc(string playerName, int placement,NetworkConnection sender = null)
    {
        playerFinalRanks[playersFinished] = playerName;
        playersFinished++;
    }
    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayerServerRpc(GameObject player, Vector3 position, Quaternion rotation)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log($"Teleporting player {player.name} to position {position} with rotation {rotation}");
        player.transform.SetPositionAndRotation(position, rotation);
    }
    
    public List<string> GetFinalRanks()
    {
        return new List<string>(playerFinalRanks);
    }

    private void Update()
    {
        //Debug.Log("1st Person: " + playerPlacements[0] + " " + "2nd Person: " + playerPlacements[1]);
        
        if(playersFinished == curPlayers)
        {
            // Game ends
            GameManager.Instance.GetUIManager().ChangeUIState(UIState.Win);
            GameManager.Instance.GetUIManager().SetWinScreen();
            GameManager.Instance.ResetGameStarted();

            playersFinished = 0;
        }
    }
    
    public List<float> GetPlacementList()
    {
        return new List<float>(playerPlacements);
    }
    
    // Implement a game end loop so we go back to the lobby
    // Maybe show the racing placements before going back to the lobby
}
