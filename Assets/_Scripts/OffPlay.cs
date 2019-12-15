using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable ArrangeTypeMemberModifiers
#pragma warning disable 108,114

public class OffPlay : PlayCall
{
    public int[] wrRoutes;
  
    public bool[] isSkillPlayerBlock;
    public int[] HbRoute;

    public int[] TeRoute;
    public List<Transform> formationTransforms;

    //public int Wr5Route;

    void Start()
    {
        base.Start();
        //GetFormationPositions();
    }
    void GetFormationPositions()
    {

        foreach (var transform1 in formationTransforms)
        {
            Debug.Log(transform1.name);
        }

    }

}