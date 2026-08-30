using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Splines;
using System.Collections.Generic;
using UnityEngine.UIElements;
using FishNet.Demo.AdditiveScenes;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    [Header("References")]
    [SerializeField] CarStateMachine        carStateMachine;
    [SerializeField] InputManager           inputManager;
    [SerializeField] SplineContainer        splines;
    [SerializeField] CheckPointManager      checkPointManager;
    [SerializeField] GameObject             carModel;

    [SerializeField] NetworkedGameManager   networkedGameManager;

    [SerializeField] UIManager              uiManager;

    [SerializeField] LobbyManager           lobbyManager;
    
    [SerializeField] private PanelRenderer lobbyMenuPanel;
    [SerializeField] private PanelRenderer lobbyJoinPanel;
    [SerializeField] private PanelRenderer usernamePanel;
    [SerializeField] private PanelRenderer placementPanel;
    [SerializeField] private PanelRenderer winPanel;
    [SerializeField] private PanelRenderer gameCountdownPanel;

    private CarMovement playerMovementScript;

    [Header("Variables")]
    [SerializeField] private List<GameObject> spawnPoints;
    private GameObject playerObject;

    private int playerIndex;
    private bool isHost = false;

    private float playerPositionWeight;

    private int maxPlayers;

    private int curPlayers;

    private bool serverInitialized;

    private int playerPlacement;

    private float curPlacementWeight;

    private string playerName;

    private bool isGameStarted;
    private bool isGameStarting;

    public float gameCountdown;

    private List<float> playerPlacementWeights;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    
    void Update()
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

    void FixedUpdate()
    {
        if(isGameStarting)
        {
            GameCountdown();
        }
    }

    void Start()
    {
        networkedGameManager.gameObject.SetActive(true);
        lobbyManager.gameObject.SetActive(true);
    }
    
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
    
    public void PlayerFinished()
    {
        networkedGameManager.UpdatePlayersFinishedServerRpc(playerName, playerPlacement);
    }
    
    
    
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
    public void SetPlayerIndex(int playerIndex)
    {
        this.playerIndex = playerIndex;
        if (playerIndex == 0)
        {
            isHost = true;
        }
    }
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
    public void SetServerInitialized()
    {
        serverInitialized = true;
    }
    public bool GetServerInitialized()
    {
        return serverInitialized;
    }
    
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
    
    public int GetPlayerPlacement()
    {
        return playerPlacement;
    }
    
    public UIManager GetUIManager()
    {
        return uiManager;
    }
    public NetworkedGameManager GetNetworkedGameManager()
    {
        return networkedGameManager;
    }
    
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }
    
    public string GetPlayerName()
    {
        return playerName;
    }
    public void SetGameStarting(bool isGameStarting)
    {
        if(isGameStarting)
        {
            uiManager.EnableCountdown(true);
            carStateMachine.isDead = true;
            playerObject.transform.position = spawnPoints[playerIndex].transform.position;
            playerObject.transform.rotation = spawnPoints[playerIndex].transform.rotation;
            playerMovementScript.SetTargetInitialRotation(spawnPoints[playerIndex].transform.rotation);
            carStateMachine.isDead = false;
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
            isGameStarted = true;
            isGameStarting = false;
            uiManager.EnableCountdown(false);
        }
    }
    
    
    public bool GetGameStart()
    {
        return isGameStarted;
    }
}


