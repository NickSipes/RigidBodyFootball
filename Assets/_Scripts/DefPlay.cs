using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DefPlay : PlayCall
{
    public bool isPress;
    public List<Zones> formationZones;
    internal override void Start()
    {
        base.Start();
        SetFormationPositions();
    }

    internal void SetFormationPositions()
    {

    }

    public Zones GetJob(DefPlayer defPlayer)
    {
        var defPlayerName = defPlayer.name;
        foreach (Zones zone in formationZones)
        {
            //todo string reference to hierarchy
            if (zone.transform.name == defPlayerName)
            {
                zone.SetDefPlayer(defPlayer);
                return zone;
                //Debug.Log(myZone.name + " " + defPlayer.name + " set");
            }
        }
        return null;
    }
}