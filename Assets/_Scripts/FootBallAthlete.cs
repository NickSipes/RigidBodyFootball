using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

public class FootBallAthlete : MonoBehaviour
{
    //public RouteManager startGoal;
    //todo clean up inheritance and add setters and getters instead of public variables. Separate OffPlayers variables from DefPlayers variables
    public Renderer materialRenderer;
    [HideInInspector] public IKControl iK;
    [HideInInspector] public Terrain terrain;
    [HideInInspector] public QB qb;
    internal SphereCollider sphereCollider;

    [HideInInspector] public Color startColor;
    [HideInInspector] public Color rayColor;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public float navStartSpeed;
    [HideInInspector] public float navStartAccel;
    public Color highlightColor;


    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator anim;
    //public LineRenderer lr;

    [SerializeField] private float maxAngle = 35;
    [SerializeField] private float maxRadius = 1;

    [HideInInspector] public Vector3 passTarget;
    [HideInInspector] public bool beenPressed = false;
    [HideInInspector] public FootBall footBall;
    public Canvas canvas;
    public Image pressBar;
    [HideInInspector] public bool isHiked = false;
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public RouteManager routeManager;
    [HideInInspector] public bool isBlocking;
    [HideInInspector] public OffPlay offPlay;

    [HideInInspector] public PlayCall currentPlayCall;
    public Routes[] routes;
    public Routes myRoute;
    [HideInInspector] public int routeSelection;
    internal int totalCuts;
    public Vector3 lastCutVector;
    internal bool wasAtLastCut = false;
    internal int currentRouteIndex = 0;
    internal float routeCutTolerance = 1.5f;
    internal Vector3 routeStartPosition;

    internal float timeSinceArrivedAtRouteCut;
    internal Vector3 nextPosition;

    [HideInInspector] public WR[] wideRecievers;
    [HideInInspector] public DB[] defBacks;
    [HideInInspector] public HB[] hbs;
    [HideInInspector] public Oline[] oLine;
    [HideInInspector] public Dline[] dLine;
    [HideInInspector] public DefPlayer[] defPlayers;
    [HideInInspector] public OffPlayer[] offPlayers;
    [Range(5, 10)] public float defZoneSize; //todo should this be determined by the Zone? Maybe awareness checks

    [HideInInspector] public Zones myZone;
    [HideInInspector] public OffPlayer targetOffPlayer;
    [HideInInspector] public Transform transformTarget;

    [HideInInspector] public HB targetHb;
    [HideInInspector] public DB targetDb;

    [HideInInspector] public bool isMan;
    [HideInInspector] public bool isZone;

    [HideInInspector] public bool isPressing;

    [HideInInspector] public bool isCatching = false;

    [HideInInspector] public CameraFollow cameraFollow;

    [HideInInspector] public UserControl userControl;
    [HideInInspector] public CameraRaycaster cameraRaycaster;
    [HideInInspector] public bool isSelected;

    internal virtual void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        routes = FindObjectsOfType<Routes>();
        routeManager = FindObjectOfType<RouteManager>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        cameraRaycaster = CameraRaycaster.instance;
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        qb = FindObjectOfType<QB>();
        hbs = FindObjectsOfType<HB>();
        defBacks = FindObjectsOfType<DB>();
        dLine = FindObjectsOfType<Dline>();
        oLine = FindObjectsOfType<Oline>();
        wideRecievers = FindObjectsOfType<WR>();
        defPlayers = FindObjectsOfType<DefPlayer>();
        offPlayers = FindObjectsOfType<OffPlayer>();
        //lr.material.color = LineColor;

        AddIk();
        startColor = materialRenderer.material.color;
        navMeshAgent.destination = transform.position;
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
        cameraRaycaster.OnMouseOverOffPlayer += OnMouseOverOffPlayer;
        gameManager.clearSelector += ClearSelector;

    }

    internal virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {
        //Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        //Debug.DrawRay(transform.position, forward, rayColor);
        Vector3 angleFov2 = Quaternion.AngleAxis(maxAngle, transform.up) * transform.forward;
        Vector3 angleFov1 = Quaternion.AngleAxis(-maxAngle, transform.up) * transform.forward;
        Debug.DrawRay(transform.position, angleFov2, rayColor);
        Debug.DrawRay(transform.position, angleFov1, rayColor);
        RayForward();

        //foreach (var offPlayer in offPlayers)
        //{
        //    //var lineColor = defPlayer.startColor;
        //    Debug.DrawLine(this.transform.position, offPlayer.transform.position, offPlayer.startColor);
        //}
    }

    private void AddIk()
    {
        iK = gameObject.AddComponent<IKControl>();
        iK.isActive = false;
    }

    //the football field
    private void GetTerrain()
    {
        if (terrain == null)
        {
            terrain = FindObjectOfType<Terrain>();
        }
    }

    public void SetUserControl()
    {
        tag = "Player";
        rb = GetComponent<Rigidbody>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        userControl = GetComponent<UserControl>();
        userControl.enabled = true; //todo create an method to disable user control on all other players
        rb.isKinematic = false;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z);
        navMeshAgent.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (this is OffPlayer) return;
        if (!terrain) GetTerrain();
        var collGo = collision.gameObject;
        if (collGo.transform == terrain.transform) return;

        //Debug.Log("Collision " + collGo.name + " and " + name);
        ContactPoint contacts = collision.contacts[0];
        Debug.DrawLine(transform.position, contacts.point);
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }

        if (gameManager.ballOwner == null) return;
        if (collGo == gameManager.ballOwner.gameObject)
        {
            //todo check if player is facing ballOwner is targetOffPlayer in 'tackle angle'

            FootBallAthlete playerType = collGo.GetComponent<FootBallAthlete>();
            if (playerType is OffPlayer)
            {
                Tackle(GameManager.instance.ballOwner);

            }
        }
        if (collision.relativeVelocity.magnitude > 2)
        {
            Debug.Log("Collision magnitude");
        }
    }

    internal void Tackle(FootBallAthlete instanceBallOwner)
    {
        //need off vs def check
        //anim.SetTrigger("TackleTrigger");
        instanceBallOwner.StartCoroutine("BeTackled");
    }

    internal IEnumerator BeTackled()
    {
        materialRenderer.material.color = Color.red;
        yield return new WaitForSeconds(.5f);

        if (gameManager.isPassStarted == false && !gameManager.isRapidfire) gameManager.ResetScene();

        materialRenderer.material.color = startColor;
    }

    internal void RayForward()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.forward, 100.0F);
        //if(hits.Length != 0)Debug.Log(hits.Length);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.isTrigger)
            {
                Transform zoneObject = hit.collider.transform;
                if (zoneObject)
                {
                    //todo something with this info about incoming routes
                }
            }

        }
    }

    internal void OnMouseOverOffPlayer(OffPlayer offPlayer)
    {
        //Debug.Log("Mouse over " + offPlayer.transform.name);
        if (!gameManager.isPass) return;

        //Debug.Log("MouseOverWR");
        if (offPlayer != this) return;
        materialRenderer.material.color = highlightColor;
        gameManager.SetSelector(gameObject);
        //todo this is terrible, maybe a switch?  Plus should we really be calling a pass from the WR???
        int mouseButton;
        if (Input.GetMouseButtonDown(0)) { mouseButton = 0; BeginPass(mouseButton, offPlayer); }
        if (Input.GetMouseButtonDown(1)) { mouseButton = 1; BeginPass(mouseButton, offPlayer); }
        if (Input.GetMouseButtonDown(2)) { mouseButton = 2; BeginPass(mouseButton, offPlayer); }


        // todo bool canBePassedTo, then RayCast OnMouseOverWr for QB to throw

    }
    internal void BeginPass(int mouseButton, OffPlayer receiver) //todo, move code to QB or game manager
    {

        //todo adjust throwPower dependent on distance and throw type
        if (mouseButton == 0) //Bullet Pass
        {
            passTarget = transform.position;
            qb.BeginThrowAnim(passTarget, receiver, 1.5f, 23f); //todo fix hardcoded variables, needs to be a measure of distance + qb throw power
        }

        if (mouseButton == 1) // Touch Pass
        {
            passTarget = transform.position;
            qb.BeginThrowAnim(passTarget, receiver, 2.3f, 20f);
        }

        if (mouseButton == 2) // Lob PASS
        {
            //Debug.Log("Pressed middle click.");
            passTarget = transform.position;
            qb.BeginThrowAnim(passTarget, receiver, 3.2f, 19.5f);
        }

    }
    internal void SetDestination(Vector3 dest)
    {
        //if(this.name == "DB") Debug.Log(this.name + " " +"set dest " + dest);
        EnableNavMeshAgent();
        navMeshAgent.SetDestination(dest);
    }
    internal void EnableNavMeshAgent()
    {
        if (!navMeshAgent.enabled)
            navMeshAgent.enabled = true;
        if (navMeshAgent.isStopped)
            navMeshAgent.isStopped = false;
    }
    internal Transform GetClosestHb(HB[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (HB potentialTarget in enemies)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (!(dSqrToTarget < closestDistanceSqr)) continue;

            closestDistanceSqr = dSqrToTarget;
            bestTarget = potentialTarget.transform;
        }

        return bestTarget;
    }
    internal Transform GetClosestWr(WR[] enemies)
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
    internal Transform GetClosestDB(DB[] enemies)
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
    internal Transform GetClosestDefPlayer(DefPlayer[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (DefPlayer potentialTarget in enemies)
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
    internal OffPlayer GetClosestOffPlayer(OffPlayer[] enemies, Vector3 startPos)
    {
        OffPlayer bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (OffPlayer potentialTarget in enemies)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - startPos;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (!(dSqrToTarget < closestDistanceSqr)) continue;

            closestDistanceSqr = dSqrToTarget;
            bestTarget = potentialTarget;
        }

        return bestTarget;
    }

    internal void AddClickCollider()
    {
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = 3f;//todo make inspector settable
        sphereCollider.isTrigger = true;
        sphereCollider.tag = "ClickCollider";
    }

    internal void ClearSelector(bool isClear)
    {
        if (materialRenderer.material.color != startColor)
            materialRenderer.material.color = startColor;
    }
    internal void StopNavMeshAgent()
    {
        if (!navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = true;
            //Debug.Log("NavAgent Stopped");
        }
    }
}

public class OffPlayer : FootBallAthlete
{
    public float blockRange = 3;
    public float blockCoolDown = 1f;
    public bool canBlock = true;
    internal bool isBlocker;
    public bool isReciever;
    List<DefPlayer> blockList;
    internal OffPlay currentOffPlay;

    internal override void Start()
    {
        base.Start();
        gameManager.onBallThrown += BallThrown;
        gameManager.hikeTheBall += HikeTheBall;
        gameManager.offPlayChange += OffPlayChange;
        gameManager.offFlipPlay += FlipOffPlay;
        startColor = materialRenderer.material.color;

    }
    internal override void Update()
    {

    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void HikeTheBall(bool wasHiked) //event
    {
        anim.SetTrigger("HikeTrigger");
    }

    internal void OffPlayChange(OffPlay offPlay)
    {
        currentOffPlay = offPlay;
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
                case "WR1":
                    routeSelection = offPlay.wrRoutes[0];
                    SetStartPosition(offPlay.formationTransforms[0].position);
                    break;
                case "WR2":
                    routeSelection = offPlay.wrRoutes[1];
                    SetStartPosition(offPlay.formationTransforms[1].position);
                    break;
                case "WR3":
                    routeSelection = offPlay.wrRoutes[2];
                    SetStartPosition(offPlay.formationTransforms[2].position);
                    break;
                case "WR4":
                    routeSelection = offPlay.wrRoutes[3];
                    break;
                case "TE1":
                    routeSelection = offPlay.teRoute[0];
                    isBlocker = offPlay.isSkillPlayerBlock[0];
                    SetStartPosition(offPlay.formationTransforms[10].position);
                    break;
                case "TE2":
                    routeSelection = offPlay.teRoute[1];
                    isBlocker = offPlay.isSkillPlayerBlock[1];
                    break;
                case "TE3":
                    routeSelection = offPlay.teRoute[2];
                    isBlocker = offPlay.isSkillPlayerBlock[2];
                    break;
                case "HB1":
                    routeSelection = offPlay.hbRoute[0];
                    isBlocker = offPlay.isSkillPlayerBlock[0];
                    SetStartPosition(offPlay.formationTransforms[9].position);
                    break;
                case "HB2":
                    routeSelection = offPlay.hbRoute[1];
                    isBlocker = offPlay.isSkillPlayerBlock[1];
                    break;
                case "FB":
                    //todo assign fullback route stuff
                    break;
                case "Center":
                    SetStartPosition(offPlay.formationTransforms[3].position);
                    isBlocker = true;
                    break;
                case "GuardR":
                    SetStartPosition(offPlay.formationTransforms[4].position);
                    isBlocker = true;
                    break;
                case "GuardL":
                    SetStartPosition(offPlay.formationTransforms[5].position);
                    isBlocker = true;
                    break;
                case "TackleL":
                    SetStartPosition(offPlay.formationTransforms[6].position);
                    isBlocker = true;
                    break;
                case "TackleR":
                    SetStartPosition(offPlay.formationTransforms[7].position);
                    isBlocker = true;
                    break;
                case "QB":
                    navMeshAgent.speed = 4;
                    SetStartPosition(offPlay.formationTransforms[8].position);
                    isBlocker = true;
                    break;
                default:
                    //todo default stuff
                    break;
            }
            currentPlayCall = offPlay;
            if (!isBlocker) isReciever = true;
            Destroy(myRoute);
            GetRoute(routeSelection);
        }
    }
    internal void DefShedBlock(FootBallAthlete brokeBlock)
    {
        //todo check assignment
        if (isReciever) return;

        isBlocker = true;
        BlockProtection();
    }
    internal void FlipOffPlay(OffPlay offPlay)
    {
        var number = this.name;
        switch (number)
        {
            case "WR1":
                routeSelection = offPlay.wrRoutes[0];
                FlipStartPosition(0);
                break;
            case "WR2":
                routeSelection = offPlay.wrRoutes[1];
                FlipStartPosition(1);
                break;
            case "WR3":
                routeSelection = offPlay.wrRoutes[2];
                FlipStartPosition(2);
                break;
            case "WR4":
                routeSelection = offPlay.wrRoutes[3];
                break;
            case "TE1":
                routeSelection = offPlay.teRoute[0];
                isBlocker = offPlay.isSkillPlayerBlock[0];
                FlipStartPosition(10); ;
                break;
            case "TE2":
                routeSelection = offPlay.teRoute[1];
                isBlocker = offPlay.isSkillPlayerBlock[1];
                break;
            case "TE3":
                routeSelection = offPlay.teRoute[2];
                isBlocker = offPlay.isSkillPlayerBlock[2];
                break;
            case "HB1":
                routeSelection = offPlay.hbRoute[0];
                isBlocker = offPlay.isSkillPlayerBlock[0];
                FlipStartPosition(9);
                break;
            case "HB2":
                routeSelection = offPlay.hbRoute[1];
                isBlocker = offPlay.isSkillPlayerBlock[1];
                break;
            case "FB":
                //todo assign fullback route stuff
                break;
        }
    }

    private void FlipStartPosition(int i)
    {
        var currentPlay = currentOffPlay.formationTransforms[i];
        var xPos = -(currentPlay.position.x);
        var yPos = currentPlay.position.y;
        var zPos = currentPlay.position.z;
        currentPlay.transform.position = new Vector3(xPos, yPos, zPos);
        myRoute.transform.position = new Vector3(xPos, yPos, zPos);
        foreach (var routeCut in myRoute.transform.GetComponentsInChildren<Transform>())
        {
            var xPosCut = -(routeCut.position.x);
            var yPosCut = routeCut.position.y;
            var zPosCut = routeCut.position.z;
            routeCut.transform.position = new Vector3(xPosCut, yPosCut, zPosCut);
        }

        lastCutVector = myRoute.routeCuts.Last().position;
        SetDestination(myRoute.GetWaypoint(currentRouteIndex));
        StartCoroutine(FaceLOS());
    }

    internal void SetStartPosition(Vector3 position)
    {
        routeStartPosition = position + new Vector3(0, 0, gameManager.lineOfScrimmage.transform.position.z);
        //anim.SetTrigger("HuddleTrigger");
    }

    internal void GetRoute(int routeSelector)
    {
        myRoute = Instantiate(routeManager.allRoutes[routeSelector], routeStartPosition, gameManager.lineOfScrimmage.transform.rotation).GetComponent<Routes>(); //todo get route index selection
        myRoute.transform.name = routeManager.allRoutes[routeSelector].name;
        myRoute.transform.SetParent(routeManager.transform);
        var childCount = myRoute.transform.childCount;
        totalCuts = childCount - 1; //todo had to subtract 1 because arrays start at 0
        lastCutVector = myRoute.transform.GetChild(totalCuts).position;
        //Debug.Log("total cuts " + totalCuts + "Child Count " + (childCount - 1));
        //Debug.Log(myRoute.transform.name);
        if (!transformTarget) GetTarget();
        SetDestination(myRoute.GetWaypoint(currentRouteIndex));
        StartCoroutine(FaceLOS());
    }
    internal IEnumerator FaceLOS()
    {
        while (gameManager.isHiked == false)
        {
            transform.LookAt(transform.position + new Vector3(0, 0, 1));
            yield return new WaitForEndOfFrame();
        }
    }
    internal void RunRoute()
    {
        if (myRoute == null) return;

        if (!AtRouteCut()) return;

        if (timeSinceArrivedAtRouteCut > myRoute.routeCutDwellTime[0])
        {
            CycleRouteCut();
            nextPosition = GetCurrentRouteCut();
            navMeshAgent.destination = nextPosition;
            timeSinceArrivedAtRouteCut = 0;
            //Debug.Log("start NavMeshAgent");
            if (!AtLastRouteCut()) return;
            wasAtLastCut = true;
            WatchQb();
            //Debug.Log("set last cut true");
            return;
        }
        timeSinceArrivedAtRouteCut += Time.deltaTime;
    }


    internal void WatchQb()
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

    internal bool IsEndOfRoute()
    {
        var endOfRoute = (totalCuts == currentRouteIndex && (transform.position - navMeshAgent.destination).sqrMagnitude <= 1);
        //Debug.Log("End of Route? = " + endOfRoute);
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


    internal void BallThrown(QB thrower, OffPlayer reciever, FootBall ball, Vector3 impactPos, float arcType, float power, bool isComplete) //event
    {
        //todo move receiver to a through pass targetOffPlayer
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
            StartCoroutine("GetToBlockPosition", impactPos);
        }
    }

    private IEnumerator GetToBlockPosition()
    {
        //todo track ball carrier position and determine blocking position

        yield return new WaitForSeconds(1);
    }

    internal bool CanBePressed()
    {
        if (!beenPressed)
        {
            beenPressed = true;
            return true;
        }
        return false;
    }

    internal void Press(float pressTimeNorm)
    {
        canvas.enabled = true;
        anim.SetTrigger("PressTrigger");
        pressBar.fillAmount = pressTimeNorm;
        navMeshAgent.acceleration = 0f;
        navMeshAgent.speed = 0f;
    }

    internal void ReleasePress()
    {
        anim.SetTrigger("ReleaseTrigger");
        canvas.enabled = !canvas.enabled;
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
    }

    internal void SetColor(Color color)
    {
        materialRenderer.material.color = color;
        //throw new System.NotImplementedException();
    }

    internal void BlockProtection() //todo consolidate duplicate code
    {
        if (!canBlock) return;
        ;
        //todo change code to be moving forward with blocks, get to the second level after shedding first defender
        if (transformTarget == null)
        {
            //sudo get positions of all other blockers
            var blockTargets = gameManager.defPlayers;
            var bestBlockTarget = GetClosestDefPlayer(blockTargets);
            if ((bestBlockTarget.transform.position - transform.position).magnitude < blockRange)


                //todo compare def players who are engaged in a block, double-team or get down-field
                transformTarget = GetClosestDefPlayer(blockTargets);
        }
        if (transformTarget == null) return;
        Vector3 directionToTarget = transformTarget.position - transform.position;
        transform.LookAt(transformTarget);

        if (gameManager.isRun) SetTargetDlineRun(transformTarget);
        if (gameManager.isPass) SetTargetDline(transformTarget);

        float dSqrToTarget = directionToTarget.sqrMagnitude;
        if (dSqrToTarget < 3) //todo setup block range variable
        {
            var defPlayer = transformTarget.GetComponent<DefPlayer>();
            if (!isBlocking) StartCoroutine(BlockTarget(transformTarget));
        }
    }


    private IEnumerator GetToImpactPos(Vector3 impact)
    {
        SetDestination(impact);
        yield return new WaitForSeconds(2);
        //todo this is a very bad way to do this
        ResetRoute();
    }

    private IEnumerator TracktheBall(FootBall ball)
    {
        while ((transform.position - ball.transform.position).magnitude > 10f) //todo will have to create some forumla based on throwPower to trigger animations correctly
        {
            iK.isActive = true;
            iK.LookAtBall(ball);
            //todo receiver doesnt get to ball well on second throw
            yield return new WaitForEndOfFrame();
        }

        anim.SetTrigger("CatchTrigger");
        StartCoroutine("CatchTheBall", ball);
    }

    private IEnumerator CatchTheBall(FootBall ball)
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
        transformTarget = null;
        isCatching = false;
        iK.isActive = false;
        StopAllCoroutines();
    }

    private void SetTarget(Transform _target)
    {
        transformTarget = _target;
    }

    private void GetTarget()
    {
        if (transformTarget == null)
        {
            SetDestination(myRoute.GetWaypoint(currentRouteIndex));
        }

        //if ((navMeshAgent.destination - transform.position).magnitude < 1 && targetOffPlayer == null)
        //{
        //    SetTargetPlayer(startGoal.transform);
        //    SetDestination(startGoal.transform.position);
        //}

        if (navMeshAgent.hasPath && navMeshAgent.remainingDistance < 2 && transformTarget != null)
        {
            var ball = transformTarget
                .GetComponent<FootBall>(); //todo this is all bad, attempting to destroy the football and set destination to the route 
            if (ball != null)
            {
                SetDestination(myRoute.GetWaypoint(currentRouteIndex));
            }
        }
    }


    private void SetTargetDlineRun(Transform target) //todo collapse into single function
    {
        var ballOwner = gameManager.ballOwner;
        if (ballOwner != null)
        {
            SetDestination(ballOwner.transform.position + (target.position - ballOwner.transform.position) / 2); // 
        }
        else
        {
            SetDestination(target.position);
        }
    }

    public void SetTargetDline(Transform target)
    {
        //if (isBlocking) return;
        SetDestination(qb.transform.position +
                       (target.position - qb.transform.position) /
                       2); // todo centralize ball carrier, access ballcarrier instead of hard coded transform
    }

    private IEnumerator BlockTarget(Transform target)
    {
        //todo 
        var defPlayer = target.GetComponent<DefPlayer>();

        if (defPlayer.blockPlayers.Contains(this)) yield break;

        float blockTime = 1f; //todo make public variable
        float blockTimeNorm = 0;
        while (blockTimeNorm <= 1f) //todo this counter is ugly and needs to be better
        {
            isBlocking = true;
            blockTimeNorm += Time.deltaTime / blockTime;
            defPlayer.BeBlocked(blockTimeNorm, this);
            yield return new WaitForEndOfFrame();
        }

        defPlayer.ReleaseBlock(this);
        transformTarget = null;
        isBlocking = false;
        canBlock = false;
        StartCoroutine(BlockCoolDown());
    }

    private IEnumerator BlockCoolDown()
    {
        yield return new WaitForSeconds(blockCoolDown);
        canBlock = true;
    }

}

public class DefPlayer : FootBallAthlete
{
    [HideInInspector] public bool isBlocked;
    public bool wasBlocked = false;
    [SerializeField] internal TextMeshPro blockText;
    internal float blockCooldown = 2f;
    internal List<OffPlayer> blockPlayers = new List<OffPlayer>();
    internal DefPlay defPlay;
    [SerializeField] internal int zoneLayer = 8;
    private bool isDeepDefender;
    internal bool isRusher;
    internal bool isBackingOff = false;
    internal bool isWrIncoming = false;
    internal bool isBlockingPass = false;
    internal bool inPressPos = false;
    public Zones.ZoneType zoneType;
    [SerializeField] internal OffPlayer startOffPlayer;
    private bool isPlayingDeep = false;

    //todo stats
    private float awarenessBoost = 3f;

    internal override void Start()
    {
        base.Start();
        AddEvents();
    }

    internal override void Update()
    {
        base.Update();
        if (gameManager.currentOffPlay == null) return;
        if (!gameManager.isHiked) return; 

        if (gameManager.isRun)
        {
            if (isBlocked) return;
            PlayReact();

        }
        if (!gameManager.isPass) return;

        if (isRusher) return;
        if (!isZone) return;

        if (isBlockingPass) return;
        if (isPressing) return;
        if (isBackingOff) return;

        if (isDeepDefender)
        {
            if (targetOffPlayer)
            {
                if ((targetOffPlayer.transform.position - myZone.zoneCenter).sqrMagnitude > myZone.zoneSize)
                {
                    targetOffPlayer = null;
                }
            }
            if (isPlayingDeep) return;
            StayDeep();
            return;
        }

        if (targetOffPlayer != null)
        {
            if (IsTargetCovered(targetOffPlayer))
            {

            }
            if (IsTargetInZone(targetOffPlayer.transform))
            {
                SetTargetOffPlayer(targetOffPlayer.transform);
                return;
            }
            else
            {
                //Debug.Log("targetOffPlayer out of myZone");
                targetOffPlayer = null;
            }
        }

        PlayZone();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    internal void AddEvents()
    {
        gameManager.ballOwnerChange += BallOwnerChange;
        gameManager.offPlayChange += OffPlayChange;
        gameManager.defPlayChange += DefPlayChange;
    }

    private void StayDeep()
    {
        //todo figure out side of field, area zone covers, check distance and direction, anticipate routes. Move back 

        if (targetOffPlayer) return;

        var deepestDefender = GetOffPlayerDownField();
        if (deepestDefender.transform.position.z < gameManager.lineOfScrimmage.transform.position.z + 5f) return;

        OffPlayer bestOffPlayer = null;
        float lastBest = 0f;
        foreach (OffPlayer offPlayer in offPlayers)
        {
            if (!offPlayer.isReciever) continue;

            //if(offPlayer.transform.position.z < myZone.zoneSize + myZone.zoneCenter.z + awarenessBoost)continue;


            if (myZone.zoneCenter.x < 0) //Left side
            {
                if (offPlayer.transform.position.x < 0 && offPlayer.transform.position.x < lastBest)
                {
                    lastBest = offPlayer.transform.position.x;
                    bestOffPlayer = offPlayer;
                    //Debug.Log("BestOff " + bestOffPlayer.name);
                }
            }

            if (myZone.zoneCenter.x > 0) //Right side
            {
                if (offPlayer.transform.position.x > 0 && offPlayer.transform.position.x > lastBest)
                {
                    lastBest = offPlayer.transform.position.x;
                    bestOffPlayer = offPlayer;
                    //Debug.Log("BestOff " + bestOffPlayer.name);
                }
            }

        }

        if (bestOffPlayer == null)
        {
            SetDestination(myZone.zoneCenter);
            return;
        }

        StartCoroutine(PlayDeepBall(bestOffPlayer));

    }

    private OffPlayer GetOffPlayerDownField()
    {
        OffPlayer bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (OffPlayer potentialTarget in offPlayers)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - transform.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (!(dSqrToTarget < closestDistanceSqr)) continue;

            closestDistanceSqr = dSqrToTarget;
            bestTarget = potentialTarget;
        }

        return bestTarget;
    }


    IEnumerator PlayDeepBall(OffPlayer offPlayer)
    {
        while (offPlayer.transform.position.z < myZone.zoneCenter.z)
        {
            isPlayingDeep = true;
            SetDestination(offPlayer.transform.position + new Vector3(0, 0, 5 + awarenessBoost));
            yield return new WaitForEndOfFrame();
        }

        isPlayingDeep = false;
    }

    private bool IsTargetCovered(OffPlayer offPlayer)
    {
        foreach (var defPlayer in defPlayers)
        {
            if (defPlayer.targetOffPlayer == offPlayer)
            {
                return true;
            }
        }

        return false;
    }

    public void Press(float pressTimeNorm)
    {
        //pressBar.fillAmount = pressTimeNorm;
        DisableNavMeshAgent();
        navMeshAgent.acceleration = 0f;
        navMeshAgent.speed = 0f;
    }

    internal void OffPlayChange(OffPlay offPlayChange)
    {
        
    }

    internal void DefPlayChange(DefPlay defPlayChange)
    {
        GetPlayCall();
        MoveToStart();
    }


    private void GetPlayCall()
    {
        defPlay = gameManager.currentDefPlay;
        myZone = defPlay.GetJob(this);
        var zoneObjects = GameObject.Find("ZoneObjects");
        myZone.transform.SetParent(zoneObjects.transform);
        zoneType = myZone.type;
        if (zoneType == Zones.ZoneType.Seam ||
            zoneType == Zones.ZoneType.Flat ||
            zoneType == Zones.ZoneType.Curl ||
            zoneType == Zones.ZoneType.DeepHalf ||
            zoneType == Zones.ZoneType.DeepThird)
        {
            isZone = true;
        }

        if (zoneType == Zones.ZoneType.DeepHalf ||
            zoneType == Zones.ZoneType.DeepThird)
        {
            isDeepDefender = true;
        }

        if (zoneType == Zones.ZoneType.Rush)
        {
            isRusher = true;
        }
        targetOffPlayer = GetClosestOffPlayer(offPlayers, myZone.zoneCenter);
    }

    private void MoveToStart()
    {
        if (myZone.isPress)
        {
            var wideReceiver = GetClosestWr(FindObjectsOfType<WR>());
            SetDestination(wideReceiver.transform.position + new Vector3(0,0,2));
        }
        SetDestination(myZone.transform.position);
    }

    IEnumerator FaceLineOfScrim()
    {
        while ((transform.position - myZone.zoneCenter).sqrMagnitude > 1)
        {
            transform.LookAt(transformTarget ? transformTarget : gameManager.lineOfScrimmage.transform);
            yield return new WaitForEndOfFrame();
        }
    }

    internal void PlayReact()
    {
        var reactTime = 0f;
        while (reactTime < 1.5f)
        {
            reactTime += Time.deltaTime;
        }
        transformTarget = gameManager.ballOwner.transform;
        SetTargetOffPlayer(transformTarget);
    }

    public void ReleasePress()
    {
        canvas.enabled = !canvas.enabled;
        EnableNavMeshAgent();
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
    }

    internal void PlayZone()
    {
        //todo access WR route to see if it will pass through myZone and then move towards intercept point
        if (targetOffPlayer == null)
        {
            //has return if enemy set
            if (CheckZone()) return;
        }
        SetDestination(base.myZone.zoneCenter);

    }

    private bool CheckZone()
    {
        if (this.GetComponent<LineBacker>())
        {
            wideRecievers = FindObjectsOfType<WR>();
        }
        var possibleEnemy = GetClosestWr(wideRecievers);
        Vector3 wrZoneCntrDist = possibleEnemy.position - base.myZone.transform.position;
        //Debug.Log(wrZoneCntrDist.magnitude);
        if (wrZoneCntrDist.magnitude < base.myZone.zoneSize)
        {
            //todo this will break with multiple recievers, need to determine coverage responsibility
            SetTargetOffPlayer(possibleEnemy);
            return true;
        }

        if (isDeepDefender)
        {

        }

        foreach (var receiver in wideRecievers)
        {
            RaycastHit[] hits = Physics.RaycastAll(receiver.transform.position, receiver.transform.forward, 100.0F);
            //if(hits.Length != 0)Debug.Log(hits.Length);

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider.tag == "ZoneObject")
                {
                    Zones zoneObject = hit.collider.gameObject.GetComponent<Zones>();

                    if (zoneObject == base.myZone)
                    {
                        //Maybe Method
                        SetDestination(hit.point);
                        Debug.DrawLine(transform.position, hit.point, Color.red);
                        return true;

                    }
                }
            }
        }
        return false;
    }

    public void SetTargetPlayer(Transform targetSetter)
    {
        EnableNavMeshAgent();
        navMeshAgent.SetDestination(targetSetter.position);
        transformTarget = targetSetter;
    }

    internal void SetTargetOffPlayer(Transform targetTransform)
    {
        EnableNavMeshAgent();
        SetDestination(targetTransform.position);
        targetOffPlayer = targetTransform.GetComponentInParent<OffPlayer>();
        //Debug.Log("Target Changed");
    }

    internal IEnumerator WrPress(OffPlayer offPlayer)
    {
        isPressing = true;
        float pressTime = .5f;
        anim.SetTrigger("PressTrigger");
        SetDestination(offPlayer.transform.position + transform.forward);
        float pressTimeNorm = 0;
        //Vector3 dir = (offPlayer.transform.position - transform.position).normalized * pressTime;
        //rb.velocity = dir;
        while (pressTimeNorm <= 1f)
        {
            StayInfront(offPlayer);
            pressTimeNorm += Time.deltaTime / pressTime;
            offPlayer.Press(pressTimeNorm);
            yield return new WaitForEndOfFrame();
        }

        anim.SetTrigger("ReleaseTrigger");
        StartCoroutine(BackOffPress(offPlayer));

    }

    private IEnumerator BackOffPress(OffPlayer offPlayer)
    {
        //read receiver route, move backwards, release receiver to new defender, moves towards next receiver

        offPlayer.ReleasePress();
        isPressing = false;
        StartCoroutine(TurnTowardsLOS());
        var backOffTime = 0f;
        while (backOffTime < 1)
        {
            //Vector3 dir = targetOffPlayer.position - transform.position;

            isBackingOff = true;
            //Debug.Log("isBackingOff true");
            BackOff(offPlayer);
            backOffTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        anim.SetTrigger("InZoneTrigger");
        isBackingOff = false;
        //Debug.Log("isBackingOff false");
        //todo this needs a stat machine to determine if the DB needs to chase the WR past the Zone, Does he have overhead help
    }

    private IEnumerator TurnTowardsLOS()
    {
        //todo write this function
        yield return new WaitForFixedUpdate();
    }

    private void StayInfront(OffPlayer offPlayer)
    {
        DisableNavMeshAgent();

        float speed = 3; //todo needs to be a calculation of WR release and DB press
        Vector3 dir = (offPlayer.transform.position + offPlayer.transform.forward - transform.position).normalized * speed;
        float v = dir.z;
        float h = dir.x;

        anim.SetFloat("VelocityX", h * speed);
        anim.SetFloat("VelocityZ", v * speed);

        rb.velocity = dir;
    }

    private void BackOff(OffPlayer offPlayer)
    {
        DisableNavMeshAgent();
        // turn around and run, cut left, or cut right

        float speed = 5; //todo this should be a calculation of offPlayer release vs db press
        Vector3 dir = (offPlayer.transform.position + offPlayer.transform.forward - transform.position).normalized * speed;
        float v = dir.z;
        float h = dir.x;

        anim.SetFloat("VelocityX", h * speed);
        anim.SetFloat("VelocityZ", v * speed);

        rb.velocity = dir;
    }

    public void BeBlocked(float blockTimeNorm, OffPlayer blocker)
    {
        canvas.enabled = true;
        canvas.transform.LookAt(Camera.main.transform);

        //todo make way around multiple defenders
        SetTargetPlayer(blocker.transform);
        pressBar.fillAmount = blockTimeNorm;
        navMeshAgent.acceleration = 0f;
        navMeshAgent.speed = 0f;
        isBlocked = true;

        if (!blockPlayers.Contains(blocker))
        {
            //print(blocker.name + " added to list");
            blockPlayers.Add(blocker);
        }

    }

    public void ReleaseBlock(OffPlayer blocker)
    {
        canvas.enabled = !canvas.enabled;
        isBlocked = false;
        wasBlocked = true;
        SetTargetPlayer(GameObject.FindGameObjectWithTag("Player").transform);
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
        gameManager.RaiseShedBlock(this);
        blockPlayers.Remove(blocker);
    }

    internal void BallThrown(QB thrower, OffPlayer reciever, FootBall ball, Vector3 impactPos, float arcType, float power, bool isComplete)
    {

    }

    internal void PassAttempt(QB thrower, OffPlayer reciever, FootBall ball, float arcType, float power)
    {
        if (IsVicinityPass(reciever))
        {
            //todo this is dumb, arctype is set in 
            if (arcType == 1.5f)
            {
                //todo roll dist vs stats
                AnticipateThrow(thrower, reciever, ball, arcType, power);
            }

            if (arcType == 2.3f) { }
            if (arcType == 3.2f) { }
        }
    }

    private void AnticipateThrow(QB thrower, OffPlayer receiver, FootBall ball, float arcType, float power)
    {
        //todo this code is used three times now 
        Vector3 targetPos = receiver.transform.position;
        Vector3 diff = targetPos - transform.position;
        Vector3 diffGround = new Vector3(diff.x, 0f, diff.z);
        Vector3 fireVel, impactPos;
        Vector3 velocity = receiver.navMeshAgent.velocity;


        //FTS Calculations https://github.com/forrestthewoods/lib_fts/tree/master/projects/unity/ballistic_trajectory
        float gravity;

        if (Ballistics.solve_ballistic_arc_lateral(transform.position, power, targetPos + Vector3.up, velocity, arcType,
            out fireVel, out gravity, out impactPos))
        {
            //todo: get distance to impact pos and match it against DB position and speed, 
            //pass outcome of roll to the football call an onInterception or onBlock event
            //figure out how to add a second impluse to the thrown football in the case of a blocked pass
            if ((transform.position - impactPos).magnitude < 5) // todo create range variableStat
            {
                EnableNavMeshAgent();
                StopAllCoroutines();
                navMeshAgent.speed += power; // todo fix this terrible code, basically speeds up character to get in position
                SetDestination(impactPos);
                Debug.Log("PassBlock");
                StartCoroutine("BlockPass", ball);
                receiver.SetColor(Color.red);
                ball.isComplete = false;
            }

        }
    }

    private IEnumerator BlockPass(FootBall ball)
    {
        isBlockingPass = true;
        anim.SetTrigger("BlockPass");
        canvas.gameObject.SetActive(true);

        while ((transform.position - ball.transform.position).magnitude > 2.7) //todo, this should be a calculation of anim time vs distance of football to targetOffPlayer.
        {
            //Debug.Log((transform.position - ball.transform.position).magnitude);
            yield return new WaitForEndOfFrame();
        }
        ball.BlockBallTrajectory();
        navMeshAgent.speed = navStartSpeed;
        isBlockingPass = false;
        canvas.gameObject.SetActive(false);
    }

    private bool IsVicinityPass(OffPlayer receiver)
    {
        float distToWr = Vector3.Distance(transform.position, receiver.transform.position);
        if (distToWr <= 5) return true; //todo create coverage range variable
        else return false;
    }

    internal bool IsTargetInZone(Transform coverTarget)
    {
        if ((coverTarget.transform.position - base.myZone.transform.position).magnitude <= base.myZone.zoneSize) return true; //targetOffPlayer is inside myZone
        else return false;
    }

    internal void SetTargetHb(Transform targetTransform)
    {
        SetDestination(targetTransform.position);
        transformTarget = targetTransform; targetHb = targetTransform.GetComponentInParent<HB>();
        //Debug.Log("Target Changed");
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

    private void TurnAround(Vector3 destination)
    {
        navMeshAgent.updateRotation = false;
        Vector3 direction = (destination - transform.position).normalized;
        Quaternion qDir = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, qDir, Time.deltaTime * 20);
    }

    //https://answers.unity.com/questions/1170087/instantly-turn-with-nav-mesh-agent.html

    public IEnumerator BlockCoolDown()
    {
        yield return new WaitForSeconds(blockCooldown);
        wasBlocked = false;
    }

    public void BallOwnerChange(FootBallAthlete ballOwner)
    {
        SetTargetPlayer(ballOwner.transform);
    }

    internal void DisableNavMeshAgent()
    {
        navMeshAgent.enabled = false;
    }
}



