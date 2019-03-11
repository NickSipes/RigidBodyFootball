using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    //todo collapse into waypoint follower

    public WaypointFollower wayPointFollower;
    private WR[] wrs;
    private WR wr;
    GameManager gameManager;
    // Use this for initialization
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        wrs = FindObjectsOfType<WR>();
        wayPointFollower = GetComponent<WaypointFollower>();
    }

    // Update is called once per frame
    void Update()
    {
        if (wr == null) return;

        if (wr.target == transform)
        {
            wayPointFollower.currentSpeed = wr.navMeshAgent.speed;
        }


    }
    public void SetWr(WR wR)
    {
        wr = wR;
    }
}
