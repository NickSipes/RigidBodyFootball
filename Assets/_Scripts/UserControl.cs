﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserControl : MonoBehaviour
{
    GameManager gameManager;
    Animator anim;
    Rigidbody rb;
    FootBallAthlete controlPlayer;
    // Start is called before the first frame update

    //todo this should be a character script, used to transistion between rb and navmesh movement
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();
        controlPlayer = GetComponent<FootBallAthlete>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (!gameManager.isHiked) return;
        if (controlPlayer.userControl == false) return;
             
        float speed = 5; // todo make setable variable
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        StrafeMove(h, v, speed);

    }
    public void StrafeMove(float h, float v, float speed)
    {
        if (controlPlayer.userControl == false) return;
        anim.SetFloat("VelocityX", h * speed);
        anim.SetFloat("VelocityZ", v * speed);
        rb.velocity = new Vector3(h * speed, 0, v * speed);
       
    }
}
