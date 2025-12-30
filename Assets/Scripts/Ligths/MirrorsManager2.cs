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

    private void Awake()
    {
        if (mirrorControllers.Count > 0)
        {
            currentMirror = mirrorControllers[0];
            currentLineController = currentMirror.GetComponent<LineController>();
        }
        if (currentMirror != null)
        {
            currentMirror.SwitchMirrorState(MirrorState.Active);
            laserLinePoints.SubscribeLinePoints(currentMirror.transform);
        } 
        if (currentLineController != null) SubscribeMirror(currentLineController);
    }

    private void Start()
    {
        SubscribeMirrorEvents();
        if(mirrorControllers.Count > 0) SetMirrorPointLines();

        if (laserLinePoints != null && mirrorControllers.Count > 0)
        {
            Debug.Log($"[MirrorManager2] Drawing laser ray with {mirrorControllers.Count} points.");
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
            currentMirror.transform.LookAt(controller.transform);
            currentMirror.SwitchMirrorState(MirrorState.Setted);
            //mirrorControllers.Add(currentMirror);
        }

        laserLinePoints.SubscribeLinePoints(controller.transform);

        currentMirror = controller;
        currentMirror.SwitchMirrorState(MirrorState.Active);

        UnsubscribeMirror(currentLineController);
        currentLineController = controller.GetComponent<LineController>();
        SubscribeMirror(currentLineController);

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