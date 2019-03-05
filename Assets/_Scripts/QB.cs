using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.CrossPlatformInput;

public class QB : FootBallAthlete {
    //CharacterController controller;
    public float speed = 5;
    public float gravity = -5;
   
    private GameObject throwingHand;
    private ThrowingHand throwingHandScript;
    private bool hasBall = true;
    float throwArc;
    float throwPower;
    Vector3 throwVector;
    
    FootBallAthlete athlete;

    
    Transform hbTransform;

    bool isRapidFire;
   

    void Start ()
    {
        //controller = GetComponent<CharacterController>();
        FindComponents();

    }
    private void FindComponents()
    {
        athlete = GetComponent<FootBallAthlete>();
        rb = GetComponent<Rigidbody>();
        throwingHandScript = FindObjectOfType<ThrowingHand>();
        gameManager = FindObjectOfType<GameManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        
        userControl = GetComponent<ThirdPersonUserControl>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        anim = GetComponent<Animator>();
        navMeshAgent.enabled = false;
      
        hbs = FindObjectsOfType<HB>();
        wideRecievers = FindObjectsOfType<WR>();
        defBacks = FindObjectsOfType<DB>();
        oLine = FindObjectsOfType<Oline>();
        dLine = FindObjectsOfType<Dline>();
    }


    // Update is called once per frame
    private void Update()
    {
        if (!gameManager.isHiked) return;


        if (gameManager.isRun)
        {
            userControl.enabled = false;
          
            if (hbTransform == null)
            {
                hbTransform = GetClosestHB(hbs);
                SetTargetHB(hbTransform);
            }
            Vector3 directionToTarget = hbTransform.position - transform.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < 1 && hasBall)
            {
                Debug.Log("Handoff");
                hasBall = false;
                HandOffBall(hbTransform);
            }
        }
    }
    private void FixedUpdate()
    {
        if (!gameManager.isHiked) return;
        // read inputs
        if (gameManager.isPass)
        {
           StrafeMove(true);
        }   
        
    }

    public void StrafeMove(bool move)
    {
        speed = 20;
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        anim.SetBool("isStrafe", move);
        anim.SetFloat("VelocityX", h * speed);
        anim.SetFloat("VelocityZ", v * speed);
        rb.velocity = new Vector3(h * speed, 0, v * speed);
    }

    public void HikeTrigger() // called from UI button
    {
            navMeshAgent.enabled = true;
        if (gameManager.isPass)
        {
            anim.SetTrigger("HikeTrigger");
        }
        if (gameManager.isRun)
        {
            Debug.Log("Run Play");
            anim.SetTrigger("HandOff");
            gameManager.Hike();
            LineManHike();
        }

    }
    void SnapTheHike() //triggered by animation event
    {
        LineManHike();
        gameManager.Hike();
    }

    public void LineManHike()
    {

        foreach (var lineman in oLine)
        {
            lineman.HikeTheBall(true);
        }
        foreach (var lineman in dLine)
        {
            lineman.HikeTheBall(true);
        }
    }
    public void SetPosition()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] clips = anim.GetCurrentAnimatorClipInfo(0);
        transform.position = transform.position + new Vector3(0, 0.2f, -2);
        Debug.Log(clips[0].clip.name);
    }

  
    private void HandOffBall(Transform hb)
    {
        //todo extract call to game manager in order to trigger gameManager.ballOwnerChange event
        HB ballCarrier = hb.GetComponent<HB>();
        transform.tag = "OffPlayer";
        StandStill();
        ballCarrier.SetPlayerTag();
        userControl.enabled = false;
        cameraFollow.ResetPlayer();
        gameManager.ChangeBallOwner(ballCarrier.gameObject); 
        //ballCarrier.navMeshAgent.enabled = false;
      
       
    }

    private void StandStill()
    {
      GameObject go = Instantiate(new GameObject(), transform.position + new Vector3(-2,0,0), Quaternion.identity);
      
      navMeshAgent.SetDestination(go.transform.position);
    }

    Transform GetClosestHB(HB[] enemies)
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

    public void SetTargetHB(Transform targetSetter)
    {
        navMeshAgent.SetDestination(targetSetter.position);
        
    }

    void StartDropBack()
    {
        AnimatorClipInfo[] clips = anim.GetCurrentAnimatorClipInfo(0);
        float clipTime = clips[0].clip.length;
        StartCoroutine("DropBack", clipTime);
    }
    IEnumerator DropBack(float clipTime)
    {
        float t = 0;
        navMeshAgent.SetDestination(transform.position + new Vector3(0, 0, -2.3f));
        while (t <= clipTime) {
            t += Time.deltaTime;
            if (Input.anyKey) { navMeshAgent.enabled = false; }
            yield return new WaitForEndOfFrame();
                }
        navMeshAgent.enabled = false;
    }


    public void BeginThrowAnim(Vector3 passTarget, WR wr, float arcType, float power)
    {
        if(!isRapidFire)
        //todo cleanup this code, could cause bugs setting these values then running coroutine
        throwVector = passTarget;
        targetWr = wr;
        throwArc = arcType;
        throwPower = power;
        StartCoroutine("PassTheBall");
        anim.SetTrigger("PassTrigger");
    }

    IEnumerator PassTheBall()   {
        Quaternion lookAt = Quaternion.LookRotation(throwVector);
        transform.rotation = lookAt;
        throwingHand = throwingHandScript.gameObject;
        yield return new WaitForSeconds(1); //todo find way to access animation curve info.
        var thrownBall = Instantiate(footBall, throwingHand.transform.position, lookAt); //todo put footballs in hierarchy container
        FootBall thrownBallScript = thrownBall.GetComponent<FootBall>();
        thrownBallScript.PassFootBallToMovingTarget(targetWr, throwArc, throwPower);
        Destroy(thrownBall, 3f); //todo get better solution to removing footballs
    }

} 





/*
 *  public float speed = 5;
 public float gravity = -5;
 
 float velocityY = 0; 
 
 CharacterController controller;
 
 void Start()
 {
     controller = GetComponent<CharacterController>();
 }
 
 void Update()
 {
     velocityY += gravity * Time.deltaTime;
 
     Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0 Input.GetAxisRaw("Vertical"));
     input = input.normalized;
 
     Vector3 temp = Vector3.zero;
     if (input.z == 1)
     {
         temp += transform.forward;
     }
     else if (input.z == -1)
     {
         temp += transform.forward * -1;
     }
 
     if (input.x == 1)
     {
         temp += transform.right;
     }
     else if (input.x == -1)
     {
         temp += transform.right * -1;
     }
 
     Vector3 velocity = temp * speed;
     velocity.y = velocityY;
     
     controller.Move(velocity * Time.deltaTime);
 
     if (controller.isGrounded)
     {
         velocityY = 0;
     }
 }
*/