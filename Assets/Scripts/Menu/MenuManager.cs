using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CameraToggle cameraToggle;
    public GameObject EmiterLight;
    public CanvasGroup menuCanvasGroup;
    private bool canvasActive = true;
    private bool lightOn = false;
    private LigthsController ligthsController;

    public MirrorManager mirrorManager;
    void Start()
    {
        if (EmiterLight != null)
        {
            ligthsController = EmiterLight.GetComponent<LigthsController>();
        }
        if (ligthsController != null)
        {
            ligthsController.isFireLightOn= lightOn;
        }
        if(mirrorManager != null)
        {
            mirrorManager.DeactiveAllMirrors();
        }
    }


    public void StartGame()
    {
        if (cameraToggle != null)
        {
            cameraToggle.SetCameraPriority();
        }
        // if (ligthsController != null)
        // {
        //     ligthsController.isFireLightOn= true;
        // }
        // if (EmiterLight != null)
        // {
        //     SpriteRenderer sr = EmiterLight.GetComponent<SpriteRenderer>();
        //     if (sr != null)
        //     {
        //         sr.enabled = true;
        //     }
        // }
        mirrorManager.SetActiveMirror(EmiterLight);
        toggleCanvas();
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
