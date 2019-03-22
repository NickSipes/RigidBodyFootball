using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    GameObject player;
    GameObject football;
    internal static CameraFollow instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("two followers");
        }
        instance = this;

    }
    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
	}
	//todo add a function to zoom in on qb when ball is thrown to easy transistion.
	// Update is called once per frame
	void Update () {
        if(player)transform.position = player.transform.position;
        if(football)transform.position = football.transform.position;

    }
    public void ResetPlayer()
    {//todo, this whole thing is gross and needs to be rethought
        player = null;
        football = null;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    internal void FollowBall(FootBall ball)
    {
        player = null;
        football = ball.gameObject;
    }
}
