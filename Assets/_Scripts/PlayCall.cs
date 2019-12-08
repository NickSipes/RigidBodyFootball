using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCall : MonoBehaviour
{

    [HideInInspector] public QB[] quaterBacks;
    [HideInInspector] public WR[] wideRecievers;
    [HideInInspector] public DB[] defBacks;
    [HideInInspector] public HB[] hbs;
    [HideInInspector] public Oline[] oLine;
    private OffPlayer[] offPlayers;
    private DefPlayer[] defPlayers;
    private OffPlay[] allOffPlays;
    private DefPlay[] allDefPlays;
    public RouteManager routeManager;
    private GameManager gameManager;

    public bool isPass;

    // Start is called before the first frame update
    internal void Start()
    {
        quaterBacks = FindObjectsOfType<QB>();
        wideRecievers = FindObjectsOfType<WR>();
        defBacks = FindObjectsOfType<DB>();
        hbs = FindObjectsOfType<HB>();
        oLine = FindObjectsOfType<Oline>();
        offPlayers = FindObjectsOfType<OffPlayer>();
        defPlayers = FindObjectsOfType<DefPlayer>();
        allDefPlays = FindObjectsOfType<DefPlay>();
        allOffPlays = FindObjectsOfType<OffPlay>();
        routeManager = FindObjectOfType<RouteManager>();
        gameManager = FindObjectOfType<GameManager>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeOffPlay(OffPlay offPlay)
    {
        if (!gameManager.isHiked)
        {
            gameManager.ChangeOffPlay(offPlay);
        }
     
    }

}

public class DefPlay : PlayCall
{

}