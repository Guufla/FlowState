using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using System;
using Unity.Services.Lobbies.Models;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    
    private Lobby hostLobby; // When they are the host
    private Lobby currentLobby; // When they are a member
    
    
    private float heartbeatTimer;
    
    private TextField userName;
    private TextField lobbyName;
    private TextField joinCode;
    
    private Button startLobby;
    private Button joinLobby;
    
    private Label lobbyNameLabel;
    private Label lobbyCodeLabel;
    private Label lobbyPlayersLabel;
    
    public bool CheckLobbies;
    
    private float lobbyUpdateTimer;
    
    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }


    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }


    private void OnUIReload(PanelRenderer renderer, VisualElement root)
    {
        userName = root.Q<TextField>("UserName");
        lobbyName = root.Q<TextField>("LobbyName");
        joinCode = root.Q<TextField>("JoinCode");

        startLobby = root.Q<Button>("LobbyStartButton");
        joinLobby = root.Q<Button>("LobbyJoinButton");
        
        lobbyNameLabel = root.Q<Label>("LobbyName");
        lobbyCodeLabel = root.Q<Label>("LobbyCode");
        lobbyPlayersLabel = root.Q<Label>("LobbyPlayerCount");

        startLobby.clicked += CreateLobby;
        joinLobby.clicked += JoinLobbyHelper;
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
            String finalName = "lobbyName";
            
            if(!string.IsNullOrWhiteSpace(lobbyName.value))
            {
                finalName = lobbyName.value;
            }
            
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions 
            {
                IsPrivate = false,
                Player = GetPlayer()
            };
            
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(finalName,maxPlayers,createLobbyOptions);
            
            hostLobby = lobby;
            currentLobby = lobby;
            
            lobbyNameLabel.text = "Lobby Name: " + hostLobby.Name;
            lobbyCodeLabel.text = "Lobby Code: " + hostLobby.LobbyCode;
            
            
            
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
            
            lobbyNameLabel.text = "Lobby Name: " + lobby.Name;
            lobbyCodeLabel.text = "Lobby Code: " + lobby.LobbyCode;
            
            currentLobby = lobby;
            
            Debug.Log("Joined Lobby with code" + lobbyCode);
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
    
}
