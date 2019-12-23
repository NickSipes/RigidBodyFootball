using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{

    //todo maybe make gamemanger a scriptable object?

    FootBallAthlete ballAthlete;
    [HideInInspector] public GameObject selector;
    public GameObject UIStuff;
    [HideInInspector] public bool isRun = false;
    [HideInInspector] public bool isPass = false;
    [HideInInspector] public bool isHiked = false;
    public GameObject lineOfScrimmage; 
    [HideInInspector] public OffPlay[] allOffPlays;
    [HideInInspector] public DefPlay[] allDefPlays;
    [HideInInspector] public RouteManager routeManager;
    [HideInInspector] public OffPlay currentOffPlay;
    [HideInInspector] public DefPlay currentDefPlay;

    [HideInInspector] public OffPlayer[] offPlayers;
    [HideInInspector] public DefPlayer[] defPlayers;

    public bool isRapidfire;

    [HideInInspector] public bool isPassStarted = false;
    [HideInInspector] public CameraFollow cameraFollow;

    internal static GameManager instance;
    internal FootBallAthlete ballOwner;
    
    //Events
    public delegate void HikeTheBall(bool wasHiked);
    public event HikeTheBall hikeTheBall;

    public delegate void ClearTheSelector(bool isCleared);
    public event ClearTheSelector clearSelector;

    public delegate void HikeTrigger();
    public event HikeTrigger hikeTrigger;

    public delegate void BallOwnerChange(FootBallAthlete ballOwner);
    public event BallOwnerChange ballOwnerChange;

    public delegate void PassAttempt(QB thrower, OffPlayer reciever, FootBall footBall, float arcType, float power);
    public event PassAttempt passAttempt;

    public delegate void OnBallThrown(QB thrower, OffPlayer reciever, FootBall footBall, Vector3 impactPos, float arcType, float power, bool isComplete);
    public event OnBallThrown onBallThrown;

    public delegate void ShedBlock(FootBallAthlete brokeBlock);
    public event ShedBlock shedBlock;

    public delegate void OffPlayChange(OffPlay offPlay);
    public event OffPlayChange offPlayChange;

    public delegate void OffFlipPlay(OffPlay flipPlay);
    public event OffFlipPlay offFlipPlay;
   
    public delegate void DefPlayChange(DefPlay defPlay);
    public event DefPlayChange defPlayChange;

  
    //todo instantiate UIStuff at runtime
    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("two game-managers");
        }
        instance = this;
    }
    void Start()
    {
        offPlayers = FindObjectsOfType<OffPlayer>();
        defPlayers = FindObjectsOfType<DefPlayer>();
        cameraFollow = FindObjectOfType<CameraFollow>();
        allDefPlays = FindObjectsOfType<DefPlay>();
        allOffPlays = FindObjectsOfType<OffPlay>();
        routeManager = FindObjectOfType<RouteManager>();
    }
    internal FootBallAthlete WhoHasBall()
    {
        return ballAthlete;
    }
    // Update is called once per frame
    void Update()
    {
        if (ballOwner != null)
        {

        }
    }
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void CallForSnap()
    {
        hikeTrigger();
    }
    public void Hike()
    {
        hikeTheBall(true);
        isHiked = true;
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
    public void ChangeBallOwner(GameObject prevOwner, GameObject newOwner)
    {
        FootBallAthlete ballCarrier = newOwner.GetComponent<FootBallAthlete>();
        prevOwner.transform.tag = "OffPlayer";
        ballCarrier.SetUserControl();
        cameraFollow.ResetPlayer();
        ballOwner = ballCarrier;
        ballAthlete = newOwner.GetComponent<FootBallAthlete>();
        ballOwnerChange(ballCarrier);
    }
    public void ThrowTheBall(QB ballThrower, OffPlayer ballReciever, FootBall ball, Vector3 impactPos, float arcType, float power, bool isComplete)
    {
        onBallThrown(ballThrower, ballReciever, ball, impactPos, arcType, power, isComplete);
    }
    public void AttemptPass(QB ballThrower, OffPlayer ballReciever, FootBall ball, float arcType, float power)
    {
        passAttempt(ballThrower, ballReciever, ball, arcType, power);
    }
    public void RaiseShedBlock(FootBallAthlete brokeBlock)
    {
        shedBlock(brokeBlock);
    }
    internal void SetSelector(GameObject go)
    {
        selector = go;
    }
    internal void ClearSelector()
    {
        selector = null;
        clearSelector(true);
    }
    internal void TipDrill()
    {
        Debug.Log("tipdrill");
    }
    public void ChangeOffPlay(OffPlay offPlay)
    {
        currentOffPlay = Instantiate(offPlay, transform);
        if (currentOffPlay.isPass)
        {
            PassPlay();
            offPlayChange?.Invoke(currentOffPlay);
        }
        else
        {
            RunPlay();
            offPlayChange?.Invoke(currentOffPlay);
        }
    }
    public void FlipPlay()
    {
       offFlipPlay(currentOffPlay);
    }
    public void ChangeDefPlay(DefPlay defPlay)
    {
        currentDefPlay = Instantiate(defPlay, lineOfScrimmage.transform.position, lineOfScrimmage.transform.rotation);
        var zoneParent = GameObject.Find("ZoneObjects");
        currentDefPlay.transform.SetParent(zoneParent.transform);
        defPlayChange?.Invoke(currentDefPlay);
    }
}
