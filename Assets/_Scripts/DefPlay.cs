using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefPlay : PlayCall
{
    public List<Transform> formationTransforms;

    internal override void Start()
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