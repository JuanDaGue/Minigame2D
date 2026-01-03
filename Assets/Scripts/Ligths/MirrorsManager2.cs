using System;
using System.Collections.Generic;
using UnityEngine;

public class MirrorManager2 : MonoBehaviour
{
   // [Tooltip("List of mirror GameObjects that contain LightsController components")]
    //public List<GameObject> mirrors = new List<GameObject>();

    [Tooltip("List of MirrorMoveController components for each mirror")]
    public List<MirrorMoveController> mirrorControllers = new List<MirrorMoveController>();

    [SerializeField] private LaserLinePoints laserLinePoints;
    [SerializeField] private MirrorMoveController currentMirror;
    [SerializeField] private LineController currentLineController;
    [SerializeField] private GlobalTime globalTime;
    [SerializeField] private float LigthReductionAmount;

    private void Awake()
    {
        if (mirrorControllers.Count > 0)
        {
            Debug.Log($"[MirrorManager2] Found {mirrorControllers.Count} mirrors assigned.");
            currentMirror = mirrorControllers[0];
            currentLineController = currentMirror.GetComponent<LineController>();  
        }
        if (currentMirror != null)
        {
            currentMirror.SwitchMirrorState(MirrorState.Active);
            laserLinePoints.SubscribeLinePoints(currentMirror.transform);
            mirrorControllers.Add(currentMirror);
            Debug.Log($"[MirrorManager2] Initial active mirror: {currentMirror.name}");
        } 
        if (currentLineController != null) SubscribeMirror(currentLineController);
    }

    private void Start()
    {
        if(globalTime == null)
        {
            globalTime = FindFirstObjectByType<GlobalTime>();
        }
        else
        {
            globalTime.OnIntervalReached += LigthReductionEvent;
        }

        SubscribeMirrorEvents();
        if(mirrorControllers.Count > 0) SetMirrorPointLines();

        if (laserLinePoints != null && mirrorControllers.Count > 0)
        {
            Debug.Log($"[MirrorManager2] Drawing laser ray with {mirrorControllers.Count} points.");
        }
        if(currentMirror != null)
        {
            //Debug.Log($"[MirrorManager2] Current active mirror: {currentMirror.name}");
            currentMirror.OnInputsSusbcribe();
        }
    }

    private void LigthReductionEvent()
    {
        foreach(MirrorMoveController mirror in mirrorControllers)
        {
            if(mirror != null)
            {
                LigthsController mirrorLigths = mirror.GetComponent<LigthsController>();
                if (mirrorLigths != null)
                {
                    mirrorLigths.IntensityController(LigthReductionAmount);
                }
            }
        }
        
    }

    private void OnDestroy()
    {
        // Ensure we unsubscribe from all events to avoid leaks
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
            // Rotate instantly toward the new mirror (could tween here if desired)
            //currentMirror.transform.LookAt(controller.transform);
            currentMirror.SwitchMirrorState(MirrorState.Setted);
            currentMirror.OnInputsUnsubsbcribe();
            mirrorControllers.Add(controller);
        }

        laserLinePoints.SubscribeLinePoints(controller.transform);

        currentMirror = controller;
        currentMirror.SwitchMirrorState(MirrorState.Active);

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
                laserLinePoints.SubscribeLinePoints(mirror.transform);
            }
        }
    }
}