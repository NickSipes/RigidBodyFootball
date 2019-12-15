using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;

// ReSharper disable UnusedMember.Local
#pragma warning disable 108,114

public class QB : OffPlayer {
    //CharacterController controller;
    public float speed = 5;
    public float gravity = -5;
    private GameObject throwingHand;
    private ThrowingHand throwingHandScript;
    private bool hasBall = true;
    float throwArc;
    float throwPower;
    Vector3 throwVector;

    Transform hbTransform;

    bool isRapidFire;

    void Start ()
    {
        base.Start();
        //controller = GetComponent<CharacterController>();
        rayColor = Color.cyan;
        throwingHandScript = GetComponentInChildren<ThrowingHand>();
        userControl = GetComponent<UserControl>();
        gameManager.hikeTrigger += HikeTrigger;
    }

    private void HikeTheBall(bool washiked)
    {
        gameManager.ballOwner = this;
    }

    private void BallThrown(QB thrower, WR reciever, FootBall footBall, Vector3 impactPos, float arcType, float power, bool isComplete)
    {
        
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
                hbTransform = GetClosestHb(hbs);
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
        base.FixedUpdate();
    }

    public void HikeTrigger() // called from UI button on gamemanger
    {
            navMeshAgent.enabled = true;
        if (gameManager.isPass)
        {
            anim.SetTrigger("HikeTrigger");
        }
        if (gameManager.isRun)
        {
            Debug.Log("Run Play");
            userControl.enabled = false;
            gameManager.Hike();
       
        }

    }
    void SnapTheHike() //triggered by animation event
    {
      gameManager.Hike();
    }
  
    private void HandOffBall(Transform hb)
    {
        gameManager.ChangeBallOwner(gameObject, hb.gameObject);
        StandStill();
    }

    private void StandStill()
    {
      GameObject go = Instantiate(new GameObject(), transform.position + new Vector3(-2,0,0), Quaternion.identity);
      go.transform.name = "StandStill";
      navMeshAgent.SetDestination(go.transform.position);
    }

   

    public void SetTargetHB(Transform targetSetter)
    {
        navMeshAgent.SetDestination(targetSetter.position);
        
    }

    void StartDropBack() // called via anim event
    {
        AnimatorClipInfo[] clips = anim.GetCurrentAnimatorClipInfo(0);
        float clipTime = clips[0].clip.length;
        StartCoroutine("DropBack", clipTime);
    }
    IEnumerator DropBack(float clipTime)
    {
        float t = 0;
        navMeshAgent.SetDestination(transform.position + new Vector3(0, 0, -2.3f));
        while (t <= clipTime)
        {
            t += Time.deltaTime;
            if (Input.anyKey) { navMeshAgent.enabled = false; }
            transform.LookAt(new Vector3(0, 0, 10));
            yield return new WaitForEndOfFrame();
        }
        navMeshAgent.enabled = false;
    }
    
    public void BeginThrowAnim(Vector3 passTarget, OffPlayer reciever, float arcType, float power)
    {
        if(!isRapidFire)
        //todo cleanup this code, could cause bugs setting these values then running coroutine
        throwVector = passTarget;
        targetReceiver = reciever;
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
        thrownBall.name = "throwBall";
        FootBall thrownBallScript = thrownBall.GetComponent<FootBall>();
        thrownBallScript.PassFootBallToMovingTarget(this, targetReceiver, thrownBallScript, throwArc, throwPower);
        gameManager.isPassStarted = true;
        //Destroy(thrownBall, 3f); //todo get better solution to removing footballs
    }

} 
