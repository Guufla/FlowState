using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class PracticeGameManager : GameManager
{

    #region Debug Variables

    [Header("Debug Variables")]
    [SerializeField] bool noNetworking = true;
    
    [SerializeField] GameObject debugPlayerObject;

    #endregion
    
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        // DO NOT call base.Start()
        // That prevents lobby/network startup.

        if (debugPlayerObject != null)
        {
            SetPlayer(debugPlayerObject);
            isGameStarted = true;
        }
    }

    protected override void Update()
    {
        // Intentionally empty.
        //
        // No networking
        // No placement calculation
        // No checkpoint resetting
        // No lobby logic
    }

    protected override void FixedUpdate()
    {
        // Intentionally empty.
        //
        // No countdown.
    }
    
    

}
