using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using System;
using Unity.Services.Lobbies.Models;
using UnityEngine.UIElements;
using System.Collections.Generic;
using FishNet;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using FishNet.Managing;
using FishNet.Transporting.UTP;
using Unity.Networking.Transport.Relay;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private NetworkedGameManager networkedGameManager;
    [SerializeField] private PanelRenderer lobbyMenuPanel;
    [SerializeField] private PanelRenderer lobbyJoinPanel;
    [SerializeField] private PanelRenderer UserNamePanel;
    
    private Lobby hostLobby; // When they are the host
    private Lobby currentLobby; // When they are a member
    
    
    private float heartbeatTimer;
    
    private TextField userName;
    private TextField lobbyName;
    private TextField joinCode;
    
    private Button setUserName;
    private Button startLobby;
    private Button joinLobby;
    private Button startGame;
    
    private Label lobbyNameLabel;
    private Label lobbyCodeLabel;
    private Label lobbyPlayersLabel;
    
    public bool CheckLobbies;
    
    private float lobbyUpdateTimer;
    
    private string relayCode;
    private string userNameStr;
    
    private LobbyState lobbyState;
    
    private void OnEnable()
    {
        UserNamePanel.RegisterUIReloadCallback(OnUIReload);
        lobbyJoinPanel.RegisterUIReloadCallback(OnUIReload);
        lobbyMenuPanel.RegisterUIReloadCallback(OnUIReload);
    }


    private void OnDisable()
    {
        UserNamePanel.UnregisterUIReloadCallback(OnUIReload);
        lobbyJoinPanel.UnregisterUIReloadCallback(OnUIReload);
        lobbyMenuPanel.UnregisterUIReloadCallback(OnUIReload);
    }


    private void OnUIReload(PanelRenderer renderer, VisualElement root)
    {
        if(renderer == UserNamePanel)
        {
            userName = root.Q<TextField>("UserName");
            setUserName = root.Q<Button>("UserNameButton");
            setUserName.clicked += SetUserName;
        }
        
        else if(renderer == lobbyJoinPanel)
        {
            lobbyName = root.Q<TextField>("LobbyName");
            joinCode = root.Q<TextField>("JoinCode");

            startLobby = root.Q<Button>("LobbyStartButton");
            joinLobby = root.Q<Button>("LobbyJoinButton");
            
            startLobby.clicked += CreateLobby;
            joinLobby.clicked += JoinLobbyHelper;
        }
        else if(renderer == lobbyMenuPanel)
        {
            
            lobbyNameLabel = root.Q<Label>("LobbyName");
            lobbyCodeLabel = root.Q<Label>("LobbyCode");
            lobbyPlayersLabel = root.Q<Label>("LobbyPlayerCount");
            
            startGame = root.Q<Button>("StartGame");
            
            startGame.clicked += StartGame;
        }
    }
    
    private void ChangeLobbyState(LobbyState newState)
    {
        lobbyState = newState;

        UserNamePanel.enabled =
            newState == LobbyState.UserName;

        lobbyJoinPanel.enabled =
            newState == LobbyState.Join;

        lobbyMenuPanel.enabled =
            newState == LobbyState.Lobby;
    }
        
    private void SetUserName()
    {
        userNameStr = userName.value;
        ChangeLobbyState(LobbyState.Join);
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        UserNamePanel.enabled = true;
        
        ChangeLobbyState(LobbyState.UserName);
    }
    
    private async void CreateLobby()
    {
        try
        {
            relayCode = await StartHostWithRelay();

            Debug.Log("Relay created: " + relayCode);
            
            
            String finalName = "lobbyName";
            
            if(!string.IsNullOrWhiteSpace(lobbyName.value))
            {
                finalName = lobbyName.value;
            }
            
            int maxPlayers = 4;
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
            
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(finalName,maxPlayers,createLobbyOptions);
            
            
            hostLobby = lobby;
            currentLobby = lobby;

            
            lobbyNameLabel.text = "Lobby Name: " + hostLobby.Name;
            lobbyCodeLabel.text = "Lobby Code: " + hostLobby.LobbyCode;
            
            ChangeLobbyState(LobbyState.Lobby);
            
            Debug.Log("Created Lobby!! + " + lobby.Name + " " + lobby.MaxPlayers);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private void JoinLobbyHelper()
    {
        if(!string.IsNullOrWhiteSpace(joinCode.value))
        {
            
        
            JoinLobbyByCode(joinCode.value);
        }
        else
        {
            Debug.Log("No Code entered");
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
            
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode , joinLobbyByCodeOptions);
            
            relayCode = lobby.Data["RelayJoinCode"].Value;
            
            lobbyNameLabel.text = "Lobby Name: " + lobby.Name;
            lobbyCodeLabel.text = "Lobby Code: " + lobby.LobbyCode;
            
            currentLobby = lobby;
            
            Debug.Log("Joined Lobby with code" + lobbyCode);
            
            relayCode = lobby.Data["RelayJoinCode"].Value;

            Debug.Log("Relay code received from lobby: " + relayCode);

            bool connected = await StartClientWithRelay(relayCode);
            ChangeLobbyState(LobbyState.Lobby);
            
            Debug.Log("FishNet client started: " + connected);
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
        
        
        String stringList = "";
        float playerCount = 0;
        if (currentLobby != null)
        {
            foreach (Player player in currentLobby.Players)
            {
                playerCount++;
                stringList = stringList + " " + player.Data["PlayerName"].Value;
            }
            lobbyPlayersLabel.text = "Lobby Players " + "(" + playerCount + ")" + stringList;
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
        }
    }
    
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string,PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, userName.value)}
            }
        };
    }
    
    private async void HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -=Time.deltaTime;
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
        if(hostLobby == null) return;
        
        lobbyMenuPanel.enabled = false;
        
        networkedGameManager.StartGameForEveryone();
        
        // PUT GAME STARTED LOGIC HERE
    }
    
    
    // -------- RELAY SETUP ------------
    
    private async Task<string> StartHostWithRelay(int maxConnections = 12)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        UnityTransport transport =InstanceFinder.NetworkManager.TransportManager.GetTransport<UnityTransport>();
    
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        
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
        if (string.IsNullOrWhiteSpace(joinCode)) return false;
        
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        UnityTransport transport =InstanceFinder.NetworkManager.TransportManager.GetTransport<UnityTransport>();
        
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));
        
        return InstanceFinder.ClientManager.StartConnection();
        
    }
    
}
