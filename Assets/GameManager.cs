﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{

    //todo make gamemanger a scriptable object

    public bool isRun = false;
    public bool isPass = false;
    public bool isHiked = false;
    public Object ballOwner;
    // Start is called before the first frame update

    public delegate void HikeTheBall(bool wasHiked);
    public event HikeTheBall hikeTheBall;

    public delegate void BallOwnerChange(GameObject ballOwner);
    public event BallOwnerChange ballOwnerChange;

    public delegate void PassAttempt(QB thrower, WR reciever, float arcType, float power);
    public event PassAttempt passAttempt;

    public delegate void OnBallThrown(QB thrower, WR reciever, Vector3 impactPos, float arcType, float power);
    public event OnBallThrown onBallThrown;

    [HideInInspector] public Oline[] oLine;
            [HideInInspector] public Dline[] dLine;

    void Start()
    {
        oLine = FindObjectsOfType<Oline>();
        dLine = FindObjectsOfType<Dline>();
    }

    // Update is called once per frame
    void Update()
    {
        if(ballOwner != null)
        {

        }
    }
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
  
    public void Hike() 
    {
        hikeTheBall(true);
        isHiked = true;
        ballOwner = FindObjectOfType<QB>(); //todo find better solution of getting the ball owner 
    }
    public void PassPlay()
    {
        Debug.Log("Pass Play");
        isPass = true;
        isRun = false;
    }
    public void RunPlay()
    {
        Debug.Log("Run Play");
        isRun = true;
        isPass = false;
    }
    public void ChangeBallOwner(GameObject target)
    {
        ballOwner = target;
        ballOwnerChange(target);
    }
    public void ThrowTheBall(QB ballThrower, WR ballReciever, Vector3 impactPos, float arcType, float power)
    {
        onBallThrown(ballThrower, ballReciever, impactPos, arcType, power);
    }
    public void AttemptPass(QB ballThrower, WR ballReciever,float arcType, float power)
    {
        passAttempt(ballThrower, ballReciever, arcType, power);
    }

}
