using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.UIElements;
using UnityStandardAssets.Characters.ThirdPerson;

public class HB : FootBallAthlete
{
    
    // Use this for initialization
    void Start()
    {
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
    void AddPlayerControl()
    {       
       
        navMeshAgent.enabled = false;
   
    }

    internal void SetPlayerTag()
    {
      tag = "Player";
      AddPlayerControl();
    }
}
