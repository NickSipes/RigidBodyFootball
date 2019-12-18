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
    internal OffPlayer[] offPlayers;
    internal DefPlayer[] defPlayers;
    private OffPlay[] allOffPlays;
    private DefPlay[] allDefPlays;
    [HideInInspector]public RouteManager routeManager;
    [HideInInspector] public bool isFlipped = false;
    private GameManager gameManager;
    private OffPlay currentOffPlay;
    private DefPlay currentDefPlay;

    [HideInInspector]public bool isPass;

    // Start is called before the first frame update
    internal virtual void Start()
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

    public void ChangeOffPlay(OffPlay offPlay)//called from UI
    {
        if (!gameManager.isHiked)
        {
            gameManager.ChangeOffPlay(offPlay);
            currentOffPlay = offPlay;
        }
     
    }
    public void FlipOffPlay()//called from UI
    {
        //todo need to copy the playCall to a new object because of how flipping assigns transforms of route cuts
        if (gameManager.isHiked) return;
        if (isFlipped)
        {
            ChangeOffPlay(currentOffPlay);
            isFlipped = false;
        }
        Debug.Log("FlipPlay");
        gameManager.FlipPlay();
        isFlipped = true;

    }
   
    public void ChangeDefPlay(DefPlay defPlay)//called from UI
    {
        if (!gameManager.isHiked)
        {
            gameManager.ChangeDefPlay(defPlay);
            currentDefPlay = defPlay;
        }
    }

}
