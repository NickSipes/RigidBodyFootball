using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [HideInInspector]public RouteManager routeManager;
    private GameManager gameManager;
    private OffPlay currerntOffPlay;
 
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
    public void FlipOffPlay()
    {
        if (!gameManager.isHiked)
        {
            Debug.Log("FlipPlay");
            gameManager.FlipPlay();
        
        }

    }

}

public class DefPlay : PlayCall
{
    public List<Transform> formationTransforms;

    void Start()
    {
        base.Start();
        GetFormationPositions();
    }
    void GetFormationPositions()
    {

        foreach (var transform1 in formationTransforms)
        {
            Debug.Log(transform1.name);
        }

    }
}