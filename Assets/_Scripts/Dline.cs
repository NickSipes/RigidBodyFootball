using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class Dline : DefPlayer
{

   
    // Start is called before the first frame update
    void Start()
    {
        rayColor = Color.red;
        FindComponents();
        AddEvents();
    }


    private void FindComponents()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        //aiCharacter = GetComponent<AICharacterControl>();
        //userControl = GetComponent<ThirdPersonUserControl>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        anim = GetComponent<Animator>();
        hbs = FindObjectsOfType<HB>();
        wideRecievers = FindObjectsOfType<WR>();
        defBacks = FindObjectsOfType<DB>();
        oLine = FindObjectsOfType<Oline>();
        dLine = FindObjectsOfType<Dline>();
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
    }
    private void AddEvents()
    {
        gameManager.ballOwnerChange += BallOwnerChange;
        gameManager.hikeTheBall += HikeTheBall;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked) return;

        if (targetPlayer == null)
        {
            targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        }
        SetTargetPlayer(targetPlayer);

        if (wasBlocked && !isBlocked)
            StartCoroutine("BlockCoolDown"); 
    }
    private void FixedUpdate()
    {
       base.FixedUpdate();  
    }


    public void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
    }
    
   

   

    public void BallOwnerChange(FootBallAthlete ballOwner)
    {
        SetTargetPlayer(ballOwner.transform);
    }
}
