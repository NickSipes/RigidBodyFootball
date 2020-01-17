using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
// ReSharper disable All
#pragma warning disable 108,114

public class DB : DefPlayer
{
    

    //todo HOW ARE WE GOING TO HANDLE JUMP ANIMATIONS
    //todo DB State Machine
    //todo readdress how pass incomlpetion are cacluated. All determining factors should be rolls vs stats ?
    //todo scramble mechanics
    //todo Man coverage
    
    internal void Start()
    {
        base.Start();
        rayColor = Color.yellow;
        gameManager.hikeTheBall += HikeTheBall;
        gameManager.onBallThrown += BallThrown;
        gameManager.passAttempt += PassAttempt;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    internal override void Update()
    {
        base.Update();
    }

    private void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
        if (gameManager.isRun)
        {
            return;
        }

        if (gameManager.isPass)
        {
            //todo move press code here!
            var potientialTarget = GetClosestWr(wideRecievers);
            if ((potientialTarget.transform.position - transform.position).magnitude < 5f)
            {
                SetTargetOffPlayer(potientialTarget);
                if(!myDefJob.isPress)return;
                if (targetOffPlayer.CanBePressed())
                    StartCoroutine(PressTarget(targetOffPlayer));
                //todo press range variable
            }
        }
    }
}