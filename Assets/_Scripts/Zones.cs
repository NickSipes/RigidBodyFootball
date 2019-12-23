using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zones : MonoBehaviour
{
    public static DefPlayer myDefPlayer;
    internal float zoneSize;
    private Color zoneColor;
    [HideInInspector] public Vector3 zoneCenter;
    SphereCollider sphereCollider;
    //todo, these should be lists with assignable values in inspector
    [System.Serializable]
    public enum ZoneType
    {
        Man, Flat, Seam, DeepHalf, DeepThird, Curl, Spy, Rush
    }
    public ZoneType type;

    // Use this for initialization
    void Start()
    {
        ZoneTypeSwitch();
        zoneCenter = transform.position;
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
    }

    private void ZoneTypeSwitch()
    {
        //todo I would love to do some kind like fill in the box math here
        switch (type)
        {
            case ZoneType.Curl:
                zoneColor = Color.yellow;
                zoneSize = 5;
                break;
            case ZoneType.DeepHalf:
                zoneColor = Color.blue;
                zoneSize = 10;
                break;
            case ZoneType.DeepThird:
                zoneColor = Color.white;
                zoneSize = 9;
                break;
            case ZoneType.Flat:
                zoneColor = Color.cyan;
                zoneSize = 6;
                break;
            case ZoneType.Seam:
                zoneColor = Color.green;
                zoneSize = 6;
                break;
            case ZoneType.Rush:
                zoneColor = Color.red;
                zoneSize = 1;
                break;
            case ZoneType.Spy:
                zoneColor = Color.gray;
                zoneSize = 1;
                break;
            case ZoneType.Man:
                zoneColor = Color.black;
                zoneSize = 1;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        sphereCollider.radius = zoneSize;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = zoneColor;
        Gizmos.DrawWireSphere(transform.position, zoneSize);
    }

    public void SetDefPlayer(DefPlayer defPlayer)
    {
        myDefPlayer = defPlayer;
    }

}

