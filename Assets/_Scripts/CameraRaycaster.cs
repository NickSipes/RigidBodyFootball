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

    public delegate void OnClickPassCatcher(OffPlayer offPlayer);
    public event OnClickPassCatcher OnMouseOverOffPlayer;

    //TODO register Collector for onMouseOverCollector


    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("two ray casters");
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

        // Check if pointer is over an interact able UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Implement UI interaction
        }
        else
        {
            //TODO Consider removing from update loop
            PerformRaycast();
        }
        currentScrenRect = new Rect(0, 0, Screen.width, Screen.height);
    }

    internal void PerformRaycast()
    {
        if (currentScrenRect.Contains(Input.mousePosition))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Specify layer priorities below, order matters
            RayForPassCatcher(ray);
         
         
        }
    }
    //todo get current weapon info and load it into function, use weapon range instead of raycast depth
   
    void RayForPassCatcher(Ray ray)
    {
        Physics.Raycast(ray, out RaycastHit hitInfo, maxRaycastDepth, recieverLayer);
        if (hitInfo.collider == null) return;

        var gameObjectHit = hitInfo.collider.gameObject;
        var offPlayer = gameObjectHit.GetComponent<OffPlayer>();
        if (offPlayer != null)
        {
            // todo create better UI change cursor
            OnMouseOverOffPlayer?.Invoke(offPlayer);

        }
        else
        {
            gameManager.ClearSelector();
        }
       
    }

}