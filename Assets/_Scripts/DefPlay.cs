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
}