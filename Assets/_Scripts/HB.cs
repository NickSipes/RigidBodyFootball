using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class HB : OffPlayer
{
    public bool isReciever;
    // Use this for initialization
    void Start()
    {
        rayColor = Color.green;
        gameManager = FindObjectOfType<GameManager>();
        qb = FindObjectOfType<QB>();
        hbs = FindObjectsOfType<HB>();
        rb = GetComponent<Rigidbody>();
        startColor = materialRenderer.material.color;
        anim = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        routeManager = FindObjectOfType<RouteManager>();
        navMeshAgent.destination = transform.position;
        //targetPlayer = startGoal.transform;
        //lr.material.color = LineColor;
        dLine = FindObjectsOfType<Dline>();

        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
        gameManager.hikeTheBall += HikeTheBall;
        gameManager.shedBlock += DefShedBlock;
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
            if (isReciever && myRoute == null)
            {
                GetRoute();
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
            }

            if (isReciever)
            {
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

    private void GetRoute()
    {
        myRoute = Instantiate(routeManager.allRoutes[routeSelection], transform.position, transform.rotation).GetComponent<Routes>(); //todo get route index selection
        myRoute.transform.name = routeManager.allRoutes[routeSelection].name;

        //myRoute.transform.position = transform.position;

        var childCount = myRoute.transform.childCount;
        totalCuts = childCount - 1; //todo had to subtract 1 because arrays start at 0

        lastCutVector = myRoute.transform.GetChild(totalCuts).position;
        Debug.Log("total cuts " + totalCuts + "Child Count " + (childCount - 1));
        Debug.Log(myRoute.transform.name);

        if (!targetPlayer) GetTarget();
        SetDestination(myRoute.GetWaypoint(currentRouteIndex));

    }
    private void GetTarget()
    {
        if (targetPlayer == null)
        {
            SetDestination(myRoute.GetWaypoint(currentRouteIndex));
        }

        //if ((navMeshAgent.destination - transform.position).magnitude < 1 && targetPlayer == null)
        //{
        //    SetTargetPlayer(startGoal.transform);
        //    SetDestination(startGoal.transform.position);
        //}

        if (navMeshAgent.hasPath && navMeshAgent.remainingDistance < 2 && targetPlayer != null)
        {
            var ball = targetPlayer.GetComponent<FootBall>(); //todo this is all bad, attempting to destroy the football and set destination to the route 
            if (ball != null)
            {
                SetDestination(myRoute.GetWaypoint(currentRouteIndex));
            }
        }
        //DrawPath();
    }

    private void RunRoute()
    {

        if (myRoute != null)
        {
            if (!AtRouteCut()) return;

            if (timeSinceArrivedAtRouteCut > myRoute.routeCutDwellTime)
            {
                CycleRouteCut();
                nextPosition = GetCurrentRouteCut();
                navMeshAgent.destination = nextPosition;
                timeSinceArrivedAtRouteCut = 0;
                Debug.Log("start NavMeshAgent");

                if (!AtLastRouteCut()) return;
                wasAtLastCut = true;
                WatchQb();
                Debug.Log("set last cut true");
                return;

            }

            timeSinceArrivedAtRouteCut += Time.deltaTime;
        }
    }

    void WatchQb()
    {
        EnableNavMeshAgent();
        navMeshAgent.SetDestination(lastCutVector);
        anim.SetTrigger("WatchQBTrigger");
        transform.LookAt(qb.transform.position);

    }

    private void CycleRouteCut()
    {
        currentRouteIndex = myRoute.GetNextIndex(currentRouteIndex);
        if (currentRouteIndex == totalCuts)
        {
            navMeshAgent.autoBraking = true;
            navMeshAgent.stoppingDistance = 0f;
        }
    }

    private bool IsEndOfRoute()
    {
        var endOfRoute = (totalCuts == currentRouteIndex && (transform.position - navMeshAgent.destination).sqrMagnitude <= 1);
        Debug.Log("End of Route? = " + endOfRoute);
        return endOfRoute;
    }

    private bool AtRouteCut()
    {
        float distanceToCut = Vector3.Distance(transform.position, GetCurrentRouteCut());
        return distanceToCut < routeCutTolerance;


    }
    private bool AtLastRouteCut()
    {
        float distanceLastCut = Vector3.Distance(transform.position, (myRoute.transform.GetChild(totalCuts).position));
        return distanceLastCut < routeCutTolerance;
    }
    private Vector3 GetCurrentRouteCut()
    {
        return myRoute.GetWaypoint(currentRouteIndex);

    }
    private void StopNavMeshAgent()
    {
        if (!navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = true;
            //Debug.Log("NavAgent Stopped");
        }
    }
    private void FixedUpdate()
    {
        base.FixedUpdate();

    }


}



