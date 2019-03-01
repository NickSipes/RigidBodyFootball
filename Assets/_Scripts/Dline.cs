using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Dline : FootBallAthlete
{

    public bool wasBlocked = false;
    [SerializeField] Image blockBar;
    // Start is called before the first frame update
    void Start()
    {
        FindComponents();
    }

  
    private void FindComponents()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        aiCharacter = GetComponent<AICharacterControl>();
        userControl = GetComponent<ThirdPersonUserControl>();
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

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked) return;
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        SetTarget(target);
    }

    public void SetTarget(Transform targetSetter)
    {
        if (aiCharacter.enabled == false)
        { aiCharacter.enabled = true; }
        aiCharacter.target = targetSetter;
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
    }

    public void ReleaseBlock()
    {
        canvas.enabled = !canvas.enabled;
        SetTarget(GameObject.FindGameObjectWithTag("Player").transform);
        wasBlocked = true;
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
    }
}
