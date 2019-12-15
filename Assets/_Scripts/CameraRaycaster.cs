using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRaycaster : MonoBehaviour
{
    [SerializeField] Vector2 cursorHotspot = new Vector2(0, 0);

    [HideInInspector] public CameraFollow camFollow;
    [HideInInspector] public GameObject player;
    GameManager gameManager; 
    // Layers
    const int receiverLayer = 9;
   
    private Ray ray;
    private GameObject rayHit;
    float maxRaycastDepth = 10000f; //todo make variable Stat
    Rect currentScrenRect = new Rect(0, 0, Screen.width, Screen.height);

    private Camera mainCamera;
    public static CameraRaycaster instance { get; private set; }

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
            PerformRay();
        }
        currentScrenRect = new Rect(0, 0, Screen.width, Screen.height);
    }

    internal void PerformRay()
    {
        if (currentScrenRect.Contains(Input.mousePosition))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Specify layer priorities below, order matters
            RayForPassCatcher(ray);
         
         
        }
    }
    
    void RayForPassCatcher(Ray rayForPassCatcher)
    {
        Physics.Raycast(rayForPassCatcher, out RaycastHit hitInfo, maxRaycastDepth, receiverLayer);
        if (hitInfo.collider == null){Debug.Log("ColliderNull"); return;}
        //Debug.Log("hit " + hitInfo.transform.name);
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

    protected virtual void OnOnMouseDownfield(Vector3 destination)
    {
        onMouseDownfield?.Invoke(destination);
    }
}