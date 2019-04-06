using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
// ReSharper disable All

public class DB : DefPlayer
{

    //todo create class between Off and Def based on FootballAthlete

    [SerializeField] float maxAngle;
    [SerializeField] float maxRadius;
    [SerializeField] internal int zoneLayer = 8;


    internal bool isWrIncoming = false;

    internal bool isBlockingPass = false;
    //todo HOW ARE WE GOING TO HANDLE JUMP ANIMATIONS
    //todo defenders need to determine LOS and where to lineup
    //todo DB State Machine
    //todo readdress how pass incomlpetion are cacluated. All determining factors should be rolls vs stats ?
    //todo scramble mechanics
    //todo Man coverage
    // Use this for initialization
    void Start()
    {
        rayColor = Color.yellow;
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
        rb = GetComponent<Rigidbody>();
        CreateZone(); //todo only run if player is in zone
        qb = FindObjectOfType<QB>();
    }


    private void CreateZone()
    {
        //todo make zoneCenterGO move functions dependent on play developement;
        Vector3 zoneCenterStart = GetZoneStart();
        GameObject zoneGO = Instantiate(new GameObject(), zoneCenterStart, Quaternion.identity);
        zone = zoneGO.AddComponent<Zones>();
        zoneGO.transform.position = transform.position + new Vector3(0, 0, 5);
        zoneGO.transform.name = transform.name + "ZoneObject";
        GameObject zoneObjectContainer = GameObject.FindGameObjectWithTag("ZoneObject"); //Hierarchy Cleanup
        zoneGO.transform.parent = zoneObjectContainer.transform;
        zoneGO.transform.tag = "ZoneObject";
        SphereCollider sphereCollider = zoneGO.gameObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        zoneGO.layer = zoneLayer;
        zone.zoneSize = defZoneSize;
    }

    private Vector3 GetZoneStart()
    {
        return transform.position + new Vector3(5, 0, 0); //todo this needs to be dependent on player responsibilities
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked)
            return;

        // ReSharper disable once InvertIf
        if (gameManager.isPass)
        {
            if (isBlockingPass) return;
            if (isPressing) return;

            if (isZone)
            {
                //todo this whole triple if statement sucks
                if (targetWr != null)
                {
                    if (IsTargetInZone(targetWr.transform))
                    {
                        SetDestination(targetWr.transform.position);
                        return;
                    }
                    else
                    {
                        //Debug.Log("targetPlayer out of zone");
                        targetWr = null;
                        targetPlayer = null;
                    }
                }
                PlayZone();
            }

            // ReSharper disable once InvertIf
            if (gameManager.isRun)
            {
                targetPlayer = GetClosestHb(hbs);
                SetTargetHb(targetPlayer);
            }
        }
    }

    void FixedUpdate()
    {
        base.FixedUpdate();
        Vector3 angleFOV2 = Quaternion.AngleAxis(maxAngle, transform.up) * transform.forward * maxRadius;
        Vector3 angleFOV1 = Quaternion.AngleAxis(-maxAngle, transform.up) * transform.forward * maxRadius;
        Debug.DrawRay(transform.position, angleFOV2);
        Debug.DrawRay(transform.position, angleFOV1);

    }
    
    private void SetTargetWr(Transform targetTransform)
    {
        EnableNavMeshAgent();
        SetDestination(targetTransform.position);
        targetPlayer = targetTransform;
        targetWr = targetTransform.GetComponentInParent<WR>();
        //Debug.Log("Target Changed");
    }

    void PlayZone()
    {
        //todo access WR route to see if it will pass through zone and then move towards intercept point
        if (targetWr == null)
        {
           //has return if enemy set
            if (CheckZone())return;
        }
        SetDestination(zone.zoneCenter);

    }

    private bool CheckZone()
    {
        var possibleEnemy = GetClosestWr(wideRecievers);
        Vector3 wrZoneCntrDist = possibleEnemy.position - zone.transform.position;
        //Debug.Log(wrZoneCntrDist.magnitude);
        if (wrZoneCntrDist.magnitude < zone.zoneSize)
        {
            //todo this will break with mulitple recievers, need to determine coverage responsablilty
            SetTargetWr(possibleEnemy);
            return true;
        }

        foreach (var reciever in wideRecievers)
        {
            RaycastHit[] hits = Physics.RaycastAll(reciever.transform.position, reciever.transform.forward, 100.0F);
            //if(hits.Length != 0)Debug.Log(hits.Length);
           
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider.tag == "ZoneObject")
                {
                    Zones zoneObject = hit.collider.gameObject.GetComponent<Zones>();

                    if (zoneObject == zone)
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


    IEnumerator WrPress(WR wr)
    {
        isPressing = true;
        float pressTime = 1f;
        anim.SetTrigger("PressTrigger");
        SetDestination(wr.transform.position + transform.forward);
        float pressTimeNorm = 0;
        //Vector3 dir = (wr.transform.position - transform.position).normalized * pressTime;
        //rb.velocity = dir;
        while (pressTimeNorm <= 1f)
        {
            StayInfront();
            pressTimeNorm += Time.deltaTime / pressTime;
            wr.Press(pressTimeNorm);
            yield return new WaitForEndOfFrame();
        }
        anim.SetTrigger("ReleaseTrigger");
        StartCoroutine("BackOffPress", wr);

        wr.ReleasePress();


    }
    IEnumerator BackOffPress(WR wr)
    {
        //read receiver route, move backwards, release reciever to new defender, moves towards next receiver


        while ((transform.position - zone.transform.position).magnitude > 1)
        {
            //Vector3 dir = targetPlayer.position - transform.position;
            BackOff(wr);
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine("TurnTowardsLOS");
        //Debug.Log("zone center reached");
        isPressing = false;
        //Vector3 moveLeft = transform.position + new Vector3(2, 0, 0);
        //todo this needs a stat machine to determine if the DB needs to chase the WR past the Zone, Does he have overhead help
        //while ((transform.position - moveLeft).magnitude > 1)
        //{
        //    float speed = 3;
        //    Vector3 dir = (moveLeft - transform.position).normalized * speed;
        //    rb.velocity = dir;
        //    int velocity = 100;
        //    float angle;
        //    var targetDir = qb.transform.position - transform.position;
        //    var forward = transform.forward;
        //    var localTarget = transform.InverseTransformPoint(qb.transform.position);
        //    angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        //    Vector3 eulerAngleVelocity = new Vector3(0, angle, 0);
        //    Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime);
        //    rb.MoveRotation(rb.rotation * deltaRotation);
        //    yield return new WaitForFixedUpdate();
        //}
        //DisableNavmeshAgent();
        //navMeshAgent.ResetPath();
        anim.SetTrigger("InZoneTrigger");
    }
    IEnumerator TurnTowardsLOS()
    {
        //todo write this function
        yield return new WaitForFixedUpdate();
    }
    private void StayInfront()
    {
        DisableNavmeshAgent();

        float speed = 3; //todo needs to be a calculation of WR release and DB press
        Vector3 dir = (targetPlayer.position + targetPlayer.transform.forward - transform.position).normalized * speed;
        float v = dir.z;
        float h = dir.x;

        anim.SetFloat("VelocityX", h * speed);
        anim.SetFloat("VelocityZ", v * speed);
        //rb.velocity = new Vector3(h * speed, 0, v * speed);
        //Vector3 tempVect = new Vector3(h, 0, v);
        //tempVect = tempVect.normalized * speed * Time.deltaTime;
        rb.velocity = dir;
    }

    private void DisableNavmeshAgent()
    {
        navMeshAgent.enabled = false;
     
    }

    private void BackOff(WR wr)
    {


       DisableNavmeshAgent();

        // turn around and run, cut left, or cut right

        float speed = 5; //todo this should be a calculation of wr release vs db press
        Vector3 dir = (targetPlayer.position + targetPlayer.transform.forward - transform.position).normalized * speed;
        float v = dir.z;
        float h = dir.x;

        anim.SetFloat("VelocityX", h * speed);
        anim.SetFloat("VelocityZ", v * speed);
        //rb.velocity = new Vector3(h * speed, 0, v * speed);
        //Vector3 tempVect = new Vector3(h, 0, v);
        //tempVect = tempVect.normalized * speed * Time.deltaTime;
        rb.velocity = dir;
    }

    private void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
        //todo move press code here!
        var potientialTarget = GetClosestWr(wideRecievers);
        if ((potientialTarget.transform.position - transform.position).magnitude < 5f)
        {
            SetTargetWr(potientialTarget);
            if (targetWr.CanBePressed())
                StartCoroutine(WrPress(targetWr));
            //todo press range variable
        }
    }

    private void BallThrown(QB thrower, WR reciever, FootBall ball, Vector3 impactPos, float arcType, float power, bool isComplete)
    {

    }

    private void PassAttempt(QB thrower, WR reciever, FootBall ball, float arcType, float power)
    {
        if (InVincintyOfPass(reciever))
        {
            if (arcType == 1.5f)
            {
                //todo roll dist vs stats
                AniciptateThrow(thrower, reciever, ball, arcType, power);
            }

            if (arcType == 2.3f) { }
            if (arcType == 3.2f) { }
        }
    }

    private void AniciptateThrow(QB thrower, WR reciever, FootBall ball, float arcType, float power)
    {
        //todo this code is used three times now 
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


            if ((transform.position - impactPos).magnitude < 5) // todo create range variableStat
            {
                EnableNavMeshAgent();
                StopAllCoroutines();
                navMeshAgent.speed += power; // todo fix this terrible code, basically speeds up character to get in position
                SetDestination(impactPos);
                Debug.Log("PassBlock");
                StartCoroutine("BlockPass", ball);
                reciever.SetColor(Color.red);
                ball.isComplete = false;
            }

        }
    }

    IEnumerator BlockPass(FootBall ball)
    {
        isBlockingPass = true;
        anim.SetTrigger("BlockPass");
        canvas.gameObject.SetActive(true);
       
        while ((transform.position - ball.transform.position).magnitude > 2.7) //todo, this should be a calculation of anim time vs distance of football to targetPlayer.
        {
            //Debug.Log((transform.position - ball.transform.position).magnitude);
            yield return new WaitForEndOfFrame();
        }
        ball.BlockBallTrajectory();
        navMeshAgent.speed = navStartSpeed;
        isBlockingPass = false;
        canvas.gameObject.SetActive(false);
    }

    bool InVincintyOfPass(WR wR)
    {
        float distToWr = Vector3.Distance(transform.position, wR.transform.position);
        if (distToWr <= 5) return true; //todo create coverage range variable
        else return false;
    }

    private bool IsTargetInZone(Transform coverTarget)
    {
        if ((coverTarget.transform.position - zone.transform.position).magnitude <= zone.zoneSize) return true; //targetPlayer is inside zone
        else return false;
    }


    private void SetTargetHb(Transform targetTransform)
    {
        SetDestination(targetTransform.position);
        targetPlayer = targetTransform;
        targetHb = targetTransform.GetComponentInParent<HB>();
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
        if (zone)
        {
            // Draw zone sphere 
            Gizmos.color = new Color(0, 0, 255, .5f);
            Gizmos.DrawWireSphere(zone.transform.position, defZoneSize);
        }
    }
    private void TurnAround(Vector3 destination)
    {

        navMeshAgent.updateRotation = false;
        Vector3 direction = (destination - transform.position).normalized;
        Quaternion qDir = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, qDir, Time.deltaTime * 20);
    }

    //https://answers.unity.com/questions/1170087/instantly-turn-with-nav-mesh-agent.html
}
