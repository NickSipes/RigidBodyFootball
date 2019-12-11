﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TE : OffPlayer
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        AddClickCollider();
        rayColor = Color.green;
        gameManager.shedBlock += DefShedBlock;
        gameManager.offPlayChange += ChangeOffRoute;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked)
        {
            return;
        }

        if (gameManager.WhoHasBall() == this) return;
        if (isCatching) return;
        if (gameManager.isRun)
        {
            if (gameManager.ballOwner == this)
            {
                transform.forward = Vector3.forward;
                navMeshAgent.enabled = false;
                return;
            }
            canBlock = true;
            BlockProtection();
        }

        if (gameManager.isPass)
        {
            if (isBlocker)
            {
                BlockProtection();
                return;
            }

            if (!isReciever) return;

            if (wasAtLastCut)
            {
                WatchQb();
                //Debug.Log("last cut");
                return;
            }


            if (IsEndOfRoute())
            {
                StopNavMeshAgent();
                Debug.Log("EndOfRoute Update");
                return;
            }
            else RunRoute();


        }
    }
    private void DefShedBlock(FootBallAthlete brokeBlock)
    {
        //todo check assignment
        if (isReciever) return;

        isBlocker = true;
        BlockProtection();
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
            var number = this.name;
            switch (number)
            {
                case "TE1":
                    routeSelection = offPlay.TeRoute[0];
                    isBlocker = offPlay.isOffPlayerBlock[0];
                    break;
                case "TE2":
                    routeSelection = offPlay.TeRoute[1];
                    isBlocker = offPlay.isOffPlayerBlock[1];
                    break;
                default:
                    routeSelection = offPlay.TeRoute[0];
                    isBlocker = offPlay.isOffPlayerBlock[0];
                    break;
            }

            if (!isBlocker) isReciever = true;
            Destroy(myRoute);
            GetRoute(routeSelection);
        }
    }
}
