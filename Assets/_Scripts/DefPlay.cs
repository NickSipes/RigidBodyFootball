using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DefPlay : PlayCall
{
    public List<Zones> formationZones;

    internal override void Start()
    {
        base.Start();
        SetFormationPositions();
    }

    internal void SetFormationPositions()
    {
        foreach (var defPlayer in defPlayers)
        {
            
        }
    }

    public Zones GetJob(DefPlayer defPlayer)
    {
        var defPlayerName = defPlayer.name;
        foreach (Zones zone in formationZones)
        {
            //todo string reference to hierarchy
            if (zone.transform.name == defPlayerName)
            {

                return zone;
                //Debug.Log(zone.name + " " + defPlayer.name + " set");
            }
        }
        return null;
    }
}