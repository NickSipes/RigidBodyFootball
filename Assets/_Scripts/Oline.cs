using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Oline : OffPlayer
{


    void Start()
    {
        rayColor = Color.magenta;
        FindComponents();
        gameManager.hikeTheBall += HikeTheBall;
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
        qb = FindObjectOfType<QB>();
        hbs = FindObjectsOfType<HB>();
        wideRecievers = FindObjectsOfType<WR>();
        defBacks = FindObjectsOfType<DB>();
        oLine = FindObjectsOfType<Oline>();
        dLine = FindObjectsOfType<Dline>();
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked) return;
      
            BlockProtection();
        
    }
    private void FixedUpdate()
    {
        base.FixedUpdate();
    
    }

    public void HikeTheBall(bool wasHiked)
    {
        //Debug.Log("Oline Hike");
        anim.SetTrigger("HikeTrigger");
    }

  
}
