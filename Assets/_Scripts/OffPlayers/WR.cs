
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.UIElements;


public class WR : OffPlayer
{

    internal override void Start()
    {
        base.Start();
        rayColor = Color.cyan;
        AddClickCollider();
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public override void FixedUpdate()
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
