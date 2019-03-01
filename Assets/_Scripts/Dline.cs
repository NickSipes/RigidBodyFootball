using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dline : FootBallAthlete
{
    // Start is called before the first frame update
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
        anim.SetTrigger("HikeTrigger");
    }
}
