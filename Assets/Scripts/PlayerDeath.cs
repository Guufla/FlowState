using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private float respTimer;
    private float curRespTimer;
    
    [SerializeField] private float transTimer;
    private float curTransTimer;
    
    private CheckPointManager checkPointManager;
    private GameObject carModelObj;
    private Rigidbody rigidbody;
    private CarMovement carMovement;
    
    bool isRespawning = false;
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        checkPointManager = GameManager.Instance.GetCheckPointManager();
        carModelObj = GameManager.Instance.GetCarModel();
        rigidbody = GetComponent<Rigidbody>();
        carMovement = GetComponent<CarMovement>();
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
            curTransTimer = transTimer;
        }
        else if (isRespawning)
        {
            if(curTransTimer >= 0.1)
            {
                checkPointManager.GotoCheckpoint();
                curTransTimer -= Time.fixedDeltaTime;
            }
            else
            {
                isRespawning = false;
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
            isRespawning = true;
            curRespTimer = respTimer;
        }
    }
}
