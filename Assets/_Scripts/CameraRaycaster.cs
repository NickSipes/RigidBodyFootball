using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRaycaster : MonoBehaviour
{

    
    [SerializeField] Vector2 cursorHotspot = new Vector2(0, 0);

    [HideInInspector] public CameraFollow camFollow;
    [HideInInspector] public GameObject player;
    GameManager gameManager; 
    // Layers
    const int recieverLayer = 9;
   
    private Ray ray;
    private GameObject rayHit;
    float maxRaycastDepth = 100f; //todo make variable Stat
    Rect currentScrenRect = new Rect(0, 0, Screen.width, Screen.height);

    private Camera mainCamera;
    [HideInInspector] public Vector3 AimPoint;
    [HideInInspector] public GameObject AimObject;

    public static CameraRaycaster instance { get; private set; }




    //todo remove this variable pull in current player weapon config


    public delegate void OnMouseDownField(Vector3 destination);
    public event OnMouseDownField onMouseDownfield;

    public delegate void OnMouseOverWr(WR wr);
    public event OnMouseOverWr onMouseOverWr;

    //TODO register Collector for onMouseOverCollector


    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("two raycasters");
        }
        instance = this;

    }

    void Start()
    {
        //todo will break when switching players
        player = GameObject.FindGameObjectWithTag("Player");
        UserControl playerControl = player.GetComponent<UserControl>();
        camFollow = CameraFollow.instance;
        gameManager = GameManager.instance;
        mainCamera = Camera.main;

    }

    void Update()
    {

        // Check if pointer is over an interactable UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Impliment UI interaction
        }
        else
        {
            //TODO Consider removing from update loop
            PerformRaycasts();
        }
        currentScrenRect = new Rect(0, 0, Screen.width, Screen.height);
    }

    void PerformRaycasts()
    {
        if (currentScrenRect.Contains(Input.mousePosition))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Specify layer priorities below, order matters
            RaycastForWr(ray);
         
         
        }
    }
    //todo get current weapon info and load it into function, use weapon range instead of raycast depth
   
    void RaycastForWr(Ray ray)
    {
        RaycastHit hitInfo;
        Physics.Raycast(ray, out hitInfo, maxRaycastDepth, recieverLayer);
        if (hitInfo.collider == null) return;

        GameObject gameObjectHit = hitInfo.collider.gameObject;
        var wrHit = gameObjectHit.GetComponent<WR>();
        if (wrHit != null)
        {
            // todo create better UI change cursor
            onMouseOverWr?.Invoke(wrHit);

        }
        else
        {
            gameManager.ClearSelector();
        }
       
    }

}