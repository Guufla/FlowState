using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using System;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using FishNet;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using FishNet.Transporting.UTP;
using Unity.Networking.Transport.Relay;

public class LobbyManager : MonoBehaviour
{
    private NetworkedGameManager networkedGameManager;

    private UIManager uiManager;
    
    private Lobby hostLobby; // When they are the host
    private Lobby currentLobby; // When they are a member
    
    private float heartbeatTimer;
    
    public bool CheckLobbies;
    
    private float lobbyUpdateTimer;
    
    private string relayCode;
    private string userNameStr;

    private int playerIndex;

    private int maxPlayers = 12;
    
    private int previousPlayerCount;
    private int playerCount;

    [SerializeField] float joinBuffer = 0f;
    private float curJoinBuffer;
    

    private void OnEnable()
    {
        networkedGameManager = GameManager.Instance.GetNetworkedGameManager();
        uiManager = GameManager.Instance.GetUIManager();

        GameManager.Instance.SetMaxPlayers(maxPlayers);

        uiManager.OnSetUserName += SetUserName;
        uiManager.OnCreateLobby += CreateLobby;
        uiManager.OnJoinLobby += JoinLobbyHelper;
        uiManager.OnStartGame += StartGame;
    }


    private void OnDisable()
    {
        if(uiManager == null)
            return;

        uiManager.OnSetUserName -= SetUserName;
        uiManager.OnCreateLobby -= CreateLobby;
        uiManager.OnJoinLobby -= JoinLobbyHelper;
        uiManager.OnStartGame -= StartGame;
    }

        
    private void SetUserName()
    {
        userNameStr = uiManager.GetUserName();
        
        GameManager.Instance.SetPlayerName(userNameStr);
        
        uiManager.ChangeUIState(UIState.Join);
    }


    private async void Start()
    {
        await UnityServices.InitializeAsync();
        
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
        };
        
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    

    private async void CreateLobby()
    {
        try
        {
            relayCode = await StartHostWithRelay();

            //Debug.Log("Relay created: " + relayCode);
            
            String finalName = "lobbyName";

            if(!string.IsNullOrWhiteSpace(uiManager.GetLobbyName()))
            {
                finalName = uiManager.GetLobbyName();
            }
            
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions 
            {
                IsPrivate = false,
                Player = GetPlayer(),
                
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "RelayJoinCode",
                        new DataObject(
                            DataObject.VisibilityOptions.Member,
                            relayCode
                        )
                    }
                }
            };
            
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                finalName,
                maxPlayers,
                createLobbyOptions
            );
            
            hostLobby = lobby;
            currentLobby = lobby;

            playerIndex = 0;
            
            GameManager.Instance.SetPlayerIndex(playerIndex);
            
            uiManager.UpdateLobbyUI(
                currentLobby.Name,
                currentLobby.LobbyCode
            );
            
            uiManager.ChangeUIState(UIState.Lobby);
            
            //Debug.Log("Created Lobby!! + " + lobby.Name + " " + lobby.MaxPlayers);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    

    private void JoinLobbyHelper()
    {
        string code = uiManager.GetJoinCode();
        
        if(!string.IsNullOrWhiteSpace(code))
        {
            JoinLobbyByCode(code);
        }
        else
        {
            //Debug.Log("No Code entered");
        }
    }
    

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(
                lobbyCode,
                joinLobbyByCodeOptions
            );

            currentLobby = lobby;
            
            relayCode = lobby.Data["RelayJoinCode"].Value;
            
            uiManager.UpdateLobbyUI(
                currentLobby.Name,
                currentLobby.LobbyCode
            );
            
            //Debug.Log("Joined Lobby with code" + lobbyCode);

            //Debug.Log("Relay code received from lobby: " + relayCode);

            bool connected = await StartClientWithRelay(relayCode);
            
            uiManager.ChangeUIState(UIState.Lobby);
            
            GameManager.Instance.SetServerInitialized();
            
            //Debug.Log("FishNet client started: " + connected);
            
            int playerCount = 0;
            
            if (currentLobby != null)
            {
                foreach (Player player in currentLobby.Players)
                {
                    playerCount++;
                }
                
                playerIndex = playerCount - 1;
            }

            GameManager.Instance.SetPlayerIndex(playerIndex);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    

    private async void ListLobbys()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
        
            Debug.Log("Lobbies Found: " + " " + queryResponse.Results.Count);
            
            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyUpdate();
        
        if(CheckLobbies)
        {
            ListLobbys();
            CheckLobbies = false;
        }
        
        UpdateLobbyPlayers();
    }
    private void FixedUpdate()
    {
        JoinBufferUpdate();
    }


    private void UpdateLobbyPlayers()
    {
        if(currentLobby == null)
            return;

        
        String stringList = "";
        int newplayerCount = 0;

        foreach (Player player in currentLobby.Players)
        {
            newplayerCount++;
            
            stringList = stringList + " " + player.Data["PlayerName"].Value;
        }

        previousPlayerCount = playerCount;
        playerCount = newplayerCount;
        if(playerCount > previousPlayerCount)
        {
            PlayerJoined();
        }
        
        GameManager.Instance.SetCurPlayers(playerCount);
        
        uiManager.UpdateLobbyPlayersUI(
            playerCount,
            stringList
        );
    }
    
    private void PlayerJoined()
    {
        curJoinBuffer = joinBuffer;
    }
    private void JoinBufferUpdate()
    {
        if(curJoinBuffer > 0)
        {
            curJoinBuffer -= Time.fixedDeltaTime * 1f;
            uiManager.StartGameBuffer(true);
        }
        else
        {
            uiManager.StartGameBuffer(false);
        }
        
        
    }

    private async void HandleLobbyUpdate()
    {
        if(currentLobby == null)
            return;

        lobbyUpdateTimer -= Time.deltaTime;

        if(lobbyUpdateTimer <= 0f)
        {
            lobbyUpdateTimer = 2f;

            currentLobby =
                await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

            // If we're the host, keep hostLobby updated too.
            if(hostLobby != null)
            {
                hostLobby = currentLobby;
            }
            
            uiManager.UpdateLobbyUI(
                currentLobby.Name,
                currentLobby.LobbyCode
            );
        }
    }
    

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string,PlayerDataObject>
            {
                {
                    "PlayerName",
                    new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Member,
                        userNameStr
                    )
                }
            }
        };
    }
    

    private async void HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            
            if(heartbeatTimer <= 0f)
            {
                float heartbeatTimerMax = 15f;
                
                heartbeatTimer = heartbeatTimerMax;
                
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    
    
    private void StartGame()
    {
        if(hostLobby == null || curJoinBuffer > 0) return;
        
        uiManager.ChangeUIState(UIState.Start);
        
        networkedGameManager.StartGameForEveryone();
        
        
        // PUT GAME STARTED LOGIC HERE
    }
    
    
    // -------- RELAY SETUP ------------
    
    private async Task<string> StartHostWithRelay(int maxConnections = 12)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        
        UnityTransport transport =
            InstanceFinder.NetworkManager.TransportManager.GetTransport<UnityTransport>();
    
        transport.SetRelayServerData(
            AllocationUtils.ToRelayServerData(allocation, "dtls")
        );
        
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        
        if (InstanceFinder.ServerManager.StartConnection())
        {
            InstanceFinder.ClientManager.StartConnection();
            
            return joinCode;
        }
        
        return null;
    }
    

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        if (string.IsNullOrWhiteSpace(joinCode))
            return false;
        
        JoinAllocation joinAllocation =
            await RelayService.Instance.JoinAllocationAsync(joinCode);
        
        UnityTransport transport =
            InstanceFinder.NetworkManager.TransportManager.GetTransport<UnityTransport>();
        
        transport.SetRelayServerData(
            AllocationUtils.ToRelayServerData(joinAllocation, "dtls")
        );
        
        return InstanceFinder.ClientManager.StartConnection();
    }
}