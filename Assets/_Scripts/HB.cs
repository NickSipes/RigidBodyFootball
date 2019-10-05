using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.UIElements;


public class HB : OffPlayer
{

    // Use this for initialization
    void Start()
    {
        rayColor = Color.green;
        gameManager = FindObjectOfType<GameManager>();
        qb = FindObjectOfType<QB>();
        hbs = FindObjectsOfType<HB>();
        rb = GetComponent<Rigidbody>();
        startColor = materialRenderer.material.color;
        anim = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.destination = transform.position;
        //targetPlayer = startGoal.transform;
        //lr.material.color = LineColor;
        dLine = FindObjectsOfType<Dline>();

        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
        gameManager.hikeTheBall += HikeTheBall;
        gameManager.shedBlock += DefShedBlock;
    }

    private void DefShedBlock(FootBallAthlete brokeBlock)
    {
        //todo check assignment
        isBlocker = true;
        BlockProtection();
    }

    private void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked) return;

        if (gameManager.isRun && gameManager.ballOwner == this)
        {
            transform.forward = Vector3.forward;
            navMeshAgent.enabled = false;
        }

        if (gameManager.isPass)
        {
            if (isBlocker)
            {
                BlockProtection();
            }
        }

    }
    private void FixedUpdate()
    {
        base.FixedUpdate();
       
    }

    
}
   

  
