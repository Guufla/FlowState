using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Splines;


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
    [SerializeField] CarStateMachine    carStateMachine;
    [SerializeField] InputManager       inputManager;
    [SerializeField] SplineContainer    splines;
    [SerializeField] CheckPointManager  checkPointManager;
    [SerializeField] GameObject         carModel;
    
    private GameObject playerObject;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetPlayer(GameObject playerObject)
    {
        this.playerObject = playerObject; 
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
}
