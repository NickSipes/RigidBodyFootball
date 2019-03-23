﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.UIElements;


public class HB : FootBallAthlete
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
        //target = startGoal.transform;
        //lr.material.color = LineColor;
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
        gameManager.hikeTheBall += HikeTheBall;
    }

    private void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked) return;

        if (gameManager.isRun)
        {

        }
    }
    private void FixedUpdate()
    {
        base.FixedUpdate();
       
    }

    
}
   

  
