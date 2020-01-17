using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class Oline : OffPlayer
{
    internal override void Start()
    {
        base.Start();
        rayColor = Color.magenta;

    }

    // Update is called once per frame
    internal override void Update()
    {
        base.Update();
        if (!gameManager.isHiked) return;

        BlockProtection();

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

    }

    public void HikeTheBall(bool wasHiked)
    {
        //Debug.Log("Oline Hike");
        anim.SetTrigger("HikeTrigger");
    }

    internal override void BlockProtection()
    {
        //get side of field... position
        base.BlockProtection();
      
        //get a list of rushers

        //get job from route?
    }


}
