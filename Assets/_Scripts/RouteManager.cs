using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    //todo collapse into waypoint follower

    private WR[] wrs;
    private WR wr;

    GameManager gameManager;

    // Use this for initialization
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        wrs = FindObjectsOfType<WR>();
    }

    public void SetWr(WR wR)
    {
        wr = wR;
    }
}