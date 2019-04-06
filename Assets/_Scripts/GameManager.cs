using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{

    //todo make gamemanger a scriptable object

    FootBallAthlete ballAthlete;
    [HideInInspector] public GameObject selector;
    public bool isRun = false;
    public bool isPass = false;
    public bool isHiked = false;
    //public Object ballOwner;
    // Start is called before the first frame update
    public CameraFollow cameraFollow;

    public delegate void HikeTheBall(bool wasHiked);
    public event HikeTheBall hikeTheBall;

    public delegate void ClearTheSelector(bool isCleared);
    public event ClearTheSelector clearSelector;

    public delegate void HikeTrigger();
    public event HikeTrigger hikeTrigger;

    public delegate void BallOwnerChange(FootBallAthlete ballOwner);
    public event BallOwnerChange ballOwnerChange;

    public delegate void PassAttempt(QB thrower, WR reciever,FootBall footBall, float arcType, float power);
    public event PassAttempt passAttempt;

    public delegate void OnBallThrown(QB thrower, WR reciever,FootBall footBall , Vector3 impactPos, float arcType, float power, bool isComplete);
    public event OnBallThrown onBallThrown;

    public delegate void ShedBlock(FootBallAthlete brokeBlock);
    public event ShedBlock shedBlock;

    [HideInInspector] public Oline[] oLine;
            [HideInInspector] public Dline[] dLine;
    internal static GameManager instance;
    public FootBallAthlete ballOwner;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("two gamemanagers");
        }
        instance = this;

    }
    void Start()
    {
        oLine = FindObjectsOfType<Oline>();
        dLine = FindObjectsOfType<Dline>();
        cameraFollow = FindObjectOfType<CameraFollow>();
    }

    internal FootBallAthlete WhoHasBall()
    {
        return ballAthlete;
    }

    // Update is called once per frame
    void Update()
    {
       // if(ballOwner != null)
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
        //ballOwner = FindObjectOfType<QB>(); //todo find better solution of getting the ball owner 
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
    public void ThrowTheBall(QB ballThrower, WR ballReciever,FootBall ball , Vector3 impactPos, float arcType, float power, bool isComplete)
    {
        onBallThrown(ballThrower, ballReciever,ball, impactPos, arcType, power, isComplete);
    }
    public void AttemptPass(QB ballThrower, WR ballReciever, FootBall ball,float arcType, float power)
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
}
