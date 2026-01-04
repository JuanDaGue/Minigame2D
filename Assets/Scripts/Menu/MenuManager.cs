using System.Collections;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CamerasManager cameraToggle;
    public GameObject EmiterLight;
    public CanvasGroup menuCanvasGroup;
   
    //private bool lightOn = false;
    private LigthsController ligthsController;
    [SerializeField]private MirrorManager2 mirrorManager;
     

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

        // Initialize mirrors now that the game is InGame
        if (mirrorManager == null)
        {
            mirrorManager = FindFirstObjectByType<MirrorManager2>();
        }
        //mirrorManager?.InitializeForGame();
        StartCoroutine(StarGameWithDelay(0.2f));
        Debug.Log("[MenuManager] Game Started");
        
    }


    public void ExitGame()
    {
        Application.Quit();
    } 

    IEnumerator StarGameWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        mirrorManager?.InitializeForGame();

    }
    #if UNITY_EDITOR
    void OnApplicationQuit()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }
    #endif
}
