using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class FootBallAthlete : MonoBehaviour
{

    public RouteManager startGoal;
    public Routes route;
    //todo clean up inheritance and add setters and getters instead of public variables. Seperate OffPlayers variables from DefPlayers variables
    public Renderer materialRenderer;
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
    public float zoneSize;
    public Vector3 zoneCenter;
    public GameObject zoneCenterGO;
    public WR targetWr;
    [HideInInspector] public HB targetHb;
    [HideInInspector] public DB targetDb;

    
    public bool isMan;
    public bool isZone;

    [HideInInspector] public bool isPressing;

    [HideInInspector] public bool isCatching = false;

    [HideInInspector] public CameraFollow cameraFollow;

    [HideInInspector] public UserControl userControl;



    // Start is called before the first frame update
    void Start()
    {
        cameraFollow = FindObjectOfType<CameraFollow>();
        navMeshAgent = GetComponent<NavMeshAgent>();
      
    }
     
    public void SetUserControl()
    {
        userControl = GetComponent<UserControl>();
        userControl.enabled = true; //todo create an method to disable user control on all other players
    }
    
    
}
