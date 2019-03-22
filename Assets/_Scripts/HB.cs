using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.UIElements;


public class HB : FootBallAthlete
{
    private Color rayColor = Color.green;

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
    private void FixedUpdate()
    {
        RaycastForward();
    }

    internal void RaycastForward()
    {
     
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Debug.DrawRay(transform.position, forward, rayColor);

        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.forward, 100.0F);
        //if(hits.Length != 0)Debug.Log(hits.Length);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.isTrigger)
            {
                Transform zoneObject = hit.collider.transform;
                if (zoneObject)
                {

                }
            }

        }
    }
    Color SetRaycastColor()
    {
        return materialRenderer.material.color;
    }
}
   

  
