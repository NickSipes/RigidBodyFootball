using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class FootBallAthlete : MonoBehaviour
{
    public Renderer materialRenderer;
    [HideInInspector] public QB qb;
    [HideInInspector] public Color startColor;
    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public float navStartSpeed;
    [HideInInspector] public float navStartAccel;
    public Color highlightColor;
    [HideInInspector] public Color LineColor;
    [HideInInspector] public AICharacterControl aiCharacter;
    [HideInInspector] public Rigidbody rb;
    public LineRenderer lr;
    [HideInInspector] public Transform startGoal;
    [HideInInspector] public Vector3 passTarget;
    [HideInInspector] public bool beenPressed = false;
    public FootBall footBall;
    public Canvas canvas;
    public Image pressBar;
    [HideInInspector] public bool isHiked = false;
    public Transform target;
    [HideInInspector] public DB[] defBacks;
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public bool isBlocking;

    [HideInInspector] public WR[] wideRecievers;
    [HideInInspector] public HB[] hb;

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

    [HideInInspector] public ThirdPersonUserControl userControl;
    [HideInInspector] public ThirdPersonCharacter thirdPerson;
    [HideInInspector] public CameraFollow cameraFollow;
    // Start is called before the first frame update
    void Start()
    {
        cameraFollow = FindObjectOfType<CameraFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked) return;
    }
}
