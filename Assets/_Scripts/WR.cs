using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class WR : FootBallAthlete
{
    // Use this for initialization
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        qb = FindObjectOfType<QB>();
        defBacks = FindObjectsOfType<DB>();
        rb = GetComponent<Rigidbody>();
        startColor = materialRenderer.material.color;
        aiCharacter = GetComponent<AICharacterControl>();
        startGoal = aiCharacter.target;
        aiCharacter.target = null;
        target = startGoal;
        //lr.material.color = LineColor;
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked)
            return;
        if (gameManager.isRun)
        {
            Transform blockTarget = GetClosestDB(defBacks);
            SetTarget(blockTarget);
            if (targetDb.CanBePressed())
                StartCoroutine(DbBlock(targetDb));
            return;
        }
        
        if (Input.anyKey)
        {
        BeginPass();
        }
        SetTarget();
        
    }

    private void SetTarget()
    {
        if (aiCharacter.target == null)
        {
            aiCharacter.target = startGoal;
        }

        if (navMeshAgent.hasPath && navMeshAgent.remainingDistance < 2 && target != null)
        {
            var ball = target.root.GetComponent<FootBall>();

            if (ball != null)
            {
                Destroy(ball);
                aiCharacter.target = startGoal;
            }


        }

        //DrawPath();
    }

    private void BeginPass()
    {
        if (materialRenderer.material.color != startColor)
        {
            //todo adjust throwPower dependent on distance and throw type
            if (Input.GetMouseButtonDown(0)) //Bullet Pass
            {
                Debug.Log("Pressed secondary button.");
                passTarget = transform.position;
                qb.Throw(passTarget, this, 1.5f, 23f);

            }

            if (Input.GetMouseButtonDown(1)) // Touch Pass
            {
                Debug.Log("Pressed secondary button.");
                passTarget = transform.position;
                qb.Throw(passTarget, this, 2.3f, 20f);
            }

            if (Input.GetMouseButtonDown(2)) // Lob PASS
            {
                //Debug.Log("Pressed middle click.");
                passTarget = transform.position;
                qb.Throw(passTarget, this, 3.2f, 19.5f);
            }
        }
        canvas.transform.LookAt(Camera.main.transform);
    }
    IEnumerator DbBlock(DB db)
    {
        Debug.Log("Block DB");
        float pressTime = 1f; // 3 seconds you can change this 
        //to whatever you want
        float pressTimeNorm = 0;
        while (pressTimeNorm <= 1f)
        {
            isBlocking = true;
            pressTimeNorm += Time.deltaTime / pressTime;
            db.Press(pressTimeNorm);
            pressBar.fillAmount = pressTimeNorm;
            yield return new WaitForEndOfFrame();
        }
        db.ReleasePress();
        isBlocking = false;


    }
    Transform GetClosestDB(DB[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (DB potentialTarget in enemies)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
                targetDb = potentialTarget;
            }
        }

        return bestTarget;
    }

    void OnMouseEnter() 
    {
        materialRenderer.material.color = highlightColor;
    }

    void OnMouseDown()
    {
      

    }

    void OnMouseExit()
    {
        materialRenderer.material.color = startColor;
    }

    public void PrintPosition()
    {
        Debug.Log("WR position " + transform.position + "" +
                  "Nav Velocity " + navMeshAgent.velocity + "" +
                  "RB Velocity " + rb.velocity);
    }

    public void SetTarget(Transform targetSetter)
    {
        aiCharacter.target = targetSetter;
        target = targetSetter;
    }

    //void DrawPath()
    //{
        
    //    Vector3[] path = navMeshAgent.path.corners;
    //    if (path != null && path.Length > 1)
    //    {
    //        lr.positionCount = path.Length;
    //        for (int i = 0; i < path.Length; i++)
    //        {
    //            lr.SetPosition(i, path[i]);
    //        }
    //    }
    //}

    public bool CanBePressed()
    {
        if (!beenPressed)
        {
            beenPressed = true;
            return true;
        }

        return false;
    }

    public void Press(float pressTimeNorm)
    {
        pressBar.fillAmount = pressTimeNorm;
        navMeshAgent.acceleration = 0f;
        navMeshAgent.speed = 0f;
    }

    public void ReleasePress()
    {
        canvas.enabled = !canvas.enabled;
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
    }


}
