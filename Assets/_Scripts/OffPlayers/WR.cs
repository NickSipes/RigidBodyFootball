
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.UIElements;
// ReSharper disable ArrangeTypeMemberModifiers
#pragma warning disable 108,114

public class WR : OffPlayer
{
    //todo rapid fire
    // Use this for initialization
    

    void Start()
    {
        base.Start();
        rayColor = Color.cyan;
        gameManager.offPlayChange += ChangeOffRoute;
        AddClickCollider();
        //start goal set by inspector
    }


    // Update is called once per frame
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void Update()
    {

        if (gameManager.WhoHasBall() == this) return;
        if (isCatching) return;

        if (!gameManager.isHiked)
        {
            if (gameManager.isRun) return;
            if (gameManager.isPass)
            {
               return;
            }
        }

        if (wasAtLastCut)
        {
            WatchQb();
            //Debug.Log("last cut");
            return;
        }

        if (gameManager.isRun)
        {
            canvas.transform.LookAt(Camera.main.transform);
            Transform blockTarget = GetClosestDB(defBacks);
            isBlocker = true;
            targetDb = blockTarget.GetComponent<DB>();
            navMeshAgent.isStopped = false;
            SetDestination(blockTarget.transform.position);
            if (targetDb.CanBePressed())
                StartCoroutine("DbBlock", (targetDb));
            return;
        }

        if (gameManager.isPass)
        {

            if (IsEndOfRoute())
            {
                StopNavMeshAgent();
                Debug.Log("EndOfRoute Update");
                return;
            }

            if (!IsEndOfRoute()) RunRoute();

        }
    }

   void ChangeOffRoute(OffPlay offPlay)
    {
        if (gameManager.isRun)
        {
            if (myRoute != null) Destroy(myRoute);
            StopNavMeshAgent();
            return;
        }

        var wrName = this.name;
        switch (wrName)
        {
            case "WR1":
                routeSelection = offPlay.wrRoutes[0];
                break;
            case "WR2":
                routeSelection = offPlay.wrRoutes[1];
                break;
            case "WR3":
                routeSelection = offPlay.wrRoutes[2];
                break;
            case "WR4":
                routeSelection = offPlay.wrRoutes[3];
                break;
            default:
                routeSelection = offPlay.HbRoute[0];
                break;
        }

        Destroy(myRoute);
        GetRoute(routeSelection);
    }

  

    IEnumerator DbBlock(DefPlayer defPlayer)
    {
        Debug.Log("BeBlocked " + "");
        float pressTime = 1f; // 3 seconds you can change this 
        //to whatever you want
        float pressTimeNorm = 0;
        while (pressTimeNorm <= 1f)
        {
            isBlocking = true;
            pressTimeNorm += Time.deltaTime / pressTime;
            defPlayer.Press(pressTimeNorm);
            pressBar.fillAmount = pressTimeNorm;
            yield return new WaitForEndOfFrame();
        }
        defPlayer.ReleasePress();
        isBlocking = false;
    }


}
