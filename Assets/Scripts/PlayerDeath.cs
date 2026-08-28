using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private float respTimer;
    private float curRespTimer;
    
    private CheckPointManager checkPointManager;
    private GameObject carModelObj;
    private Rigidbody rigidbody;
    private CarMovement carMovement;
    private CarStateMachine carStateMachine;
    
    bool isRespawning = false;
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        checkPointManager = GameManager.Instance.GetCheckPointManager();
        carModelObj = GameManager.Instance.GetCarModel();
        rigidbody = GetComponent<Rigidbody>();
        carMovement = GetComponent<CarMovement>();
        carStateMachine = GameManager.Instance.GetStateMachine();
    }

    // Update is called once per frame
    private void Update() 
    {
        if(isRespawning && curRespTimer >= 0.1)
        {
            carModelObj.SetActive(false);
            rigidbody.useGravity = false;
            rigidbody.linearVelocity = new Vector3(0,0,0);
            rigidbody.angularVelocity = new Vector3(0,0,0);
            carMovement.enabled = false;
            
            curRespTimer -= Time.fixedDeltaTime;
        }
        else if (isRespawning)
        {
            if(!checkPointManager.isAtCheckpoint())
            {
                checkPointManager.GotoCheckpoint();
            }
            else
            {
                isRespawning = false;
                carStateMachine.isDead = false;
                carModelObj.SetActive(true);
                rigidbody.useGravity = true;
                carMovement.enabled = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "DeathBox" && !isRespawning)
        {
            Debug.Log("IsRespawning");
            carStateMachine.isDead = true;
            isRespawning = true;
            curRespTimer = respTimer;
        }
    }
}
