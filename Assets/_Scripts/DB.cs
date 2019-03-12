using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class DB : FootBallAthlete
{
   //todo HOW ARE WE GOING TO HANDLE JUMP ANIMATIONS

    //todo DB State Machine
    
    // Use this for initialization
    void Start ()
    {
        gameManager = FindObjectOfType<GameManager>();
        wideRecievers = FindObjectsOfType<WR>();
        hbs = FindObjectsOfType<HB>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
        anim = GetComponent<Animator>();
        gameManager.hikeTheBall += HikeTheBall;
        gameManager.onBallThrown += BallThrown;
        gameManager.passAttempt += PassAttempt;
        CreateZone(); //todo only run if player is in zone
    }

    
    private void CreateZone()
    {
        zoneCenter = transform.position + new Vector3(0, 0, 5);
        //todo make zoneCenterGO move functions;
        zoneCenterGO = Instantiate(new GameObject(), zoneCenter, Quaternion.identity);
        zoneCenterGO.transform.name = transform.name + "ZoneObject";
        GameObject zoneObjectContainer = GameObject.FindGameObjectWithTag("ZoneObject");
        zoneCenterGO.transform.parent = zoneObjectContainer.transform;
        zoneCenterGO.transform.tag = "ZoneObject";
        zoneCenterGO.AddComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update () {
        if (!gameManager.isHiked)
            return;

        if (gameManager.isPass)
        {
            if (targetWr != null)
            {
                if (IsTargetInZone())
                {
                    navMeshAgent.SetDestination(targetWr.transform.position);
                }
                else
                {
                    targetWr = null;
                    target = null;
                }
            }

            if (target == null)
            {
                //todo this code will break zone coverage later, only targets WRs
                startTarget = GetClosestWr(wideRecievers);//todo this is unneccesary variables 
                SetTargetWr(startTarget);
                if (targetWr == null)
                {
                    Debug.Log("WR not found from DB set");
                    return;
                }

                if (targetWr.CanBePressed())
                    StartCoroutine(WrPress(targetWr));
            }
          

            if (isZone && !isPressing)
            {
                PlayZone();
            }
        }
        if (gameManager.isRun)
        {
            target = GetClosestHb(hbs);
            SetTargetHb(target);
        }
	}

    private void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");

    }

    private void BallThrown(QB thrower, WR reciever,FootBall ball, Vector3 impactPos, float arcType, float power, bool isComplete)
    {


    }
    private void PassAttempt(QB thrower, WR reciever,FootBall ball, float arcType, float power)
    {
        if (InVincintyOfPass(reciever))
        {
            if (arcType == 1.5f)
            {


               
                AniciptateThrow(thrower, reciever, ball, arcType, power);
            }

            if (arcType == 2.3f) { }
            if (arcType == 3.2f) { }
        }

    }

    private void AniciptateThrow(QB thrower, WR reciever, FootBall ball, float arcType, float power)

    {
        //todo this code is used twice now 
        Vector3 targetPos = reciever.transform.position;
        Vector3 diff = targetPos - transform.position;
        Vector3 diffGround = new Vector3(diff.x, 0f, diff.z);
        Vector3 fireVel, impactPos;
        Vector3 velocity = reciever.navMeshAgent.velocity;

        //FTS Calculations https://github.com/forrestthewoods/lib_fts/tree/master/projects/unity/ballistic_trajectory
        float gravity;

        if (Ballistics.solve_ballistic_arc_lateral(transform.position, power, targetPos + Vector3.up, velocity, arcType,
            out fireVel, out gravity, out impactPos))
        {
            //todo: get distance to impact pos and match it against DB position and speed, 
            //pass outcome of roll to the football call an onInterception or onBlock event
            //figure out how to add a second impluse to the thrown football in the case of a blocked pass


            if ((transform.position - impactPos).magnitude < 5 ) // todo create range variableStat
            {
                navMeshAgent.speed += power; // todo fix this terrible code, basically speeds up character to get in position
                navMeshAgent.SetDestination(impactPos);
                //Debug.Log("PassBlock");
                StartCoroutine("BlockPass", ball);
                ball.isComplete = false;
            }

        }
    }

    IEnumerator BlockPass(FootBall ball)
    {
        anim.SetTrigger("BlockPass");
        while ((transform.position - ball.transform.position).magnitude > 2.7)
        {
            //Debug.Log((transform.position - ball.transform.position).magnitude);
            yield return new WaitForEndOfFrame();
        }

        ball.BlockBallTrajectory();
    }

    bool InVincintyOfPass(WR wR)
    {
        float distToWr = Vector3.Distance(transform.position, wR.transform.position);
        if (distToWr <= 5) return true; //todo create coverage range variable
        else return false;
    }


    private bool IsTargetInZone()
    {
        if ((targetWr.transform.position - zoneCenterGO.transform.position).magnitude <= zoneSize) return true; //target is outside zone
        else return false;
    }

    private Transform GetClosestHb(HB[] HalfBacks)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (HB potentialTarget in HalfBacks)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
                targetHb = potentialTarget;
            }
        }
        return bestTarget;
    }

    IEnumerator WrPress(WR wr)
    {
        float pressTime = 1f; // 3 seconds you can change this 
        //to whatever you want
        float pressTimeNorm = 0;
        while (pressTimeNorm <= 1f)
        {
            isPressing = true;
            pressTimeNorm += Time.deltaTime / pressTime;
            wr.Press(pressTimeNorm);
            yield return new WaitForEndOfFrame();
        }
        wr.ReleasePress();
        isPressing = false;
    }
        
    private void SetTargetWr(Transform targetTransform)
    {
        navMeshAgent.SetDestination(targetTransform.position);
        target = targetTransform;
        targetWr = targetTransform.GetComponentInParent<WR>();
        //Debug.Log("Target Changed");
    }

    private void SetTargetHb(Transform targetTransform)
    {
        navMeshAgent.SetDestination(targetTransform.position);
        target = targetTransform;
        targetHb = targetTransform.GetComponentInParent<HB>();
        //Debug.Log("Target Changed");
    }

    private void SitInZone(Transform targetTransform)
    {
        CheckIncomingRoutes();
        target = targetTransform;
        targetWr = null;
        navMeshAgent.SetDestination(targetTransform.position);
        //Debug.Log("Target Changed");
    }

    private void CheckIncomingRoutes()
    {
        foreach (WR wR in wideRecievers)
        {
            wR.RaycastForward();
        }
    }

    Transform GetClosestWr(WR[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (WR potentialTarget in enemies)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
                targetWr = potentialTarget;
            }
        }

        return bestTarget;
    }

    Transform CheckZones(WR[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (WR potentialTarget in enemies)
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

    void PlayZone()
    {
        //todo access WR route to see if it will pass through zone and then move towards intercept point
        if (targetWr == null)
        {
           var possibleEnemy = CheckZones(wideRecievers);
           Vector3 wrZoneCntrDist = possibleEnemy.position - zoneCenter;
           //Debug.Log(wrZoneCntrDist.magnitude);
           if (wrZoneCntrDist.magnitude < zoneSize)
           {
                 SetTargetWr(possibleEnemy);
           }
        }
        else
        {
            Vector3 wrDistanceFromZoneCenter = targetWr.transform.position - zoneCenter;
            if (wrDistanceFromZoneCenter.magnitude > zoneSize)
            {
                if (target != zoneCenterGO.transform)
                {
                    SitInZone(zoneCenterGO.transform);
                }

            }
        }

    }

    public bool CanBePressed() //todo rename to block
    {
        if (!beenPressed)
        {
            beenPressed = true;
            return true;
        }

        return false;
    }

    public void Press(float pressTimeNorm)
    {
        //pressBar.fillAmount = pressTimeNorm;
        navMeshAgent.acceleration = 0f;
        navMeshAgent.speed = 0f;
    }

    public void ReleasePress()
    {
        //canvas.enabled = !canvas.enabled;
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
    }

    void OnDrawGizmos()
    {
        // Draw zone sphere 
        Gizmos.color = new Color(0, 0, 255, .5f);
        Gizmos.DrawWireSphere(zoneCenter, zoneSize);
    }
}
