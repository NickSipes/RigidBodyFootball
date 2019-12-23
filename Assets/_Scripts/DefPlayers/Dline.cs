using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


// ReSharper disable once CheckNamespace
public class Dline : DefPlayer
{
    // Start is called before the first frame update
    internal void Start()
    {
        base.Start();
        rayColor = Color.red;
        gameManager.hikeTheBall += HikeTheBall;
    }

    // Update is called once per frame
    internal override void Update()
    {
        if (!gameManager.isHiked) return;

        if (transformTarget == null)
        {
            transformTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
        SetTargetPlayer(transformTarget);

        if (wasBlocked && !isBlocked)
            StartCoroutine("BlockCoolDown"); 
    }

    public override void FixedUpdate()
    {
       base.FixedUpdate();  
    }


    public void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
    }
    
   

   

}
