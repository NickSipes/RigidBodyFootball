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
        aiCharacter = GetComponent<AICharacterControl>();
        thirdPerson = GetComponent<ThirdPersonCharacter>();
        userControl = GetComponent<ThirdPersonUserControl>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        startGoal = aiCharacter.target;
        aiCharacter.target = null;
        target = startGoal;
        //lr.material.color = LineColor;
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
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
        userControl.enabled = true;
        navMeshAgent.enabled = false;
        aiCharacter.enabled = false;
    }

    internal void SetPlayerTag()
    {
      tag = "Player";
      AddPlayerControl();
    }
}
