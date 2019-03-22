using System.Collections;
using UnityEngine;

public class WR : FootBallAthlete
{
    //todo rapid fire
    // Use this for initialization
    SphereCollider sphereCollider;
    private Color rayColor = Color.cyan;

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
        cameraRaycaster = CameraRaycaster.instance;
        //target = startGoal.transform;
        //lr.material.color = LineColor;
        navMeshAgent.destination = transform.position;

        gameManager.clearSelector += ClearSelector;
        gameManager.onBallThrown += BallThrown;
        gameManager.hikeTheBall += HikeTheBall;
        cameraRaycaster.onMouseOverWr += OnMouseOverWr;

        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;

        AddClickCollider();
        //start goal set by inspector
        startGoal.SetWr(this);
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

        if (!gameManager.isHiked) return;
        if (gameManager.WhoHasBall() == this) return;
        if (isCatching) return;

        if (gameManager.isRun)
        {
            canvas.transform.LookAt(Camera.main.transform);
            Transform blockTarget = GetClosestDB(defBacks);
            SetDestination(blockTarget.transform.position);
            if (targetDb.CanBePressed())
                StartCoroutine(DbBlock(targetDb));
            return;
        }
        if (!target) GetTarget();
        SetDestination(target.transform.position);


    }

    void FixedUpdate()
    {
        DrawRaycast();
      
    }

    internal void DrawRaycast()
    {

        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Debug.DrawRay(transform.position, forward, rayColor);
    }

    public void RayCastForward()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 100.0F);
        //if(hits.Length != 0)Debug.Log(hits.Length);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            //todo stuff

        }
    }


    private void HikeTheBall(bool wasHiked) //event
    {
        anim.SetTrigger("HikeTrigger");
    }

    public void BallThrown(QB thrower, WR reciever, FootBall ball, Vector3 impactPos, float arcType, float power, bool isComplete) //event
    {
        //todo move reciever to a through pass target
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

        }
        ResetRoute();
    }


    public void ResetRoute() // Called from anim event
    {
        footBall = null;
        target = null;
        isCatching = false;
        iK.isActive = false;
        StopAllCoroutines();
    }

    private void SetTarget(Transform _target)
    {
        target = _target;
    }

    public void SetDestination(Vector3 targetSetter)
    {
        //Debug.Log(this + "Dest set " + targetSetter);
        if (!navMeshAgent.enabled) navMeshAgent.enabled = true;
        navMeshAgent.SetDestination(targetSetter);
        //target = targetSetter;
    }

    private void GetTarget()
    {
        if(target == null)
        {
            SetTarget(startGoal.transform);
            SetDestination(startGoal.transform.position);
        }

        //if ((navMeshAgent.destination - transform.position).magnitude < 1 && target == null)
        //{
        //    SetTarget(startGoal.transform);
        //    SetDestination(startGoal.transform.position);
        //}

        if (navMeshAgent.hasPath && navMeshAgent.remainingDistance < 2 && target != null)
        {
            var ball = target.GetComponent<FootBall>(); //todo this is all bad, attempting to destroy the football and set destination to the route 
            if (ball != null)
            {
               SetDestination(startGoal.transform.position);
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
