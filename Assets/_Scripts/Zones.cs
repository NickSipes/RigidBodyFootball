using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zones : MonoBehaviour
{
    internal float zoneSize;
    private DB db;
    [SerializeField] enum zoneType
    {
        flat, seam, deepHalf, deepThird, Curl, spy
    }
    [HideInInspector]public Vector3 zoneCenter;
    SphereCollider sphereCollider;

    // Use this for initialization
    void Start ()
    {
        zoneCenter = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        if (!sphereCollider)
        {
            sphereCollider = GetComponent<SphereCollider>();
        }
        sphereCollider.radius = zoneSize;
        zoneCenter = transform.position;
    }
    public void SetDB(DB dB)
    {
        db = dB;
    }
    public DB GetDB()
    {
        return db;
    }
}
