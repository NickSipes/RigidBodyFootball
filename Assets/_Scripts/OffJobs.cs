using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
public class OffJobs : MonoBehaviour
{
    LineRenderer lineRenderer;
    const float waypointGizmoRadius = 0.3f;
    public float[] routeCutDwellTime;
    private Transform routeStartLocation;
    [HideInInspector] public Transform[] routeCuts;
    private bool hasRoute;

    void Start()
    {
        GetRouteCuts();
        AddLineRenderer();
        //lineRenderer.widthMultiplier = .1f;
        //lineRenderer.SetPosition(0, transform.GetChild(0).position);
    }

    public Vector3 LastCutVector()
    {

        return new Vector3();
    }
    private void AddLineRenderer()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
    }

    private void GetRouteCuts()
    {
        routeCuts = GetComponentsInChildren<Transform>();
        hasRoute = true;
    }

    void Update()
    {
        if(GameManager.instance.isHiked)return;
        DrawPath();
    }
    private void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int j = GetNextIndex(i);
            Gizmos.DrawSphere(GetWaypoint(i), waypointGizmoRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
        }
    }

    public int GetNextIndex(int i)
    {
        if (i + 1 == transform.childCount)
        {
            return i;
        }
        return i + 1;
    }

    public Vector3 GetWaypoint(int i)
    {
        return transform.GetChild(i).position;
    }

    void DrawPath()
    {
        //if (!hasRoute)
        //{
        //    GetRouteCuts();
        //}
        
        //lineRenderer.positionCount = routeCuts.Length;
        //Debug.Log(this.name + " " + lineRenderer.positionCount);
        //foreach (Transform cut in routeCuts)  
        //{
           
        //}
        
    }

}
