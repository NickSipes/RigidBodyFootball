using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
// ReSharper disable All
#pragma warning disable 108,114

public class DB : DefPlayer
{
    public object SetTargetWr;

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
        //CreateZone(); //todo only run if player is in zone
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void Update()
    {
        if (!gameManager.isHiked)
            return;

        if (gameManager.isRun)
        {
            if (isBlocked) return;
            PlayReact();

        }

        // ReSharper disable once InvertIf
        if (gameManager.isPass)
        {
            if (isBlockingPass) return;

            if (isPressing) return;

            if (isBackingOff) return;
            {
                if (isZone)

                {
                    //todo this whole triple if statement sucks
                    if (targetReceiver != null)
                    {
                        if (IsTargetInZone(targetReceiver.transform))
                        {

                            SetDestination(targetReceiver.transform.position);
                            return;
                        }
                        else
                        {
                            //Debug.Log("targetPlayer out of zone");
                            targetReceiver = null;
                            targetPlayer = null;
                        }
                    }
                }
            }

            PlayZone();
        }

        // ReSharper disable once InvertIf
    }



    private void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
        if (gameManager.isRun)
        {

        }

        if (gameManager.isPass)
        {
            //todo move press code here!
            var potientialTarget = GetClosestWr(wideRecievers);
            if ((potientialTarget.transform.position - transform.position).magnitude < 5f)
            {
                SetTargetOffPlayer(potientialTarget);
                if (targetReceiver.CanBePressed())
                    StartCoroutine(WrPress(targetReceiver));
                //todo press range variable
            }
        }
    }
}