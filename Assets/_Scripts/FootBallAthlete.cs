using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class FootBallAthlete : MonoBehaviour
{

    //public RouteManager startGoal;
    //todo clean up inheritance and add setters and getters instead of public variables. Separate OffPlayers variables from DefPlayers variables
    public Renderer materialRenderer;
    [HideInInspector] public IKControl iK;
    [HideInInspector] public Terrain terrain;
    [HideInInspector] public QB qb;

    [HideInInspector] public Color startColor;
    [HideInInspector] public Color rayColor;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public float navStartSpeed;
    [HideInInspector] public float navStartAccel;
    public Color highlightColor;


    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator anim;
    //public LineRenderer lr;



    [SerializeField] float maxAngle = 35;
    [SerializeField] float maxRadius = 5;

    [HideInInspector] public Vector3 passTarget;
    [HideInInspector] public bool beenPressed = false;
    [HideInInspector] public FootBall footBall;
    public Canvas canvas;
    public Image pressBar;
    [HideInInspector] public bool isHiked = false;
    [HideInInspector] public Transform targetPlayer;
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public RouteManager routeManager;
    [HideInInspector] public bool isBlocking;


    public Routes[] routes;
    public Routes myRoute;
    internal int totalCuts;
    internal Vector3 lastCutVector;
    internal bool wasAtLastCut = false;
    internal int currentRouteIndex = 0;
    internal float routeCutTolerance = 1.5f;


    internal float timeSinceArrivedAtRouteCut;
    internal Vector3 nextPosition;

    [HideInInspector] public WR[] wideRecievers;
    [HideInInspector] public DB[] defBacks;
    [HideInInspector] public HB[] hbs;
    [HideInInspector] public Oline[] oLine;
    [HideInInspector] public Dline[] dLine;
    [HideInInspector] public Transform startTarget;

    [Range(5, 10)]
    public float defZoneSize; //todo should this be determined by the Zone? //Maybe awareness checks

    [HideInInspector] public Zones zone;
    [HideInInspector] public WR targetWr;
    [HideInInspector] public HB targetHb;
    [HideInInspector] public DB targetDb;


    public bool isMan;
    public bool isZone;

    [HideInInspector] public bool isPressing;

    [HideInInspector] public bool isCatching = false;

    [HideInInspector] public CameraFollow cameraFollow;

    [HideInInspector] public UserControl userControl;
    [HideInInspector] public CameraRaycaster cameraRaycaster;
    [HideInInspector] public bool isSelected;

    void GetTerrain()
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

    void OnCollisionEnter(Collision collision)
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
        if (collGo == GameManager.instance.ballOwner.gameObject)
        {
            //todo check if player is facing ballowner is targetPlayer in 'tackle angle'

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

    internal void FixedUpdate()
    {
        //Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        //Debug.DrawRay(transform.position, forward, rayColor);
        Vector3 angleFOV2 = Quaternion.AngleAxis(maxAngle, transform.up) * transform.forward * maxRadius;
        Vector3 angleFOV1 = Quaternion.AngleAxis(-maxAngle, transform.up) * transform.forward * maxRadius;
        Debug.DrawRay(transform.position, angleFOV2, rayColor);
        Debug.DrawRay(transform.position, angleFOV1, rayColor);
        RaycastForward();
    }

    internal void RaycastForward()
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

                }
            }

        }
    }
    internal void SetDestination(Vector3 dest)
    {
        EnableNavMeshAgent();
        navMeshAgent.SetDestination(dest);
    }
    internal void EnableNavMeshAgent()
    {
        if (!navMeshAgent.enabled)
            navMeshAgent.enabled = true;
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
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
            }
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
    //internal Transform GetClosestDline(Dline[] enemies)
    //{
    //    Transform bestTarget = null;
    //    float closestDistanceSqr = Mathf.Infinity;
    //    Vector3 currentPosition = transform.position;

    //    foreach (Dline potentialTarget in enemies)
    //    {

    //        Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
    //        float dSqrToTarget = directionToTarget.sqrMagnitude;
    //        if (dSqrToTarget < closestDistanceSqr)
    //        {

    //            closestDistanceSqr = dSqrToTarget;
    //            bestTarget = potentialTarget.transform;
    //        }
    //    }

    //    return bestTarget;
    //}
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

}

public class OffPlayer : FootBallAthlete
{
    public float blockRange = 3;
    public float blockCoolDown = 1f;
    public bool canBlock = true;
    internal bool isBlocker;

    internal void BlockProtection() //todo consolidate duplicate code
    {
        if (!canBlock) return;
        ;
        //todo change code to be moving forward with blocks, get to the second level after shedding first defender

        //sudo get positions of all other blockers
        var blockTargets = gameManager.defPlayers;

        foreach (var defender in blockTargets)
        {
            if ((defender.transform.position - transform.position).magnitude < blockRange)
            {

            }

            ;
        }

        if (targetPlayer == null)
        {
            targetPlayer = GetClosestDefPlayer(gameManager.defPlayers);
        }

        Vector3 directionToTarget = targetPlayer.position - transform.position;
        transform.LookAt(targetPlayer);

        if (gameManager.isRun) SetTargetDlineRun(targetPlayer);
        if (gameManager.isPass) SetTargetDline(targetPlayer);

        float dSqrToTarget = directionToTarget.sqrMagnitude;
        if (dSqrToTarget < 3) //todo setup block range variable
        {
            var defPlayer = targetPlayer.GetComponent<DefPlayer>();
            if (!isBlocking) StartCoroutine("BlockTarget", targetPlayer);
        }
    }


    private void SetTargetDlineRun(Transform target) //todo collapse into single function
    {

        SetDestination(target.position); // 
    }

    public void SetTargetDline(Transform target)
    {
        //if (isBlocking) return;
        SetDestination(qb.transform.position +
                       (target.position - qb.transform.position) /
                       2); // todo centralize ball carrier, access ballcarrier instead of hard coded transform
    }

    IEnumerator BlockTarget(Transform target)
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
        isBlocking = false;
        canBlock = false;
        StartCoroutine(BlockCoolDown());
    }

    IEnumerator BlockCoolDown()
    {
        yield return new WaitForSeconds(blockCoolDown);
        canBlock = true;
    }

    public int routeSelection;
}

public class DefPlayer : FootBallAthlete
{
    [HideInInspector] public bool isBlocked;
    public bool wasBlocked = false;
    [SerializeField] Image blockBar;
    internal float blockCooldown = 2f;
    internal List<OffPlayer> blockPlayers = new List<OffPlayer>();

    public void BeBlocked(float blockTimeNorm, OffPlayer blocker)
    {
        canvas.enabled = true;
        canvas.transform.LookAt(Camera.main.transform);

        //todo make way around multiple defenders
        SetTargetPlayer(blocker.transform);
        blockBar.fillAmount = blockTimeNorm;
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

    public void SetTargetPlayer(Transform targetSetter)
    {
        navMeshAgent.SetDestination(targetSetter.position);
        targetPlayer = targetSetter;
    }

    IEnumerator BlockCoolDown()
    {
        yield return new WaitForSeconds(blockCooldown);
        wasBlocked = false;
    }

    public void BallOwnerChange(FootBallAthlete ballOwner)
    {
        SetTargetPlayer(ballOwner.transform);
    }

}


