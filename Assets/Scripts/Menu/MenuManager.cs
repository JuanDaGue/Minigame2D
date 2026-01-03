using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CameraToggle cameraToggle;
    public GameObject EmiterLight;
    public CanvasGroup menuCanvasGroup;
    private bool canvasActive = true;
    //private bool lightOn = false;
    private LigthsController ligthsController;
    //private MirrorManager2 mirrorManager;
     

    void Start()
    {
        if (EmiterLight != null)
        {
            ligthsController = EmiterLight.GetComponent<LigthsController>();
        }
        //GameManager.Instance.SetState(GameManager.States.Pause);
    }


    public void StartGame()
    {
        if (cameraToggle != null)
        {
            cameraToggle.SetCameraPriority();
        }
        GameManager.Instance.SetState(GameManager.States.InGame);

       Debug.Log("[MenuManager] Game Started");
    }
    public void ExitGame()
    {
        Application.Quit();
    } 

    void toggleCanvas()
    {
        canvasActive = !canvasActive;
        if (canvasActive)
        {
            menuCanvasGroup.alpha = 1;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            menuCanvasGroup.alpha = 0;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }
    }  
    #if UNITY_EDITOR
    void OnApplicationQuit()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }
    #endif
}
