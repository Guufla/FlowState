using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private PanelRenderer lobbyMenuPanel;
    private PanelRenderer lobbyJoinPanel;
    private PanelRenderer usernamePanel;
    private PanelRenderer placementPanel;
    private PanelRenderer winPanel;
    private PanelRenderer countdownPanel;

    private UIState UIState = UIState.UserName;
    
    private int curPlacement;

    private TextField userName;
    private TextField lobbyName;
    private TextField joinCode;
    
    private Label lobbyNameLabel;
    private Label lobbyCodeLabel;
    private Label lobbyPlayersLabel;
    
    private Label placementNumber;
    private Label placementSuffix;
    private Label countdownText;

    private List<Label> userNames = new List<Label>();

    private Button setUserName;
    private Button startLobby;
    private Button joinLobby;
    private Button startGame;
    private Button backToLobbyButton;

    [SerializeField] List<Color> colorList;
    
    public Vector3 firstPlaceColorVector;

    [SerializeField] Color firstPlaceColor;
    [SerializeField] float firstPlaceColorSpeed;

    Color StartGameInitialColor;

    private int colorStage;

    public List<string> finalRanks = new List<string>();
    
    
    public event Action OnSetUserName;
    public event Action OnCreateLobby;
    public event Action OnJoinLobby;
    public event Action OnStartGame;
    
    
    void Start()
    {
        lobbyMenuPanel = GameManager.Instance.GetLobbyMenuPanel();
        lobbyJoinPanel = GameManager.Instance.GetLobbyJoinPanel();
        usernamePanel = GameManager.Instance.GetUserNamePanel();
        placementPanel = GameManager.Instance.GetPlacementPanel();
        winPanel = GameManager.Instance.GetWinPanel();
        countdownPanel = GameManager.Instance.GetCountdownPanel();

        RegisterUICallbacks();

        finalRanks.Clear();

        for (int i = 0; i < GameManager.Instance.GetMaxPlayers(); i++)
        {
            finalRanks.Add("");
        }

        ChangeUIState(UIState.UserName);
        EnableCountdown(false);
    }
    

    
    private void OnDestroy()
    {
        UnregisterUICallbacks();
    }


    private void RegisterUICallbacks()
    {
        usernamePanel.RegisterUIReloadCallback(OnUIReload);
        lobbyJoinPanel.RegisterUIReloadCallback(OnUIReload);
        lobbyMenuPanel.RegisterUIReloadCallback(OnUIReload);
        placementPanel.RegisterUIReloadCallback(OnUIReload);
        winPanel.RegisterUIReloadCallback(OnUIReload);
        countdownPanel.RegisterUIReloadCallback(OnUIReload);
    }


    private void UnregisterUICallbacks()
    {
        usernamePanel.UnregisterUIReloadCallback(OnUIReload);
        lobbyJoinPanel.UnregisterUIReloadCallback(OnUIReload);
        lobbyMenuPanel.UnregisterUIReloadCallback(OnUIReload);
        placementPanel.UnregisterUIReloadCallback(OnUIReload);
        winPanel.UnregisterUIReloadCallback(OnUIReload);
        countdownPanel.UnregisterUIReloadCallback(OnUIReload);
    }


    private void OnUIReload(PanelRenderer renderer, VisualElement root)
    {
        if(renderer == usernamePanel)
        {
            SetUsernamePanelVariables(root);
        }
        
        else if(renderer == lobbyJoinPanel)
        {
            SetLobbyJoinPanelVariables(root);
        }
        
        else if(renderer == lobbyMenuPanel)
        {
            SetLobbyMenuPanelVariables(root);
        }
        
        else if(renderer == placementPanel)
        {
            SetPlacementPanelVariables(root);
        }
        
        else if(renderer == winPanel)
        {
            SetWinPanelVariables(root);
        }
        else if(renderer == countdownPanel)
        {
            SetCountdownPanelVariables(root);
        }
    }
    private void SetCountdownPanelVariables(VisualElement root)
    {
        countdownText = root.Q<Label>("Count");
    }


    private void SetUsernamePanelVariables(VisualElement root)
    {
        userName = root.Q<TextField>("UserName");
        setUserName = root.Q<Button>("UserNameButton");

        setUserName.clicked -= SetUserNameButton;
        setUserName.clicked += SetUserNameButton;
    }


    private void SetLobbyJoinPanelVariables(VisualElement root)
    {
        lobbyName = root.Q<TextField>("LobbyName");
        joinCode = root.Q<TextField>("JoinCode");

        startLobby = root.Q<Button>("LobbyStartButton");
        joinLobby = root.Q<Button>("LobbyJoinButton");

        startLobby.clicked -= CreateLobbyButton;
        joinLobby.clicked -= JoinLobbyButton;
        
        startLobby.clicked += CreateLobbyButton;
        joinLobby.clicked += JoinLobbyButton;
    }


    private void SetLobbyMenuPanelVariables(VisualElement root)
    {
        lobbyNameLabel = root.Q<Label>("LobbyName");
        lobbyCodeLabel = root.Q<Label>("LobbyCode");
        lobbyPlayersLabel = root.Q<Label>("LobbyPlayerCount");

        startGame = root.Q<Button>("StartGame");

        StartGameInitialColor = startGame.resolvedStyle.backgroundColor;
        StartGameInitialColor.a = 1f;

        startGame.clicked -= StartGameButton;
        startGame.clicked += StartGameButton;
    }
    
    public void StartGameBuffer(bool isBuffer)
    {
        if (startGame == null) return;
        
        if(isBuffer)
        {
            Debug.Log("SWITCHING COLOR");
            startGame.style.backgroundColor = new Color(StartGameInitialColor.r - 0.5f,StartGameInitialColor.g - 0.5f,StartGameInitialColor.b - 0.5f,0.5f);
        }
        else
        {
            startGame.style.backgroundColor = StartGameInitialColor;
        }
    }


    public void SetPlacementPanelVariables(VisualElement root)
    {
        placementNumber = root.Q<Label>("Placement");
        placementSuffix = root.Q<Label>("Suffix");
    }


    public void SetWinPanelVariables(VisualElement root)
    {
        
        for(int i = 0; i < GameManager.Instance.GetMaxPlayers(); i++)
        {
            userNames.Add(root.Q<Label>("Username" + (i + 1).ToString()));
        }
    
        backToLobbyButton = root.Q<Button>("BackToLobby");

        backToLobbyButton.clicked -= BackToLobby;
        backToLobbyButton.clicked += BackToLobby;
    }


    private void SetUserNameButton()
    {
        OnSetUserName?.Invoke();
    }


    private void CreateLobbyButton()
    {
        OnCreateLobby?.Invoke();
    }


    private void JoinLobbyButton()
    {
        OnJoinLobby?.Invoke();
    }


    private void StartGameButton()
    {
        OnStartGame?.Invoke();
    }


    void Update()
    {
        curPlacement = GameManager.Instance.GetPlayerPlacement();

        UpdatePlacementUI();
        Update1stColor();
    }


    public string GetUserName()
    {
        if(userName == null)
            return "";

        return userName.value;
    }


    public string GetLobbyName()
    {
        if(lobbyName == null)
            return "";

        return lobbyName.value;
    }


    public string GetJoinCode()
    {
        if(joinCode == null)
            return "";

        return joinCode.value;
    }


    public void UpdateLobbyUI(string name, string code)
    {
        if(lobbyNameLabel != null)
        {
            lobbyNameLabel.text = "Lobby Name: " + name;
        }

        if(lobbyCodeLabel != null)
        {
            lobbyCodeLabel.text = "Lobby Code: " + code;
        }
    }


    public void UpdateLobbyPlayersUI(int playerCount, string playerList)
    {
        if(lobbyPlayersLabel == null)
            return;

        lobbyPlayersLabel.text = "Lobby Players " + "(" + playerCount + ")" + playerList;
    }


    public void SetWinScreen()
    {
        finalRanks = GameManager.Instance.GetNetworkedGameManager().GetFinalRanks();
        
        for(int i = 0; i < GameManager.Instance.GetMaxPlayers(); i++)
        {

            userNames[i].text = finalRanks[i];
        }
    }
    
    public void UpdateCountDown(float countdownNum)
    {
        countdownText.text = Mathf.Round(countdownNum).ToString();
        countdownText.style.unityTextOutlineColor = firstPlaceColor;
    }
    public void EnableCountdown(bool isCountdown)
    {
        countdownPanel.enabled = isCountdown;
    }


    void Update1stColor()
    {
        float amount = firstPlaceColorSpeed * Time.deltaTime;

        switch (colorStage)
        {
            // Red -> Yellow
            case 0:
                firstPlaceColorVector.y = Mathf.MoveTowards(firstPlaceColorVector.y, 255f, amount);

                if (firstPlaceColorVector.y >= 255f)
                    colorStage = 1;
                break;

            // Yellow -> Green
            case 1:
                firstPlaceColorVector.x = Mathf.MoveTowards(firstPlaceColorVector.x, 0f, amount);

                if (firstPlaceColorVector.x <= 0f)
                    colorStage = 2;
                break;

            // Green -> Cyan
            case 2:
                firstPlaceColorVector.z = Mathf.MoveTowards(firstPlaceColorVector.z, 255f, amount);

                if (firstPlaceColorVector.z >= 255f)
                    colorStage = 3;
                break;

            // Cyan -> Blue
            case 3:
                firstPlaceColorVector.y = Mathf.MoveTowards(firstPlaceColorVector.y, 0f, amount);

                if (firstPlaceColorVector.y <= 0f)
                    colorStage = 4;
                break;

            // Blue -> Magenta
            case 4:
                firstPlaceColorVector.x = Mathf.MoveTowards(firstPlaceColorVector.x, 255f, amount);

                if (firstPlaceColorVector.x >= 255f)
                    colorStage = 5;
                break;

            // Magenta -> Red
            case 5:
                firstPlaceColorVector.z = Mathf.MoveTowards(firstPlaceColorVector.z, 0f, amount);

                if (firstPlaceColorVector.z <= 0f)
                    colorStage = 0;
                break;
        }

        firstPlaceColor = new Color(
            firstPlaceColorVector.x / 255f,
            firstPlaceColorVector.y / 255f,
            firstPlaceColorVector.z / 255f,
            1f
        );
    }


    void UpdatePlacementUI()
    {
        if (placementNumber == null) return;

        Color outlineColor = colorList[0];
        
        if(curPlacement == 1)
        {
            placementNumber.text = curPlacement.ToString();
            placementSuffix.text = "st";

            outlineColor = firstPlaceColor;
        }
        else if(curPlacement == 2)
        {
            placementNumber.text = curPlacement.ToString();
            placementSuffix.text = "nd";

            outlineColor = colorList[1];
        }
        else if(curPlacement == 3)
        {
            placementNumber.text = curPlacement.ToString();
            placementSuffix.text = "rd";

            outlineColor = colorList[2];
        }
        else if(curPlacement > 3)
        {
            placementNumber.text = curPlacement.ToString();
            placementSuffix.text = "th";
            
            outlineColor = colorList[curPlacement - 1];
        }
        
        placementNumber.style.unityTextOutlineColor = outlineColor;
        placementSuffix.style.unityTextOutlineColor = outlineColor;
        
        placementNumber.style.unityTextOutlineWidth = 12f;
        placementSuffix.style.unityTextOutlineWidth = 12f;
    }


    public void ChangeUIState(UIState newState)
    {
        UIState = newState;

        usernamePanel.enabled = newState == UIState.UserName;
        lobbyJoinPanel.enabled = newState == UIState.Join;
        lobbyMenuPanel.enabled = newState == UIState.Lobby;
        placementPanel.enabled = newState == UIState.Start;
        winPanel.enabled = newState == UIState.Win;
    }


    private void BackToLobby()
    {
        Debug.Log("ClickedBackTOLobby");
        ChangeUIState(UIState.Lobby);
    }
}