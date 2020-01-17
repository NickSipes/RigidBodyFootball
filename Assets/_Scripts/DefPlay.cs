using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DefPlay : PlayCall
{
    public bool isPress;
    public List<DefJobs> formationJobs;
    internal override void Start()
    {
        base.Start();
    }

    internal void SetFormationPositions()
    {
        foreach (DefJobs defJobs in this.GetComponentsInChildren<DefJobs>())
        {
            formationJobs.Add(defJobs);
        } 
    }

    public DefJobs GetJob(DefPlayer defPlayer)
    {
        if (formationJobs.Count == 0)
        {
            SetFormationPositions();
        }
        var defPlayerName = defPlayer.name;
        foreach (DefJobs defJobs in formationJobs)
        {
            //todo string reference to hierarchy
            if (defJobs.transform.name == defPlayerName)
            {
                defJobs.SetDefPlayer(defPlayer);
                return defJobs;
                //Debug.Log(myDefJob.name + " " + defPlayer.name + " set");
            }
        }
        return null;
    }
}