
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.UIElements;
// ReSharper disable ArrangeTypeMemberModifiers
#pragma warning disable 108,114

public class WR : OffPlayer
{
    //todo rapid fire
    // Use this for initialization
    SphereCollider sphereCollider;

    void Start()
    {

        rayColor = Color.cyan;
        routes = FindObjectsOfType<Routes>();
        gameManager = FindObjectOfType<GameManager>();
        qb = FindObjectOfType<QB>();
        defBacks = FindObjectsOfType<DB>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        iK = GetComponent<IKControl>();
        iK.isActive = false;
        startColor = materialRenderer.material.color;
        cameraFollow = FindObjectOfType<CameraFollow>();
        cameraRaycaster = CameraRaycaster.instance;
        //targetPlayer = startGoal.transform;
        //lr.material.color = LineColor;
        navMeshAgent.destination = transform.position;

        gameManager.clearSelector += ClearSelector;
        gameManager.onBallThrown += BallThrown;
        gameManager.hikeTheBall += HikeTheBall;
        gameManager.offPlayChange += ChangeOffRoute;
        cameraRaycaster.onMouseOverWr += OnMouseOverWr;
        routeManager = FindObjectOfType<RouteManager>();

        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;

        AddClickCollider();
        //start goal set by inspector
    }

    private void AddClickCollider()
    {
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = 3f;//todo make inspector settable
        sphereCollider.isTrigger = true;
        sphereCollider.tag = "ClickCollider";
    }

    // Update is called once per frame
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void Update()
    {

        if (gameManager.WhoHasBall() == this) return;
        if (isCatching) return;

        if (!gameManager.isHiked)
        {
            if (gameManager.isRun) return;
            if (gameManager.isPass)
            {
                if (myRoute == null)
                {
                    GetRoute();
                }

                return;
            }
        }

        if (wasAtLastCut)
        {
            WatchQb();
            Debug.Log("last cut");
            return;
        }

        if (gameManager.isRun)
        {
            canvas.transform.LookAt(Camera.main.transform);
            Transform blockTarget = GetClosestDB(defBacks);
            isBlocker = true;
            targetDb = blockTarget.GetComponent<DB>();
            navMeshAgent.isStopped = false;
            SetDestination(blockTarget.transform.position);
            if (targetDb.CanBePressed())
                StartCoroutine("DbBlock", (targetDb));
            return;
        }

        if (gameManager.isPass)
        {

            if (IsEndOfRoute())
            {
                StopNavMeshAgent();
                Debug.Log("EndOfRoute Update");
                return;
            }

            if (!IsEndOfRoute()) RunRoute();

        }
    }

    private void StopNavMeshAgent()
    {
        if (!navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = true;
            //Debug.Log("NavAgent Stopped");
        }
    }

    void ChangeOffRoute(OffPlay offPlay)
    {
        if (gameManager.isRun)
        {
            if (myRoute != null) Destroy(myRoute);
            StopNavMeshAgent();
            return;
        }
        var wrName = this.name;
        switch (wrName)
        {
            case "WR1":
                routeSelection = offPlay.wrRoutes[0];
                break;
            case "WR2":
                routeSelection = offPlay.wrRoutes[1];
                break;
            case "WR3":
                routeSelection = offPlay.wrRoutes[2];
                break;
            case "WR4":
                routeSelection = offPlay.wrRoutes[3];
                break;
            default:
                routeSelection = offPlay.HbRoute[0];
                break;
        }
        Destroy(myRoute);
        GetRoute();
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

        navMeshAgent.autoBraking = true;
        navMeshAgent.stoppingDistance = 0f;
        if (!targetPlayer) GetTarget();
        SetDestination(myRoute.GetWaypoint(currentRouteIndex));

    }


    private void RunRoute()
    {

        if (myRoute != null)
        {
            if (!AtRouteCut()) return;

            if (timeSinceArrivedAtRouteCut > myRoute.routeCutDwellTime[0])
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

    private void HikeTheBall(bool wasHiked) //event
    {
        anim.SetTrigger("HikeTrigger");
    }

    public void BallThrown(QB thrower, WR reciever, FootBall ball, Vector3 impactPos, float arcType, float power, bool isComplete) //event
    {
        //todo move reciever to a through pass targetPlayer
        if (reciever == this)
        {
            StartCoroutine("GetToImpactPos", impactPos);
            StartCoroutine("TracktheBall", ball);

            SetTarget(ball.transform);
            footBall = ball;

            isCatching = true;
            if (isComplete) qb.userControl.enabled = false; //todo i dont like this, should be in the qb script?
        }
        else
        {
            StartCoroutine("GetToImpactPos", impactPos);
        }
    }


    IEnumerator GetToImpactPos(Vector3 impact)
    {
        SetDestination(impact);
        yield return new WaitForSeconds(2);
        //todo this is a very bad way to do this
        ResetRoute();
    }

    IEnumerator TracktheBall(FootBall ball)
    {
        while ((transform.position - ball.transform.position).magnitude > 10f) //todo will have to create some forumla based on throwPower to trigger animations correctly
        {
            iK.isActive = true;
            iK.LookAtBall(ball);
            //todo reciever doesnt get to ball well on second throw
            yield return new WaitForEndOfFrame();
        }

        anim.SetTrigger("CatchTrigger");
        StartCoroutine("CatchTheBall", ball);
    }
    IEnumerator CatchTheBall(FootBall ball)
    {
        iK.rightHandObj = ball.transform;

        if (ball.isComplete)
        {
            anim.SetBool("hasBall", true);
            while ((transform.position - ball.transform.position).magnitude > 2f)
            {
                cameraFollow.FollowBall(ball);
                yield return new WaitForEndOfFrame();
            }

            if (navMeshAgent.enabled == true)
            {
                navMeshAgent.ResetPath();
                navMeshAgent.enabled = false;
            }

            gameManager.ChangeBallOwner(GameObject.FindGameObjectWithTag("Player"), gameObject);
            gameManager.isPassStarted = false;

        }
        ResetRoute();
    }


    public void ResetRoute() // Called from anim event
    {
        myRoute = null;
        footBall = null;
        targetPlayer = null;
        isCatching = false;
        iK.isActive = false;
        StopAllCoroutines();
    }

    private void SetTarget(Transform _target)
    {
        targetPlayer = _target;
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

    void OnMouseOverWr(WR wr)
    {
        if (!gameManager.isPass) return;
        if (wr != this) { materialRenderer.material.color = startColor; return; }
        //Debug.Log("MouseOverWR");

        materialRenderer.material.color = highlightColor;
        gameManager.SetSelector(gameObject);
        //todo this is terrible, maybe a switch?  Plus should we really be calling a pass from the WR???
        int mouseButton;
        if (Input.GetMouseButtonDown(0)) { mouseButton = 0; BeginPass(mouseButton); }
        if (Input.GetMouseButtonDown(1)) { mouseButton = 1; BeginPass(mouseButton); }
        if (Input.GetMouseButtonDown(2)) { mouseButton = 2; BeginPass(mouseButton); }


        // todo bool canBePassedTo, then raycast OnMouseOverWr for QB to throw

    }

    private void BeginPass(int mouseButton) //todo, move code to QB or game manager
    {

        //todo adjust throwPower dependent on distance and throw type
        if (mouseButton == 0) //Bullet Pass
        {
            passTarget = transform.position;
            qb.BeginThrowAnim(passTarget, this, 1.5f, 23f); //todo fix hardcoded variables, needs to be a measure of distance + qb throw power
        }

        if (mouseButton == 1) // Touch Pass
        {
            passTarget = transform.position;
            qb.BeginThrowAnim(passTarget, this, 2.3f, 20f);
        }

        if (mouseButton == 2) // Lob PASS
        {
            //Debug.Log("Pressed middle click.");
            passTarget = transform.position;
            qb.BeginThrowAnim(passTarget, this, 3.2f, 19.5f);
        }


    }

    void ClearSelector(bool isClear)
    {
        if (materialRenderer.material.color != startColor)
            materialRenderer.material.color = startColor;
    }

    IEnumerator DbBlock(DB db)
    {
        Debug.Log("BeBlocked " + "");
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
        anim.SetTrigger("PressTrigger");
        pressBar.fillAmount = pressTimeNorm;
        navMeshAgent.acceleration = 0f;
        navMeshAgent.speed = 0f;
    }

    public void ReleasePress()
    {
        anim.SetTrigger("ReleaseTrigger");
        canvas.enabled = !canvas.enabled;
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
    }

    public void SetColor(Color color)
    {
        materialRenderer.material.color = color;
        //throw new System.NotImplementedException();
    }
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
