using UnityEngine;

[System.Serializable]
public class Routes : MonoBehaviour
{
    private LineRenderer lineRenderer;
    const float waypointGizmoRadius = 0.3f;
    public float[] routeCutDwellTime;

    void Start()
    {
        var variableForPrefab = Resources.Load("_prefabs/PlayArt/Yellow") as Material;
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = variableForPrefab;

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
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            int j = GetNextIndex(i);
            Debug.DrawLine(GetWaypoint(i), GetWaypoint(j));
           
        }
        
    }

}
