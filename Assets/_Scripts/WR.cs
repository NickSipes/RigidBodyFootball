using System.Collections;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;

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

        gameManager.clearSelector += ClearSelector;
        gameManager.onBallThrown += BallThrown;
        gameManager.hikeTheBall += HikeTheBall;
        cameraRaycaster.onMouseOverWr += OnMouseOverWr;

        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
        navMeshAgent.destination = transform.position;

        //seeker = GetComponent<Seeker>();
        //ai = GetComponent<RichAI>();

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
    
    void Update()
    {

        if (!gameManager.isHiked)
        {
            navMeshAgent.isStopped = true;
            if (myRoute == null)
                {
                    GetRoute();
                }
            

            return;
        }
        if (gameManager.WhoHasBall() == this) return;
        if (isCatching) return;
        if (gameManager.isPass)
        {
            if(isAtLastCut) return;

            RunRoute();
            timeSinceArrivedAtRouteCut += Time.deltaTime;
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

    private void RunRoute()
    {
        
        if (myRoute != null)
        {
           
            if (AtRouteCut())
            {
                if (IsEndofRoute())
                {
                    navMeshAgent.acceleration = 0;
                    navMeshAgent.speed = 0;
                    isAtLastCut = true;
                    navMeshAgent.velocity = Vector3.zero;
                    navMeshAgent.isStopped = true;
                    navMeshAgent.enabled = false;
                    navMeshAgent.enabled = true;
                    //navMeshAgent.enabled = false;
                    Debug.Log("Stop nav agent");
                    return;
                }
                timeSinceArrivedAtRouteCut = 0;
                CycleRouteCut();
            }
          
            nextPosition = GetCurrentRouteCut();
        }

        if (timeSinceArrivedAtRouteCut > myRoute.routeCutDwellTime)
        {
            //ai.destination = nextPosition;
            //ai.SearchPath();
            navMeshAgent.destination = nextPosition;
            navMeshAgent.isStopped = false;
            Debug.Log("start navemesh agent");
        }
    }

    private void CycleRouteCut()
    {
        if (IsEndofRoute()) return;

        currentRouteIndex = myRoute.GetNextIndex(currentRouteIndex);
    }

    private bool IsEndofRoute()
    {
        return totalCuts == currentRouteIndex;
    }

    private bool AtRouteCut()
    {
        float distanceToCut = Vector3.Distance(transform.position, GetCurrentRouteCut());
        return distanceToCut < routeCutTolerance;
        
    }

    private Vector3 GetCurrentRouteCut()
    {
        return myRoute.GetWaypoint(currentRouteIndex);

    }

    private void GetRoute()
    {
        myRoute = routes[0];
        totalCuts = myRoute.transform.childCount - 1; //todo had to subtract 1 because arrays start at 0
        Debug.Log("total cuts " + totalCuts);
        Debug.Log(myRoute.transform.name);

    }


    void FixedUpdate()
    {
        base.FixedUpdate();
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
            navMeshAgent.ResetPath();
            navMeshAgent.enabled = false;
            gameManager.ChangeBallOwner(GameObject.FindGameObjectWithTag("Player"), gameObject);
            gameManager.isPassStarted = false;

        }
        ResetRoute();
    }


    public void ResetRoute() // Called from anim event
    {
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
        Debug.Log("BeBlocked DB");
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
