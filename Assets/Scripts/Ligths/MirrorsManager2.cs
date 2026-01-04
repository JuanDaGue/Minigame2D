using System.Collections.Generic;
using UnityEngine;

public class MirrorManager2 : MonoBehaviour
{
    [Tooltip("List of MirrorMoveController components for each mirror")]
    public List<MirrorMoveController> mirrorControllers = new List<MirrorMoveController>();

    [SerializeField] private LaserLinePoints laserLinePoints;
    [SerializeField] private MirrorMoveController currentMirror;
    [SerializeField] private LineController currentLineController;
    [SerializeField] private GlobalTime globalTime;
    [SerializeField] private float LigthReductionAmount;
    [SerializeField] private CamerasManager camerasManager;

    private bool isInitialized = false;

    private void Awake()
    {
        // Optional: keep minimal editor-time checks but do not run full game initialization here.
        if (mirrorControllers.Count > 0)
        {
            Debug.Log($"[MirrorManager2] {mirrorControllers.Count} mirrors assigned in inspector.");
        }
        if( camerasManager == null)
        {
            camerasManager = FindFirstObjectByType<CamerasManager>();
        }   
    }

    /// <summary>
    /// Call this when the game actually starts (e.g., from MenuManager.StartGame).
    /// Safe to call multiple times; it will only initialize once.
    /// </summary>
    public void InitializeForGame()
    {
        if (isInitialized) return;
        isInitialized = true;

        // If currentMirror wasn't assigned, try to pick the first from the list
        if (currentMirror == null && mirrorControllers.Count > 0)
        {
            currentMirror = mirrorControllers[0];
        }

        // Ensure currentMirror is in the list (but don't add duplicates)
        if (currentMirror != null && !mirrorControllers.Contains(currentMirror))
        {
            mirrorControllers.Add(currentMirror);
        }

        // Setup current line controller
        if (currentMirror != null)
        {
            currentLineController = currentMirror.GetComponent<LineController>();
            currentMirror.SwitchMirrorState(MirrorState.Active);
            laserLinePoints?.SubscribeLinePoints(currentMirror.transform);
            Debug.Log($"[MirrorManager2] Initial active mirror: {currentMirror.name}");
        }

        // Subscribe to global time properly
        if (globalTime == null)
        {
            globalTime = FindFirstObjectByType<GlobalTime>();
        }
        if (globalTime != null)
        {
            globalTime.OnIntervalReached += LigthReductionEvent;
        }

        SubscribeMirrorEvents();
        if (mirrorControllers.Count > 0) SetMirrorPointLines();

        if (currentLineController != null) SubscribeMirror(currentLineController);
        if (currentMirror != null) currentMirror.OnInputsSusbcribe();
    }

    private void LigthReductionEvent()
    {
        foreach (MirrorMoveController mirror in mirrorControllers)
        {
            if (mirror == null) continue;
            LigthsController mirrorLigths = mirror.GetComponent<LigthsController>();
            if (mirrorLigths != null)
            {
                mirrorLigths.IntensityController(LigthReductionAmount);
            }
        }
    }

    private void OnDestroy()
    {
        if (globalTime != null) globalTime.OnIntervalReached -= LigthReductionEvent;

        foreach (var mirror in mirrorControllers)
        {
            var mirrorLine = mirror?.GetComponent<LineController>();
            UnsubscribeMirror(mirrorLine);
        }
    }

    private void SubscribeMirrorEvents()
    {
        foreach (var mirror in mirrorControllers)
        {
            var mirrorLine = mirror?.GetComponent<LineController>();
            SubscribeMirror(mirrorLine);
        }
    }

    private void SubscribeMirror(LineController mirrorLine)
    {
        if (mirrorLine != null)
        {
            mirrorLine.OnMirrorHit += HandleMirrorHit;
            Debug.Log($"[MirrorManager2] Subscribed to mirror line: {mirrorLine.name}");
        }
    }

    private void UnsubscribeMirror(LineController mirrorLine)
    {
        if (mirrorLine != null)
        {
            mirrorLine.OnMirrorHit -= HandleMirrorHit;
            Debug.Log($"[MirrorManager2] Unsubscribed from mirror line: {mirrorLine.name}");
        }
    }

    private void HandleMirrorHit(MirrorMoveController controller)
    {
        if (controller == null || mirrorControllers.Contains(controller)) return;

        if (currentMirror != null)
        {
            currentMirror.SwitchMirrorState(MirrorState.Setted);
            currentMirror.OnInputsUnsubsbcribe();
            mirrorControllers.Add(controller);
        }

        laserLinePoints?.SubscribeLinePoints(controller.transform);

        currentMirror = controller;
        currentMirror.SwitchMirrorState(MirrorState.Active);
        camerasManager.MoveCameraToTarget(currentMirror.transform, 1.5f, new Vector3(0, 0, 0));

        UnsubscribeMirror(currentLineController);
        currentLineController = controller.GetComponent<LineController>();
        SubscribeMirror(currentLineController);
        currentMirror.OnInputsSusbcribe();
        currentMirror.gameObject.layer = LayerMask.NameToLayer("Emitter");

        Debug.Log($"[MirrorManager2] Mirror hit registered: {controller.name}");
    }

    private void SetMirrorPointLines()
    {
        foreach (var mirror in mirrorControllers)
        {
            if (mirror != null)
            {
                laserLinePoints?.SubscribeLinePoints(mirror.transform);
            }
        }
    }
}