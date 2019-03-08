using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FootBall : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    GameManager gameManager;
    [SerializeField] [Range(0, 1)] private float arcPeakRange;
    [SerializeField] [Range(0, 1)] private float throwPowerRange;

    public float arcPeak { get { return Mathf.Lerp(.1f, 10f, arcPeakRange); } }
    public float throwPower { get { return Mathf.Lerp(1f, 30f, throwPowerRange); } }

    [SerializeField] private GameObject targetMarker;

    //private bool isDestroying = false;
    public float destroyTime = 3f;
    
    // Use this for initialization
    void Start ()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
     
    }
	
	// Update is called once per frame
	void Update () {

        
	}

    void SetGameManager()
    {
        if(!gameManager)
        gameManager = FindObjectOfType<GameManager>();
    }
    public void PassFootBallToMovingTarget(QB ballThrower, WR wideReceiver, float arcType, float power) 
    {
        gameManager.AttemptPass(ballThrower, wideReceiver, arcType, power);
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
     
        }
        //targetPos = GetPositionIn(2, wr);
        SetGameManager();
        transform.parent = null;
        rb.useGravity = true;
        BallisticMotion motion = GetComponent<BallisticMotion>();
        Vector3 targetPos = wideReceiver.transform.position;
        Vector3 diff = targetPos - transform.position;
        Vector3 diffGround = new Vector3(diff.x, 0f, diff.z);
        Vector3 fireVel, impactPos;
        Vector3 velocity = wideReceiver.navMeshAgent.velocity;

        //FTS Calculations https://github.com/forrestthewoods/lib_fts/tree/master/projects/unity/ballistic_trajectory
        float gravity;
        
        if (Ballistics.solve_ballistic_arc_lateral(transform.position, power, targetPos + Vector3.up, velocity, arcType,
            out fireVel, out gravity, out impactPos))
        {
            GameObject go = Instantiate(targetMarker, impactPos, Quaternion.LookRotation(ballThrower.transform.position + new Vector3(0,1,0)));
            Destroy(go, 2);
            transform.forward = diffGround;
            motion.Initialize(transform.position, gravity);
            motion.AddImpulse(fireVel);
            gameManager.ThrowTheBall(ballThrower, wideReceiver,impactPos, arcType, power);
        }
        Debug.Log("Firing at " + impactPos);
      
       

    }

    /// <summary>
    /// Calculates the position of this enemy in x amount of seconds and returns it.
    /// </summary>
    /// <param name="seconds">Amount of seconds we're predicting ahead.</param>
    /// <returns></returns>

    public Vector3 GetPositionIn(float seconds, WR wr)
    {
        // Setup variables
        NavMeshPath path = wr.navMeshAgent.path;
        float distanceExpected = wr.navMeshAgent.velocity.magnitude * seconds;
        int expectedTargetWaypoint = 1;
        bool isPredictingFurtherThanTargetWaypoint = false;
        float distanceToCover = 0;

        while (true)
        {
            // if statement prevents the error when using expectedTargetWaypoint - 1.
            if (expectedTargetWaypoint == 0)
            {
                // Distance from start to first waypoint
                distanceToCover = Vector3.Distance(transform.position, path.corners[expectedTargetWaypoint]);
            }
            else
            {
                // Distance from current Waypoint to the Next
                distanceToCover = Vector3.Distance(path.corners[expectedTargetWaypoint - 1], path.corners[expectedTargetWaypoint]);
            }

            // Will you make it to the next waypoint?
            if (distanceExpected - distanceToCover > 0)
            {
                distanceExpected -= distanceToCover;
                expectedTargetWaypoint++;
                isPredictingFurtherThanTargetWaypoint = true;

                // Made it to the finish, return finish point.
                if (expectedTargetWaypoint >= path.corners.Length)
                    return path.corners[path.corners.Length - 1];
            }
            else
            {
                // return the position where we expect this enemy to be after xSeconds at this constant speed.
                // if statement prevents the error when using expectedTargetWaypoint - 1.
                if (!isPredictingFurtherThanTargetWaypoint)
                {
                    Debug.Log("Distance Expected: " + distanceExpected + ", Distance to Cover: " + distanceToCover + ", result: " + (distanceExpected / distanceToCover));

                    // return a point between A and B depending on the percentage of its full distance we expect to cover.
                    return Vector3.Lerp(transform.position, path.corners[expectedTargetWaypoint], distanceExpected / distanceToCover);
                }
                else
                {
                    Debug.Log("Distance Expected: " + distanceExpected + ", Distance to Cover: " + distanceToCover + ", result: " + (distanceExpected / distanceToCover));
                    // return a point between A and B depending on the percentage of its full distance we expect to cover.
                    return Vector3.Lerp(path.corners[expectedTargetWaypoint - 1], path.corners[expectedTargetWaypoint], distanceExpected / distanceToCover);
                }
            }
        }
    }


}



/*
 *  function BallisticVel(target: Transform, angle: float): Vector3 {
     var dir = target.position - transform.position;  // get target direction
     var h = dir.y;  // get height difference
     dir.y = 0;  // retain only the horizontal direction
     var dist = dir.magnitude ;  // get horizontal distance
     var a = angle * Mathf.Deg2Rad;  // convert angle to radians
     dir.y = dist * Mathf.Tan(a);  // set dir to the elevation angle
     dist += h / Mathf.Tan(a);  // correct for small height differences
     // calculate the velocity magnitude
     var vel = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
     return vel * dir.normalized;
 }
 
 var myTarget: Transform;  // drag the target here
 var cannonball: GameObject;  // drag the cannonball prefab here
 var shootAngle: float = 30;  // elevation angle
 
 function Update(){
     if (Input.GetKeyDown("b")){  // press b to shoot
         var ball: GameObject = Instantiate(cannonball, transform.position, Quaternion.identity);
         ball.rigidbody.velocity = BallisticVel(myTarget, shootAngle);
         Destroy(ball, 10);
     }
 }
 

    https://answers.unity.com/questions/148399/shooting-a-cannonball.html



      private void FireCannonAtPoint(Vector3 point)
    {
        var velocity = BallisticVelocity(point, angle);
        Debug.Log("Firing at " + point + " velocity " + velocity);

        cannonballInstance.transform.position = transform.position;
        cannonballInstance.velocity = velocity;
    }

    private Vector3 BallisticVelocity(Vector3 destination, float angle)
    {
        Vector3 dir = destination - transform.position; // get Target Direction
        float height = dir.y; // get height difference
        dir.y = 0; // retain only the horizontal difference
        float dist = dir.magnitude; // get horizontal direction
        float a = angle * Mathf.Deg2Rad; // Convert angle to radians
        dir.y = dist * Mathf.Tan(a); // set dir to the elevation angle.
        dist += height / Mathf.Tan(a); // Correction for small height differences

        // Calculate the velocity magnitude
        float velocity = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
        return velocity * dir.normalized; // Return a normalized vector.
    }
    https://unity3d.college/2017/06/30/unity3d-cannon-projectile-ballistics/
    */
