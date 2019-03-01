using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oline : FootBallAthlete
{
    void Start()
    {
        FindComponents();
    }

    private void FindComponents()
    {
        anim = GetComponent<Animator>();
        oLine = FindObjectsOfType<Oline>();
        dLine = FindObjectsOfType<Dline>();
       //gameManager.hikeTheBall += HikeTheBall;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void HikeTheBall(bool wasHiked)
    {
        Debug.Log("Oline Hike");
        anim.SetTrigger("HikeTrigger");
    }
}
