using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    [SerializeField] public OffJobs[] allRoutes;
    GameManager gameManager;

    // Use this for initialization
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
}