using UnityEngine;

[System.Serializable]
public class Routes : MonoBehaviour
{
    LineRenderer lineRenderer;
    const float waypointGizmoRadius = 0.3f;
    public float[] routeCutDwellTime;
    private Transform routeStartLocation;
    private Transform[] routeCuts;

    void Start()
    {
        routeCuts = GetComponentsInChildren<Transform>();
        //lineRenderer = GetComponent<LineRenderer>();
        //lineRenderer.widthMultiplier = .1f;
        //lineRenderer.SetPosition(0, transform.GetChild(0).position);
    }

    void Update()
    {
        DrawPath();
    }
    private void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int j = GetNextIndex(i);
            Gizmos.DrawSphere(GetWaypoint(i), waypointGizmoRadius);
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
        foreach (Transform cut in routeCuts)  
        {
            
           
        }
        
    }

}
