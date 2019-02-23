using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class WR : MonoBehaviour
{
    [SerializeField]private Renderer materialRenderer;
    private QB qb;
    private Color startColor;
    public NavMeshAgent navMeshAgent;
    private float navStartSpeed;
    private float navStartAccel;
    [SerializeField] private Color highlightColor;
    public Color LineColor;
    private AICharacterControl aiCharacter;
    private Rigidbody rb;
    [SerializeField]LineRenderer lr;
    public Transform startGoal;
    private Vector3 passTarget;
    private bool beenPressed = false;

    public Canvas canvas;
    public Image pressBar;

    public Transform target;
    // Use this for initialization
    void Start()
    {
        qb = FindObjectOfType<QB>();
        rb = GetComponent<Rigidbody>();
        startColor = materialRenderer.material.color;
        aiCharacter = GetComponent<AICharacterControl>();
        startGoal = aiCharacter.target;
        target = startGoal;
        lr.material.color = LineColor;
        navStartSpeed = navMeshAgent.speed;
        navStartAccel = navMeshAgent.acceleration;
    }

    // Update is called once per frame
    void Update()
    {
        if (materialRenderer.material.color != startColor)
        {
            if (Input.GetMouseButtonDown(0)) //Bullet Pass
            {
                Debug.Log("Pressed secondary button.");
                passTarget = transform.position;
                qb.Throw(passTarget, this, 1.5f, 23f);

            }

            if (Input.GetMouseButtonDown(1)) // Touch Pass
            {
                Debug.Log("Pressed secondary button.");
                passTarget = transform.position;
                qb.Throw(passTarget, this, 2.3f, 20f);
             }

            if (Input.GetMouseButtonDown(2)) // Lob PASS
            {
                Debug.Log("Pressed middle click.");
                passTarget = transform.position;
                qb.Throw(passTarget, this, 3.2f, 19.5f);
            }
        }
        canvas.transform.LookAt(Camera.main.transform);

        if (aiCharacter.target == null)
        {
            aiCharacter.target = startGoal;
        }

        if (navMeshAgent.hasPath && navMeshAgent.remainingDistance < 2 && target != null)
        {
            var ball = target.root.GetComponent<FootBall>();

            if (ball != null)
            {
                Destroy(ball);
                aiCharacter.target = startGoal;
            }
              

        }
         
        DrawPath();
        
    }

    void OnMouseEnter() 
    {
        materialRenderer.material.color = highlightColor;
    }

    void OnMouseDown()
    {
      

    }

    void OnMouseExit()
    {
        materialRenderer.material.color = startColor;
    }

    public void PrintPosition()
    {
        Debug.Log("WR position " + transform.position + "" +
                  "Nav Velocity " + navMeshAgent.velocity + "" +
                  "RB Velocity " + rb.velocity);
    }

    public void SetTarget(Transform targetSetter)
    {
        aiCharacter.target = targetSetter;
        target = targetSetter;
    }

    void DrawPath()
    {
        
        Vector3[] path = navMeshAgent.path.corners;
        if (path != null && path.Length > 1)
        {
            lr.positionCount = path.Length;
            for (int i = 0; i < path.Length; i++)
            {
                lr.SetPosition(i, path[i]);
            }
        }
    }

    public bool CanBePressed()
    {
        if (!beenPressed)
        {
            beenPressed = true;
            return true;
        }

        return false;
    }

    public void Press(float pressTimeNorm)
    {
        pressBar.fillAmount = pressTimeNorm;
        navMeshAgent.acceleration = 0f;
        navMeshAgent.speed = 0f;
    }

    public void ReleasePress()
    {
        canvas.enabled = !canvas.enabled;
        navMeshAgent.speed = navStartSpeed;
        navMeshAgent.acceleration = navStartAccel;
    }
    
}
