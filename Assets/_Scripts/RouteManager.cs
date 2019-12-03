using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    //todo collapse into Routes

    public Routes routes;
    private WR[] wrs;
    private WR wr;

    GameManager gameManager;

    // Use this for initialization
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        wrs = FindObjectsOfType<WR>();
        routes = this.GetComponentInChildren<Routes>();
    }
    void Update()
    {
        if (wr == null) return;

    }
    public void SetWr(WR wR)
    {
        wr = wR;
    }
}