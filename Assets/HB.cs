using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.UIElements;
using UnityStandardAssets.Characters.ThirdPerson;

public class HB : MonoBehaviour
{
    [SerializeField] private Renderer materialRenderer;
    private QB qb;
    private HB hb;
    private Color startColor;
    public NavMeshAgent navMeshAgent;
    private float navStartSpeed;
    private float navStartAccel;
    [SerializeField] private Color highlightColor;
    public Color LineColor;
    private AICharacterControl aiCharacter;
    private Rigidbody rb;
    [SerializeField] LineRenderer lr;
    public Transform startGoal;
    private Vector3 passTarget;
    private bool beenPressed = false;
    public Image pressBar;

    public Canvas canvas;

    bool isHiked = false;
    public Transform target;

    GameManager gameManager;

    // Use this for initialization
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        qb = FindObjectOfType<QB>();
        hb = FindObjectOfType<HB>();
        rb = GetComponent<Rigidbody>();
        startColor = materialRenderer.material.color;
        aiCharacter = GetComponent<AICharacterControl>();
        startGoal = aiCharacter.target;
        aiCharacter.target = null;
        target = startGoal;
        lr.material.color = LineColor;
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (gameManager.isRun)
        {
            aiCharacter.target = hb.transform;
        }
    }
}
