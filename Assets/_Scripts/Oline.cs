using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Oline : FootBallAthlete
{



    void Start()
    {
        FindComponents();
    }

    private void FindComponents()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        //aiCharacter = GetComponent<AICharacterControl>();
        //userControl = GetComponent<ThirdPersonUserControl>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        anim = GetComponent<Animator>();
        qb = FindObjectOfType<QB>();
        hbs = FindObjectsOfType<HB>();
        wideRecievers = FindObjectsOfType<WR>();
        defBacks = FindObjectsOfType<DB>();
        oLine = FindObjectsOfType<Oline>();
        dLine = FindObjectsOfType<Dline>();
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked) return;
        if (gameManager.isPass)
        {
            PassProtection();
        }
        if (gameManager.isRun)
        {
            RunProtection();
        }


    }
    public void HikeTheBall(bool wasHiked)
    {
        Debug.Log("Oline Hike");
        anim.SetTrigger("HikeTrigger");
    }

    void PassProtection()
    {
        if (target == null)
        {
            target = GetClosestDline(dLine);
        }
        SetTargetDline(target);
        Vector3 directionToTarget = target.position - transform.position;
        float dSqrToTarget = directionToTarget.sqrMagnitude;
        if (dSqrToTarget < 3)//todo setup block range variable
        {
            var dlineToBlock = target.GetComponent<Dline>();
            if (!dlineToBlock.wasBlocked && !dlineToBlock.isBlocked)
            {
                Debug.Log("Block " + target.name);
                StartCoroutine("BlockTarget", target);
            }
        }

    }
    void RunProtection()
    {
        if (target == null)
        {
            target = GetClosestDline(dLine);
        }
        SetTargetDline(target);
        Vector3 directionToTarget = target.position - transform.position;
        float dSqrToTarget = directionToTarget.sqrMagnitude;
        if (dSqrToTarget < 1)
        {
            var dlineToBlock = target.GetComponent<Dline>();
            if (!dlineToBlock.wasBlocked && !dlineToBlock.isBlocked)
            {
            
                StartCoroutine("BlockTarget", target);
            }
        }
    }

    IEnumerator BlockTarget(Transform target)
    {

        var lineMan = target.GetComponent<Dline>();
        float blockTime = 1f; // make setable variable
        float blockTimeNorm = 0;
        while (blockTimeNorm <= 1f) //todo this counter is ugly and needs to be better
        {

            isBlocking = true;
            blockTimeNorm += Time.deltaTime / blockTime;
            lineMan.Block(blockTimeNorm, this);
            yield return new WaitForEndOfFrame();
        }
        lineMan.ReleaseBlock();
        isBlocking = false;
    }

    Transform GetClosestDline(Dline[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (Dline potentialTarget in enemies)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {

                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
            }
        }

        return bestTarget;
    }
    public void SetTargetDline(Transform targetSetter)
    {

        navMeshAgent.SetDestination(qb.transform.position + (targetSetter.position - qb.transform.position) / 2);



    }

}
