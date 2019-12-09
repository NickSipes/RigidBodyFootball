using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
// ReSharper disable All

public class DB : DefPlayer
{

    //todo create class between Off and Def based on FootballAthlete


    //todo HOW ARE WE GOING TO HANDLE JUMP ANIMATIONS
    //todo defenders need to determine LOS and where to lineup
    //todo DB State Machine
    //todo readdress how pass incomlpetion are cacluated. All determining factors should be rolls vs stats ?
    //todo scramble mechanics
    //todo Man coverage
    // Use this for initialization
    void Start()
    {
        base.Start();
        rayColor = Color.yellow;
        gameManager.hikeTheBall += HikeTheBall;
        gameManager.onBallThrown += BallThrown;
        gameManager.passAttempt += PassAttempt;
        CreateZone(); //todo only run if player is in zone
    }

    private void CreateZone()
    {
        //todo make zoneCenterGO move functions dependent on play developement;
        Vector3 zoneCenterStart = GetZoneStart();
        GameObject zoneGO = Instantiate(new GameObject(), zoneCenterStart, Quaternion.identity);
        zone = zoneGO.AddComponent<Zones>();
        zoneGO.transform.position = transform.position + new Vector3(0, 0, 5);
        zoneGO.transform.name = transform.name + "ZoneObject";
        GameObject zoneObjectContainer = GameObject.FindGameObjectWithTag("ZoneObject"); //Hierarchy Cleanup
        zoneGO.transform.parent = zoneObjectContainer.transform;
        zoneGO.transform.tag = "ZoneObject";
        SphereCollider sphereCollider = zoneGO.gameObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        zoneGO.layer = zoneLayer;
        zone.zoneSize = defZoneSize;
    }

    private Vector3 GetZoneStart()
    {
        return transform.position + new Vector3(5, 0, 0); //todo this needs to be dependent on player responsibilities
    }

    private void HikeTheBall(bool wasHiked)
    {
        anim.SetTrigger("HikeTrigger");
        if (gameManager.isRun)
        {

        }

        if (gameManager.isPass)
        {
            //todo move press code here!
            var potientialTarget = GetClosestWr(wideRecievers);
            if ((potientialTarget.transform.position - transform.position).magnitude < 5f)
            {
                SetTargetWr(potientialTarget);
                if (targetReciever.CanBePressed())
                    StartCoroutine(WrPress(targetReciever));
                //todo press range variable
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.isHiked)
            return;

        if (gameManager.isRun)
        {
            PlayReact();

        }

        // ReSharper disable once InvertIf
        if (gameManager.isPass)
        {
            if (isBlockingPass) return;

            if (isPressing) return;

            if (isZone)
            {
                //todo this whole triple if statement sucks
                if (targetReciever != null)
                {
                    if (IsTargetInZone(targetReciever.transform))
                    {
                        SetDestination(targetReciever.transform.position);
                        return;
                    }
                    else
                    {
                        //Debug.Log("targetPlayer out of zone");
                        targetReciever = null;
                        targetPlayer = null;
                    }
                }

                PlayZone();
            }

            // ReSharper disable once InvertIf
        }

    }

    private void PlayReact()
    {
        var reactTime = 0f;
        while (reactTime < 1.5f)
        {
            reactTime += Time.deltaTime;
        }

        targetPlayer = GetClosestHb(hbs);
        SetTargetHb(targetPlayer);
    }

#pragma warning disable 108,114
    void FixedUpdate()
    {
        base.FixedUpdate();

    }

    private void SetTargetWr(Transform targetTransform)
    {
        EnableNavMeshAgent();
        SetDestination(targetTransform.position);
        targetPlayer = targetTransform;
        targetReciever = targetTransform.GetComponentInParent<WR>();
        //Debug.Log("Target Changed");
    }

    void PlayZone()
    {
        //todo access WR route to see if it will pass through zone and then move towards intercept point
        if (targetReciever == null)
        {
            //has return if enemy set
            if (CheckZone()) return;
        }

        SetDestination(zone.zoneCenter);

    }

    private bool CheckZone()
    {
        var possibleEnemy = GetClosestWr(wideRecievers);
        Vector3 wrZoneCntrDist = possibleEnemy.position - zone.transform.position;
        //Debug.Log(wrZoneCntrDist.magnitude);
        if (wrZoneCntrDist.magnitude < zone.zoneSize)
        {
            //todo this will break with mulitple recievers, need to determine coverage responsablilty
            SetTargetWr(possibleEnemy);
            return true;
        }

        foreach (var reciever in wideRecievers)
        {
            RaycastHit[] hits = Physics.RaycastAll(reciever.transform.position, reciever.transform.forward, 100.0F);
            //if(hits.Length != 0)Debug.Log(hits.Length);

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider.tag == "ZoneObject")
                {
                    Zones zoneObject = hit.collider.gameObject.GetComponent<Zones>();

                    if (zoneObject == zone)
                    {
                        //Maybe Method
                        SetDestination(hit.point);
                        Debug.DrawLine(transform.position, hit.point, Color.red);
                        return true;

                    }

                }

            }
        }

        return false;

    }


    IEnumerator WrPress(OffPlayer offPlayer)
    {
        isPressing = true;
        float pressTime = .5f;
        anim.SetTrigger("PressTrigger");
        SetDestination(offPlayer.transform.position + transform.forward);
        float pressTimeNorm = 0;
        //Vector3 dir = (offPlayer.transform.position - transform.position).normalized * pressTime;
        //rb.velocity = dir;
        while (pressTimeNorm <= 1f)
        {
            StayInfront();
            pressTimeNorm += Time.deltaTime / pressTime;
            offPlayer.Press(pressTimeNorm);
            yield return new WaitForEndOfFrame();
        }

        anim.SetTrigger("ReleaseTrigger");
        StartCoroutine("BackOffPress", offPlayer);
        offPlayer.ReleasePress();
        isPressing = false;

    }

    IEnumerator BackOffPress(WR wr)
    {
        //read receiver route, move backwards, release reciever to new defender, moves towards next receiver
        while ((transform.position - zone.transform.position).magnitude > 1)
        {
            //Vector3 dir = targetPlayer.position - transform.position;
            if (targetPlayer != null)
            {
                BackOff(wr);
            }

            yield return new WaitForFixedUpdate();
        }

        StartCoroutine("TurnTowardsLOS");
        anim.SetTrigger("InZoneTrigger");

        //todo this needs a stat machine to determine if the DB needs to chase the WR past the Zone, Does he have overhead help
        //Debug.Log("zone center reached");
        //Vector3 moveLeft = transform.position + new Vector3(2, 0, 0);
        //while ((transform.position - moveLeft).magnitude > 1)
        //{
        //    float speed = 3;
        //    Vector3 dir = (moveLeft - transform.position).normalized * speed;
        //    rb.velocity = dir;
        //    int velocity = 100;
        //    float angle;
        //    var targetDir = qb.transform.position - transform.position;
        //    var forward = transform.forward;
        //    var localTarget = transform.InverseTransformPoint(qb.transform.position);
        //    angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        //    Vector3 eulerAngleVelocity = new Vector3(0, angle, 0);
        //    Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime);
        //    rb.MoveRotation(rb.rotation * deltaRotation);
        //    yield return new WaitForFixedUpdate();
        //}
        //DisableNavmeshAgent();
        //navMeshAgent.ResetPath();
    }

    IEnumerator TurnTowardsLOS()
    {
        //todo write this function
        yield return new WaitForFixedUpdate();
    }

    private void StayInfront()
    {
        DisableNavmeshAgent();

        float speed = 3; //todo needs to be a calculation of WR release and DB press
        Vector3 dir = (targetPlayer.position + targetPlayer.transform.forward - transform.position).normalized * speed;
        float v = dir.z;
        float h = dir.x;

        anim.SetFloat("VelocityX", h * speed);
        anim.SetFloat("VelocityZ", v * speed);
        //rb.velocity = new Vector3(h * speed, 0, v * speed);
        //Vector3 tempVect = new Vector3(h, 0, v);
        //tempVect = tempVect.normalized * speed * Time.deltaTime;
        rb.velocity = dir;
    }



    private void BackOff(WR wr)
    {


        DisableNavmeshAgent();

        // turn around and run, cut left, or cut right

        float speed = 5; //todo this should be a calculation of offPlayer release vs db press
        Vector3 dir = (targetPlayer.position + targetPlayer.transform.forward - transform.position).normalized * speed;
        float v = dir.z;
        float h = dir.x;

        anim.SetFloat("VelocityX", h * speed);
        anim.SetFloat("VelocityZ", v * speed);
        //rb.velocity = new Vector3(h * speed, 0, v * speed);
        //Vector3 tempVect = new Vector3(h, 0, v);
        //tempVect = tempVect.normalized * speed * Time.deltaTime;
        rb.velocity = dir;
    }


}