using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public WaypointFollower wayPointFollower;
    private WR wr;
	// Use this for initialization
	void Start ()
    {
        wr = FindObjectOfType<WR>();
        wayPointFollower = GetComponent<WaypointFollower>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (wr.target == transform)
        {
            wayPointFollower.currentSpeed = wr.navMeshAgent.speed;
        }
       
        
    }
    void OnDestroy()
    {
        if (wr != null)
        wr.SetTarget(wr.startGoal);
    }
}
