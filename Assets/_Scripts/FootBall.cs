using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FootBall : MonoBehaviour
{

    //todo Make function to get football time to targetPlayer
    // distance from qb to impactPos 
    // speed is velocity
    // time = distance/speed

    [SerializeField]
    private Rigidbody rb;
    GameManager gameManager;
    [SerializeField] [Range(0, 1)] private float arcPeakRange;
    [SerializeField] [Range(0, 1)] private float throwPowerRange;

    public float arcPeak { get { return Mathf.Lerp(.1f, 10f, arcPeakRange); } }
    public float throwPower { get { return Mathf.Lerp(1f, 30f, throwPowerRange); } }

    [SerializeField] private GameObject targetMarker;
    public bool isComplete;
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

    public void PassFootBallToMovingTarget(QB ballThrower, WR wideReceiver,FootBall footBall,float arcType, float power) 
    {
        isComplete = true;
        SetGameManager();
        gameManager.AttemptPass(ballThrower, wideReceiver, this, arcType, power); //todo, this is ugly. Probably should be a bool for isComplete
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
     
        }

        //targetPos = GetPositionIn(2, wr);
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
            gameManager.ThrowTheBall(ballThrower, wideReceiver, this, impactPos, arcType, power, isComplete); //todo the football stores whether the pass is complete or not, not sure if thats a good idea.
        }
        //Debug.Log("Firing at " + impactPos);
      
       

    }

    public void BlockBallTrajectory()
    {
        gameManager.TipDrill(); //tdoo 

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();

        }
     
        //todo this code is used three times now REFACTOR
        transform.parent = null;
        rb.useGravity = true;
        BallisticMotion motion = GetComponent<BallisticMotion>();
        Vector3 targetPos = transform.position + new Vector3(5,5,0);
        Vector3 diff = targetPos - transform.position;
        Vector3 diffGround = new Vector3(diff.x, 0f, diff.z);
        Vector3 fireVel, impactPos;
        Debug.Log("BeBlocked Ballistics");
        //FTS Calculations https://github.com/forrestthewoods/lib_fts/tree/master/projects/unity/ballistic_trajectory
        float gravity;

        if (Ballistics.solve_ballistic_arc_lateral(transform.position, 1, targetPos, transform.position + new Vector3(10,0,0), 10,
            out fireVel, out gravity, out impactPos))
        {
      
            transform.forward = diffGround;
            motion.Initialize(transform.position, gravity);
            motion.AddImpulse(fireVel);
            
        }
        //Debug.Log("Blocked at " + impactPos);



    }



}



/*
 *  function BallisticVel(targetPlayer: Transform, angle: float): Vector3 {
     var dir = targetPlayer.position - transform.position;  // get targetPlayer direction
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
 
 var myTarget: Transform;  // drag the targetPlayer here
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
