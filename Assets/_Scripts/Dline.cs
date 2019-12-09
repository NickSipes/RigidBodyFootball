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
        base.Start();
        rayColor = Color.red;
        AddEvents();
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
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    private void FixedUpdate()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
    {
       base.FixedUpdate();  
    }


    public void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
    }
    
   

   

}
