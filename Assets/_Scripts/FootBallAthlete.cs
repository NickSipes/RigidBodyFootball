using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class FootBallAthlete : MonoBehaviour
{

    public RouteManager startGoal;
    public Routes route;
    //todo clean up inheritance and add setters and getters instead of public variables. Seperate OffPlayers variables from DefPlayers variables
    public Renderer materialRenderer;
    [HideInInspector] public IKControl iK;

    [HideInInspector] public QB qb; 

    [HideInInspector] public Color startColor;


    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public float navStartSpeed;
    [HideInInspector] public float navStartAccel;
    public Color highlightColor;
    [HideInInspector] public Color LineColor;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator anim;
    //public LineRenderer lr;

    [HideInInspector] public Vector3 passTarget;
    [HideInInspector] public bool beenPressed = false;
    public FootBall footBall;
    public Canvas canvas;
    public Image pressBar;
    [HideInInspector] public bool isHiked = false;
    public Transform target;
    public GameManager gameManager;

    [HideInInspector] public bool isBlocking;

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
    public bool isSelected;


    //todo create virtual start 
    //https://stackoverflow.com/questions/53076669/how-to-correctly-inherit-unitys-callback-functions-like-awake-start-and-up
    //protected virtual void Start()
    //{
    //    whatIsGround = LayerMask.GetMask("Ground");
    //    groundRadius = 0.01f;
    //}

    // Start is called before the first frame update
    void Start()
    {
       
        //rayColor = materialRenderer.material.color;
    }
    //protected virtual void FixedUpdate()
    //{
    //    RaycastForward();
    //}

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

    //internal void RaycastForward()
    //{
    //    Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
    //    if (rayColor == null) rayColor = SetRaycastColor();
    //    Debug.DrawRay(transform.position, forward, rayColor);

    //    RaycastHit[] hits;
    //    hits = Physics.RaycastAll(transform.position, transform.forward, 100.0F);
    //    //if(hits.Length != 0)Debug.Log(hits.Length);

    //    for (int i = 0; i < hits.Length; i++)
    //    {
    //        RaycastHit hit = hits[i];
    //        if (hit.collider.isTrigger)
    //        {
    //            Transform zoneObject = hit.collider.transform;
    //            if (zoneObject)
    //            {

    //            }
    //        }

    //    }
    //}
    //Color SetRaycastColor()
    //{
    //    return materialRenderer.material.color;
    //}

}


