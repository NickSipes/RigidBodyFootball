﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zones : MonoBehaviour
{
    internal float zoneSize;
    private DB db;
    [System.Serializable] public enum ZoneType
    {
        Flat, Seam, DeepHalf, DeepThird, Curl, Spy, Rush
    }
    public ZoneType type;
    [HideInInspector]public Vector3 zoneCenter;
    SphereCollider sphereCollider;

    // Use this for initialization
    void Start ()
    {
        sphereCollider = gameObject.AddComponent<SphereCollider>();
    }

    // Update is called once per frame
	void Update ()
    { 
        sphereCollider.radius = zoneSize;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 4);
    }

}
