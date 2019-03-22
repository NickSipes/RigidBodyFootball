using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (!gameManager.isHiked) return;
             
        float speed = 5; // todo make setable variable
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        StrafeMove(h, v, speed);

    }
    public void StrafeMove(float h, float v, float speed)
    {
        anim.SetFloat("VelocityX", h * speed);
        anim.SetFloat("VelocityZ", v * speed);
        rb.velocity = new Vector3(h * speed, 0, v * speed);
       
    }
}
