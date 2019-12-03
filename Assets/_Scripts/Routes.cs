using System;
using System.Collections.Generic;
using UnityEngine;

public class Routes : MonoBehaviour
{
    public static Routes[] allRoutes;
    public float routeCutDwellTime = .2f;

    private void OnDrawGizmos()
    {
        float routeGizmoRadius = .3f;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < transform.childCount; i++)
        {
            Gizmos.DrawSphere(GetWaypoint(i), routeGizmoRadius);
            if (i + 1 == transform.childCount)
            {
                return;
            }
            int j = GetNextIndex(i);
            Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
        }
    }

    public int GetNextIndex(int i)
    {
      
        return i +  1;
        throw new NotImplementedException();
    }

    public Vector3 GetWaypoint(int i)
    {
        return transform.GetChild(i).position;
        throw new NotImplementedException();
    }
    
}
