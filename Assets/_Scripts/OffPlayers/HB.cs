using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.AI;
#pragma warning disable 108,114


public class HB : OffPlayer
{
 
    // Use this for initialization
    void Start()
    {
        base.Start();
        AddClickCollider();
        rayColor = Color.green;
        gameManager.shedBlock += DefShedBlock;
        //gameManager.offPlayChange += ChangeOffPlay;
    }
    
    private void FixedUpdate()
    {
        base.FixedUpdate();

    }
 

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if (!gameManager.isHiked)
        {
            return;
        }

        if (gameManager.WhoHasBall() == this) return;
        if (isCatching) return;

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
            }else RunRoute();


        }
    }

    //void ChangeOffRoute(OffPlay offPlay)
    //{
    //    if (gameManager.isRun)
    //    {
    //        Destroy(myRoute);
    //        isReciever = false;
    //        return;
    //    }

    //    if (gameManager.isPass)
    //    {
    //        var hbNumber = this.name;
    //        switch (hbNumber)
    //        {
                
    //            default:
    //                routeSelection = offPlay.HbRoute[0];
    //                isBlocker = offPlay.isSkillPlayerBlock[0];
    //                break;
    //        }
    //        if (!isBlocker) isReciever = true;
    //        Destroy(myRoute);
    //        GetRoute(routeSelection);
    //    }
    //}
    private void DefShedBlock(FootBallAthlete brokeBlock)
    {
        //todo check assignment
        if (isReciever) return;

        isBlocker = true;
        BlockProtection();
    }

    private void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
    }

}



