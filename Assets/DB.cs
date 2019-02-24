using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class DB : MonoBehaviour
{
    private WR[] wideRecievers;
    private AICharacterControl aiCharacter;
    private Transform target;
    public Transform startTarget;
    [Range(5, 10)]
    public float zoneSize;
    public Vector3 zoneCenter;
    public GameObject zoneCenterGO;
    public WR targetWr;


    [SerializeField]private bool isMan;
    [SerializeField] private bool isZone;
    GameManager gameManager;
    private bool isPressing;
    bool isHiked = false;
    // Use this for initialization
    void Start ()
    {
        gameManager = FindObjectOfType<GameManager>();
        wideRecievers = FindObjectsOfType<WR>();
        aiCharacter = GetComponent<AICharacterControl>();
        zoneCenter = transform.position + new Vector3(0, 0, 5);
        zoneCenterGO = Instantiate(new GameObject(), zoneCenter, Quaternion.identity);
            
    }
	//todo DB State Machine
	// Update is called once per frame
	void Update () {
        if (!gameManager.isHiked)
            return;

        if (startTarget == null)
        {
            //todo this code will break zone coverage later
            startTarget = GetClosestEnemy(wideRecievers);
            SetTarget(startTarget);
            if (targetWr == null)
            {
                Debug.Log("WR not found from DB set");
                return;
            }

            if (targetWr.CanBePressed()) 
            StartCoroutine(WrPress(targetWr));
        }
        if (isMan) { }

        if (isZone && isPressing == false) 
        {
            PlayZone();
        }
        
	}

    IEnumerator WrPress(WR wr)
    {
        float pressTime = 1f; // 3 seconds you can change this 
        //to whatever you want
        float pressTimeNorm = 0;
        while (pressTimeNorm <= 1f)
        {
            isPressing = true;
            pressTimeNorm += Time.deltaTime / pressTime;
            wr.Press(pressTimeNorm);
            yield return new WaitForEndOfFrame();
        }
        wr.ReleasePress();
        isPressing = false;


    }

    private void SetTarget(Transform targetTransform)
    {
        aiCharacter.target = targetTransform;
        target = targetTransform;
        targetWr = targetTransform.GetComponentInParent<WR>();
        //Debug.Log("Target Changed");
    }
    private void SitInZone(Transform targetTransform)
    {
        aiCharacter.target = targetTransform;
        target = targetTransform;
        targetWr = null;
        //Debug.Log("Target Changed");
    }
    Transform GetClosestEnemy(WR[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (WR potentialTarget in enemies)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
                targetWr = potentialTarget;
            }
        }

        return bestTarget;
    }
    Transform CheckZones(WR[] enemies)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (WR potentialTarget in enemies)
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

    void PlayZone()
    {
        //todo access WR route to see if it will pass through zone and then move towards intercept point
        if (targetWr == null)
        {
           var possibleEnemy = CheckZones(wideRecievers);
           Vector3 wrZoneCntrDist = possibleEnemy.position - zoneCenter;
           //Debug.Log(wrZoneCntrDist.magnitude);
           if (wrZoneCntrDist.magnitude < zoneSize)
           {
               
               SetTarget(possibleEnemy);
           }
        }
        else
        {
            Vector3 wrDistanceFromZoneCenter = targetWr.transform.position - zoneCenter;
            if (wrDistanceFromZoneCenter.magnitude > zoneSize)
            {
                if (target != zoneCenterGO.transform)
                {
                    SitInZone(zoneCenterGO.transform);
                }

            }
        }





        //bool inWeaponCircle = distanceToPlayer <= currentWeaponRange;
        //bool inChaseRing = distanceToPlayer > currentWeaponRange
        //                   &&
        //                   distanceToPlayer <= chaseRadius;
        //bool outsideChaseRing = distanceToPlayer > chaseRadius;

    }
    void OnDrawGizmos()
    {
        // Draw zone sphere 
        Gizmos.color = new Color(0, 0, 255, .5f);
        Gizmos.DrawWireSphere(zoneCenter, zoneSize);
    }
}
