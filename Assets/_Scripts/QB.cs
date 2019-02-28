using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

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

    HB[] hbs;
    WR[] wrs;
    DB[] dbs;
    Transform hbTransform;

    bool isRapidFire;
   

    void Start ()
    {
        //controller = GetComponent<CharacterController>();
        FindComponenets();
        gameManager.hikeTheBall += HikeTheBall;
    }

    private void HikeTheBall(bool wasHiked)
    {
        HikeTrigger(); //anim
    }

    private void FindComponenets()
    {
        athlete = GetComponent<FootBallAthlete>();
        rb = GetComponent<Rigidbody>();
        throwingHandScript = FindObjectOfType<ThrowingHand>();
        gameManager = FindObjectOfType<GameManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        aiCharacter = GetComponent<AICharacterControl>();
        userControl = GetComponent<ThirdPersonUserControl>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        anim = GetComponent<Animator>();
        navMeshAgent.enabled = false;
        aiCharacter.enabled = false;
        hbs = FindObjectsOfType<HB>();
        wrs = FindObjectsOfType<WR>();
        dbs = FindObjectsOfType<DB>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!gameManager.isHiked) return;
        

        if (gameManager.isRun)
        {
            if (!navMeshAgent.enabled)
            {
                navMeshAgent.enabled = true;
                aiCharacter.enabled = true;
            }
            if (hbTransform == null)
            {
                hbTransform = GetClosestHB(hbs);
                SetTargetHB(hbTransform);
            }
            Vector3 directionToTarget = hbTransform.position - transform.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < 1 && hasBall)
            {
                Debug.Log("Handoff");
                hasBall = false;
                HandOffBall(hbTransform);
            }
        }
    }

    private void HandOffBall(Transform hb)
    {
        HB ballCarrier = hb.GetComponent<HB>();
        transform.tag = "OffPlayer";
        StandStill();
        ballCarrier.SetPlayerTag();
        userControl.enabled = false;
        cameraFollow.ResetPlayer();
        //ballCarrier.navMeshAgent.enabled = false;
        ballCarrier.aiCharacter.enabled = false;
       
    }

    private void StandStill()
    {
      GameObject go = Instantiate(new GameObject(), transform.position + new Vector3(-2,0,0), Quaternion.identity);
      
      aiCharacter.target = go.transform;
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
        aiCharacter.target = targetSetter;
        
    }

    void OnMouseDown()
    {

    }
    public void HikeTrigger()
    {
        anim.SetTrigger("HikeTrigger");
        navMeshAgent.enabled = true;
    
       
    }
    public void SetPosition()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] clips = anim.GetCurrentAnimatorClipInfo(0);
        Keyframe keyframe;
        
        transform.position = transform.position + new Vector3(0,0.2f,-2);
        Debug.Log(clips[0].clip.name);  
        
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


    public void Throw(Vector3 passTarget, WR wr, float arcType, float power)
    {
        if(!isRapidFire)
        throwVector = passTarget;
        targetWr = wr;
        throwArc = arcType;
        throwPower = power;
        StartCoroutine("PassTheBall");
        anim.SetTrigger("PassTrigger");
    }

    IEnumerator PassTheBall()   {
        Quaternion lookAt = Quaternion.LookRotation(throwVector);
        throwingHand = throwingHandScript.gameObject;
        yield return new WaitForSeconds(1);
        var thrownBall = Instantiate(footBall, throwingHand.transform.position, lookAt);
        FootBall thrownBallScript = thrownBall.GetComponent<FootBall>();
        thrownBallScript.PassFootBallToMovingTarget(targetWr, throwArc, throwPower);
        Destroy(thrownBallScript, 3f);
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