using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Oline : FootBallAthlete
{


    void Start()
    {
        rayColor = Color.magenta;
        FindComponents();
        gameManager.hikeTheBall += HikeTheBall;
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
    private void FixedUpdate()
    {
        base.FixedUpdate();
    
    }

    public void HikeTheBall(bool wasHiked)
    {
        //Debug.Log("Oline Hike");
        anim.SetTrigger("HikeTrigger");
    }

    void PassProtection() //todo consolidate duplicate code
    {
        if (target == null)
        {
            target = GetClosestDline(dLine);
        }
        SetTargetDline(target);
        transform.LookAt(target);
        Vector3 directionToTarget = target.position - transform.position;
        float dSqrToTarget = directionToTarget.sqrMagnitude;
        if (dSqrToTarget < 3)//todo setup block range variable
        {
            var dlineToBlock = target.GetComponent<Dline>();
            if (!dlineToBlock.wasBlocked && !dlineToBlock.isBlocked)
            {
                //Debug.Log("Block " + target.name);
                StartCoroutine("BlockTarget", target);
            }
        }
    }
 
    void RunProtection() //todo consolidate duplicate code
    {
        if (target == null)
        {
            target = GetClosestDline(dLine);
        }
        Vector3 directionToTarget = target.position - transform.position;
        transform.LookAt(target);
        //todo change code to be moving foreward with blocks, get to the second level after shedding first defender
        SetTargetDlineRun(target);
        float dSqrToTarget = directionToTarget.sqrMagnitude;
        if (dSqrToTarget < 3) //todo setup runblock range variable
        {
            var dlineToBlock = target.GetComponent<Dline>();
            if (!dlineToBlock.wasBlocked && !dlineToBlock.isBlocked)
            {
                StartCoroutine("RunBlockTarget", target);
            }
        }
    }

    private void SetTargetDlineRun(Transform target) //todo collapse into single function
    {
        SetDestination(target.position); // todo centralize ball carrier, access ballcarrier instead of hard coded transform
    }

    public void SetTargetDline(Transform targetSetter)
    {
        SetDestination(qb.transform.position + (targetSetter.position - qb.transform.position) / 2); // todo centralize ball carrier, access ballcarrier instead of hard coded transform
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
    IEnumerator RunBlockTarget(Transform target)
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
}
