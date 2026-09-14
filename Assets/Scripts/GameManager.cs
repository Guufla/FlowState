using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Splines;
using System.Collections.Generic;
using UnityEngine.UIElements;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    #region References

    [Header("Core References")]
    [SerializeField] protected CarStateMachine carStateMachine;
    [SerializeField] protected InputManager inputManager;
    [SerializeField] protected CheckPointManager checkPointManager;
    [SerializeField] protected NetworkedGameManager networkedGameManager;
    [SerializeField] protected UIManager uiManager;
    [SerializeField] protected LobbyManager lobbyManager;

    [Header("World References")]
    [SerializeField] protected SplineContainer splines;
    [SerializeField] protected GameObject carModel;
    [SerializeField] protected List<GameObject> spawnPoints;

    #endregion

    #region UI Panels

    [Header("UI Panels")]
    [SerializeField] protected PanelRenderer lobbyMenuPanel;
    [SerializeField] protected PanelRenderer lobbyJoinPanel;
    [SerializeField] protected PanelRenderer usernamePanel;
    [SerializeField] protected PanelRenderer placementPanel;
    [SerializeField] protected PanelRenderer winPanel;
    [SerializeField] protected PanelRenderer gameCountdownPanel;

    #endregion

    #region Player

    [Header("Player")]
    protected GameObject playerObject;
    protected CarMovement playerMovementScript;

    protected string playerName;
    protected int playerIndex;
    protected int playerPlacement;

    protected float playerPositionWeight;
    protected float curPlacementWeight;

    #endregion

    #region Player Management

    [Header("Player Management")]
    protected int maxPlayers;
    protected int curPlayers;

    protected List<float> playerPlacementWeights;

    #endregion

    #region Game State

    [Header("Game State")]
    protected bool isHost;
    protected bool serverInitialized;
    protected bool isGameStarted;
    protected bool isGameStarting;

    [SerializeField] protected float gameCountdown;

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    
       // Update is called once per frame
    
    protected virtual void Update()
    {
        if(!isGameStarted)
        {
            checkPointManager.ResetCheckpoints();
        }
        else
        {
            networkedGameManager.UpdatePlacementServerRpc(playerIndex,playerPositionWeight);
            playerPositionWeight = checkPointManager.GetPlacementWeight();
            PlayerPlacement();
        }
    
        
    }

    protected virtual void  FixedUpdate()
    {
        if(isGameStarting)
        {
            GameCountdown();
        }
    }

    protected virtual void Start()
    {
        networkedGameManager.gameObject.SetActive(true);
        lobbyManager.gameObject.SetActive(true);
        
    }
    
    
    #region Player Placement
    
    private void PlayerPlacement()
    {
        playerPlacementWeights = networkedGameManager.GetPlacementList();

        curPlacementWeight = playerPlacementWeights[playerIndex];

        playerPlacement = 1;
        
        for(int i = 0; i < curPlayers; i++)
        {
            if (curPlacementWeight < playerPlacementWeights[i])
            {
                playerPlacement++;
            }
        }
    }
    
    public int GetPlayerPlacement()
    {
        return playerPlacement;
    }
    
    public void PlayerFinished()
    {
        networkedGameManager.UpdatePlayersFinishedServerRpc(playerName, playerPlacement);
    }
    
    #endregion
    
    
    #region Player
    
    public void SetPlayer(GameObject playerObject)
    {
        this.playerObject = playerObject;
        playerMovementScript = playerObject.GetComponent<CarMovement>();
    }
    
    public GameObject GetPlayer()
    {
        if(playerObject != null)
        {
            return playerObject;
        }
        else
        {
            return null;
        }
    }
    
    public void SetPlayerIndex(int playerIndex)
    {
        this.playerIndex = playerIndex;
        if (playerIndex == 0)
        {
            isHost = true;
        }
    }
    
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }
    
    public string GetPlayerName()
    {
        return playerName;
    }
    
    #endregion
    
    
    #region Player Count
    
    public void SetMaxPlayers(int maxPlayers)
    {
        this.maxPlayers = maxPlayers;
    }
    
    public int GetMaxPlayers()
    {
        return maxPlayers;
    }
    
    public void SetCurPlayers(int curPlayers)
    {
        this.curPlayers = curPlayers;
    }
    
    public int GetCurPlayers()
    {
        return curPlayers;
    }
    
    #endregion
    
    
    #region Server
    
    public void SetServerInitialized()
    {
        serverInitialized = true;
    }
    
    public bool GetServerInitialized()
    {
        return serverInitialized;
    }
    
    #endregion
    
    
    #region Game State
    
    public void SetGameStarting(bool isGameStarting)
    {
        if(isGameStarting)
        {
            Transform spawn = spawnPoints[playerIndex].transform;

            Debug.Log(
                $"STARTING PLAYER: {playerName} | " +
                $"Index: {playerIndex} | Spawn: {spawn.name}"
            );

            uiManager.EnableCountdown(true);

            carStateMachine.isInitialize = true;
            
            playerObject.GetComponent<Rigidbody>().useGravity = false;
            playerObject.GetComponent<Rigidbody>().linearVelocity = new Vector3(0,0,0);
            playerObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(0,0,0);
            playerObject.GetComponent<Rigidbody>().position = spawn.position;
            playerObject.GetComponent<Rigidbody>().rotation = spawn.rotation;
            playerObject.GetComponent<CarMovement>().enabled = false;

            //networkedGameManager.TeleportPlayerServerRpc(playerObject,spawn.position,spawn.rotation);
            Debug.Log($"Player {playerName} spawned at {spawn.position} with rotation {spawn.rotation}");

            playerMovementScript.SetTargetInitialRotation(spawn.rotation);
        }

        gameCountdown = 5f;
        this.isGameStarting = isGameStarting;
    }
    

    public void ResetGameStarted()
    {
        isGameStarted = false;
    }
    
    public void GameCountdown()
    {
        gameCountdown -= Time.fixedDeltaTime;

        uiManager.UpdateCountDown(gameCountdown);
        
        if(gameCountdown <= 0.1f)
        {
            carStateMachine.isInitialize = false;
            playerObject.GetComponent<Rigidbody>().useGravity = true;
            playerObject.GetComponent<CarMovement>().enabled = true;
            
            isGameStarted = true;
            isGameStarting = false;
            uiManager.EnableCountdown(false);
        }
    }
    
    
    public bool GetGameStart()
    {
        return isGameStarted;
    }
    
    #endregion
    
    
    #region Core References
    
    public CarStateMachine GetStateMachine()
    {
        return carStateMachine;
    }
    
    public InputManager GetInputManager()
    {
        return inputManager;
    }
    
    public SplineContainer GetSplineContainer()
    {
        return splines;
    }
    
    public CheckPointManager GetCheckPointManager()
    {
        return checkPointManager;
    }
    
    public GameObject GetCarModel()
    {
        return carModel;
    }
    
    public UIManager GetUIManager()
    {
        return uiManager;
    }
    
    public NetworkedGameManager GetNetworkedGameManager()
    {
        return networkedGameManager;
    }
    
    #endregion
    
    
    #region UI Panels
    
    public PanelRenderer GetLobbyMenuPanel()
    {
        return lobbyMenuPanel;
    }

    public PanelRenderer GetLobbyJoinPanel()
    {
        return lobbyJoinPanel;
    }

    public PanelRenderer GetUserNamePanel()
    {
        return usernamePanel;
    }

    public PanelRenderer GetPlacementPanel()
    {
        return placementPanel;
    }
    
    public PanelRenderer GetWinPanel()
    {
        return winPanel;
    }
    
    public PanelRenderer GetCountdownPanel()
    {
        return gameCountdownPanel;
    }
    
    #endregion

}


