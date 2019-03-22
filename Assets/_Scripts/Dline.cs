using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class Dline : FootBallAthlete
{

    public bool wasBlocked = false;
    [SerializeField] Image blockBar;
    private float blockCooldown = 2f;
    [HideInInspector] public bool isBlocked;
    private Color rayColor = Color.red;
    // Start is called before the first frame update
    void Start()
    {
        FindComponents();
        RegesterEvents();
    }


    private void FindComponents()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        //aiCharacter = GetComponent<AICharacterControl>();
        //userControl = GetComponent<ThirdPersonUserControl>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        anim = GetComponent<Animator>();
        hbs = FindObjectsOfType<HB>();
        wideRecievers = FindObjectsOfType<WR>();
        defBacks = FindObjectsOfType<DB>();
        oLine = FindObjectsOfType<Oline>();
        dLine = FindObjectsOfType<Dline>();
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
    }
    private void RegesterEvents()
    {
        gameManager.ballOwnerChange += BallOwnerChange;
        gameManager.hikeTheBall += HikeTheBall;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked) return;

        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        SetTarget(target);

        if (wasBlocked && !isBlocked)
            StartCoroutine(BlockCoolDown()); 
    }
    private void FixedUpdate()
    {
        RaycastForward();
    }

    internal void RaycastForward()
    {
       
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Debug.DrawRay(transform.position, forward, rayColor);

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
    Color SetRaycastColor()
    {
        return materialRenderer.material.color;
    }
    IEnumerator BlockCoolDown()
    {
        yield return new WaitForSeconds(blockCooldown);
        wasBlocked = false;

    }

    public void SetTarget(Transform targetSetter)
    {
       
       navMeshAgent.SetDestination(targetSetter.position);
        target = targetSetter;

    }

    public void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
    }


    public void Block(float blockTimeNorm, Oline blocker)
    {
        canvas.enabled = true;
        canvas.transform.LookAt(Camera.main.transform);
        SetTarget(blocker.transform);
        blockBar.fillAmount = blockTimeNorm;
        navMeshAgent.acceleration = 0f;
        navMeshAgent.speed = 0f;
        isBlocked = true;
    }

    public void ReleaseBlock()
    {
        canvas.enabled = !canvas.enabled;
        isBlocked = false;
        wasBlocked = true;
        SetTarget(GameObject.FindGameObjectWithTag("Player").transform);
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
    }

    public void BallOwnerChange(FootBallAthlete ballOwner)
    {
        SetTarget(ballOwner.transform);

    }
}
