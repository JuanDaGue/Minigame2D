using System.Collections;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public CamerasManager cameraToggle;
    public GameObject EmiterLight;
    public CanvasGroup menuCanvasGroup;
   
    private LigthsController ligthsController;
    [SerializeField] private MirrorManager2 mirrorManager;
     

    void Start()
    {
        if (EmiterLight != null)
        {
            ligthsController = EmiterLight.GetComponent<LigthsController>();
        }
    }


    public void StartGame()
    {
        if (cameraToggle != null)
        {
            cameraToggle.SetCameraPriority();
        }

        GameManager.Instance.SetState(GameManager.States.InGame);

        // Initialize mirrors now that the game is InGame
        if (mirrorManager == null)
        {
            mirrorManager = FindFirstObjectByType<MirrorManager2>();
        }
        
        StartCoroutine(StartGameWithDelay(0.2f));
        Debug.Log("[MenuManager] Game Started");
    }


    public void ExitGame()
    {
        Application.Quit();
    } 

    IEnumerator StartGameWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Initialize the mirror manager
        if (mirrorManager != null)
        {
            mirrorManager.Initialize();
            Debug.Log("[MenuManager] MirrorManager initialized");
        }
        else
        {
            Debug.LogError("[MenuManager] MirrorManager2 not found!");
        }
    }
    
    #if UNITY_EDITOR
    void OnApplicationQuit()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }
    #endif
}