using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class WR : FootBallAthlete
{
    // Use this for initialization
    IKControl iK;
    void Start()
    {

       
        gameManager = FindObjectOfType<GameManager>();
        qb = FindObjectOfType<QB>();
        defBacks = FindObjectsOfType<DB>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        iK = GetComponent<IKControl>();
        iK.isActive = false;
        startColor = materialRenderer.material.color;
        cameraFollow = FindObjectOfType<CameraFollow>();

        //target = startGoal.transform;
        //lr.material.color = LineColor;
        navMeshAgent.destination = transform.position;

        gameManager.onBallThrown += BallThrown;
        gameManager.hikeTheBall += HikeTheBall;

        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;

        //start goal set by inspector
        startGoal.SetWr(this);
    }
    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked) return;
        if (!isCatching) GetTarget();

        if (gameManager.isPass)
        {
            if (!isCatching)
            {
                if (materialRenderer.material.color != startColor) //

                    if (Input.anyKey)
                    {
                        BeginPass(); // todo this is a really bad way to call this function, should be called from QB
                    }
            }
        }
        if (gameManager.isRun)
        {
            canvas.transform.LookAt(Camera.main.transform);
            Transform blockTarget = GetClosestDB(defBacks);
            SetDestination(blockTarget.transform.position);
            if (targetDb.CanBePressed())
                StartCoroutine(DbBlock(targetDb));
            return;
        }
    }


    private void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
    }

    public void BallThrown(QB thrower, WR reciever,FootBall ball,Vector3 impactPos, float arcType, float power)
    {
        StartCoroutine("GetToImpactPos", impactPos);
               
        //todo move reciever to a through pass target
        if (reciever == this)
        {
            StartCoroutine("TracktheBall", ball);
            SetTarget(ball.transform);
            footBall = ball;
            isCatching = true;
        }
    }
    IEnumerator GetToImpactPos(Vector3 impact)
    {
        SetDestination(impact);

        yield return new WaitForSeconds(2); //todo this is a very bad way to do this

        ResetRoute();

    }
    IEnumerator TracktheBall(FootBall ball)
    {
        while ((transform.position - ball.transform.position).magnitude > 10f) //todo will have to create some forumla based on throwPower to trigger animations correctly
        {
            iK.isActive = true;
            iK.LookAtBall(ball);
            yield return new WaitForEndOfFrame();
        }
           
        anim.SetTrigger("CatchTrigger");
        StartCoroutine("CatchTheBall", ball);
    }
    IEnumerator CatchTheBall(FootBall ball)
    {
        gameManager.ChangeBallOwner(GameObject.FindGameObjectWithTag("Player"), gameObject);
        iK.rightHandObj = ball.transform;
        yield return new WaitForEndOfFrame();
        anim.SetBool("hasBall", true);
        iK.isActive = false;
    }

    public void ResetRoute() // Called from anim event
    {
        footBall = null;
        target = null;
        isCatching = false;
        StopAllCoroutines();
    }

    private void SetTarget(Transform _target)
    {
        target = _target;
    }

  
    public void SetDestination(Vector3 targetSetter)
    {
        Debug.Log(this + "Dest set " + targetSetter);
        navMeshAgent.SetDestination(targetSetter);
        //target = targetSetter;
    }

    private void GetTarget()
    {
        if (navMeshAgent.destination == transform.position && target == null)
        {
           SetDestination(startGoal.transform.position);
        }

        if (navMeshAgent.hasPath && navMeshAgent.remainingDistance < 2 && target != null)
        {
            var ball = target.GetComponent<FootBall>();
            if (ball != null)
            {
                Destroy(ball);
                SetDestination(startGoal.transform.position);
            }
        }
        //DrawPath();
    }

    private void BeginPass() //todo, move code to QB or game manager
    {
       
        //todo adjust throwPower dependent on distance and throw type
            if (Input.GetMouseButtonDown(0)) //Bullet Pass
            {
                
                passTarget = transform.position;
                qb.BeginThrowAnim(passTarget, this, 1.5f, 23f); //todo fix hardcoded variables, needs to be a measure of distance + qb throw power

            }

            if (Input.GetMouseButtonDown(1)) // Touch Pass
            {
               
                passTarget = transform.position;
                qb.BeginThrowAnim(passTarget, this, 2.3f, 20f);
            }

            if (Input.GetMouseButtonDown(2)) // Lob PASS
            {
                //Debug.Log("Pressed middle click.");
                passTarget = transform.position;
                qb.BeginThrowAnim(passTarget, this, 3.2f, 19.5f);
            }
        
       
    }
    IEnumerator DbBlock(DB db)
    {
        Debug.Log("Block DB");
        float pressTime = 1f; // 3 seconds you can change this 
        //to whatever you want
        float pressTimeNorm = 0;
        while (pressTimeNorm <= 1f)
        {
            isBlocking = true;
            pressTimeNorm += Time.deltaTime / pressTime;
            db.Press(pressTimeNorm);
            pressBar.fillAmount = pressTimeNorm;
            yield return new WaitForEndOfFrame();
        }
        db.ReleasePress();
        isBlocking = false;


    }
    Transform GetClosestDB(DB[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (DB potentialTarget in enemies)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
                targetDb = potentialTarget;
            }
        }

        return bestTarget;
    }

    internal void RaycastForward()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, fwd, 10))
            print("There is something in front of the object!");
    }

    void OnMouseEnter() 
    {
        materialRenderer.material.color = highlightColor;
    }

    void OnMouseDown()
    {
      

    }

    void OnMouseExit()
    {
        materialRenderer.material.color = startColor;
    }

    public void PrintPosition()
    {
        Debug.Log("WR position " + transform.position + "" +
                  "Nav Velocity " + navMeshAgent.velocity + "" +
                  "RB Velocity " + rb.velocity);
    }

  

    //void DrawPath()
    //{
        
    //    Vector3[] path = navMeshAgent.path.corners;
    //    if (path != null && path.Length > 1)
    //    {
    //        lr.positionCount = path.Length;
    //        for (int i = 0; i < path.Length; i++)
    //        {
    //            lr.SetPosition(i, path[i]);
    //        }
    //    }
    //}

    public bool CanBePressed()
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
        pressBar.fillAmount = pressTimeNorm;
        navMeshAgent.acceleration = 0f;
        navMeshAgent.speed = 0f;
    }

    public void ReleasePress()
    {
        canvas.enabled = !canvas.enabled;
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
    }


}
