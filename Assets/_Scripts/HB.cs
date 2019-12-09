using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.AI;
#pragma warning disable 108,114


public class HB : OffPlayer
{
    public bool isReciever;
  
    // Use this for initialization
    void Start()
    {
        base.Start();
        rayColor = Color.green;
        gameManager.shedBlock += DefShedBlock;
        gameManager.offPlayChange += ChangeOffRoute;
    }
    
    private void FixedUpdate()
    {
        base.FixedUpdate();

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
        if (!gameManager.isHiked)
        {
            if (gameManager.isPass)
            {
                AddClickCollider();
            }
            return;
        }

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
                return;
            }

            if (isReciever)
            {
                if (sphereCollider == null)
                {
                    AddClickCollider();
                }
                if (wasAtLastCut)
                {
                    WatchQb();
                    Debug.Log("last cut");
                    return;
                }


                if (IsEndOfRoute())
                {
                    StopNavMeshAgent();
                    Debug.Log("EndOfRoute Update");
                    return;
                }

                if (!IsEndOfRoute()) RunRoute();
            }


        }
    }

    void ChangeOffRoute(OffPlay offPlay)
    {
        if (gameManager.isRun)
        {
            Destroy(myRoute);
            isReciever = false;
            return;
        }

        if (gameManager.isPass)
        {
            var hbNumber = this.name;
            switch (hbNumber)
            {
                case "HB1":
                    routeSelection = offPlay.HbRoute[0];
                    isBlocker = offPlay.isHbBlock[0];
                    break;
                case "HB2":
                    routeSelection = offPlay.HbRoute[1];
                    isBlocker = offPlay.isHbBlock[1];
                    break;
                default:
                    routeSelection = offPlay.HbRoute[0];
                    isBlocker = offPlay.isHbBlock[0];
                    break;
            }
            if (!isBlocker) isReciever = true;
            Destroy(myRoute);
            GetRoute(routeSelection);
        }
    }

}



