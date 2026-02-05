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
        if (mirrorControllers.Count > 0)
            Debug.Log($"[MirrorManager2] {mirrorControllers.Count} mirrors assigned in inspector.");

        if (camerasManager == null)
            camerasManager = FindFirstObjectByType<CamerasManager>();
    }

    /// <summary>
    /// Call this when the game actually starts. Safe to call multiple times.
    /// </summary>
    public void InitializeForGame()
    {
        if (isInitialized) return;
        isInitialized = true;

        if (currentMirror == null && mirrorControllers.Count > 0)
            currentMirror = mirrorControllers[0];

        if (currentMirror != null && !mirrorControllers.Contains(currentMirror))
            mirrorControllers.Add(currentMirror);

        if (currentMirror != null)
        {
            currentLineController = currentMirror.GetComponent<LineController>();
            currentMirror.SwitchMirrorState(MirrorState.Active);
            laserLinePoints?.SubscribeLinePoints(currentMirror.transform);
            Debug.Log($"[MirrorManager2] Initial active mirror: {currentMirror.name}");
        }

        if (globalTime == null)
            globalTime = FindFirstObjectByType<GlobalTime>();
        if (globalTime != null)
            globalTime.OnIntervalReached += LigthReductionEvent;

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
                mirrorLigths.IntensityController(LigthReductionAmount);
        }
    }

    private void OnDestroy()
    {
        if (globalTime != null) globalTime.OnIntervalReached -= LigthReductionEvent;
        UnsubscribeMirrorEvents();

        // Unsubscribe any remaining LineController handlers
        foreach (var mirror in mirrorControllers)
        {
            var mirrorLine = mirror?.GetComponent<LineController>();
            UnsubscribeMirror(mirrorLine);
        }
    }

    private void SubscribeMirrorEvents()
    {
        // Subscribe to each mirror's tap event and its LineController
        foreach (var mirror in mirrorControllers)
        {
            if (mirror == null) continue;

            // Ensure we don't double-subscribe
            mirror.OnMirrorTapped -= HandleMirrorTapped;
            mirror.OnMirrorTapped += HandleMirrorTapped;
            Debug.Log($"[MirrorManager2] Subscribed to mirror tap: {mirror.name}");
            var mirrorLine = mirror.GetComponent<LineController>();
            SubscribeMirror(mirrorLine);
        }
    }

    private void UnsubscribeMirrorEvents()
    {
        foreach (var mirror in mirrorControllers)
        {
            if (mirror == null) continue;
            mirror.OnMirrorTapped -= HandleMirrorTapped;

            var mirrorLine = mirror.GetComponent<LineController>();
            UnsubscribeMirror(mirrorLine);
        }
    }

    private void SubscribeMirror(LineController mirrorLine)
    {
        if (mirrorLine == null) return;
        mirrorLine.OnMirrorHit -= HandleMirrorHit;
        mirrorLine.OnMirrorHit += HandleMirrorHit;
        Debug.Log($"[MirrorManager2] Subscribed to mirror line: {mirrorLine.name}");
    }

    private void UnsubscribeMirror(LineController mirrorLine)
    {
        if (mirrorLine == null) return;
        mirrorLine.OnMirrorHit -= HandleMirrorHit;
        //Debug.Log($"[MirrorManager2] Unsubscribed from mirror line: {mirrorLine.name}");
    }

    private void HandleMirrorHit(MirrorMoveController controller)
    {
        if (controller == null) return;

        // If controller already in list, ignore (prevents duplicates)
        if (mirrorControllers.Contains(controller)) return;

        if (currentMirror != null)
        {
            currentMirror.SwitchMirrorState(MirrorState.Setted);
            currentMirror.OnInputsUnsubsbcribe();
            mirrorControllers.Add(controller);
            SubscribeMirrorEvents();
        }

        laserLinePoints?.SubscribeLinePoints(controller.transform);

        currentMirror = controller;
        currentMirror.SwitchMirrorState(MirrorState.Active);
        camerasManager?.MoveCameraToTarget(currentMirror.transform, 1.5f, Vector3.zero);

        UnsubscribeMirror(currentLineController);
        currentLineController = controller.GetComponent<LineController>();
        SubscribeMirror(currentLineController);
        currentMirror.OnInputsSusbcribe();
        currentMirror.gameObject.layer = LayerMask.NameToLayer("Emitter");

       // Debug.Log($"[MirrorManager2] Mirror hit registered: {controller.name}");
    }

    private void SetMirrorPointLines()
    {
        foreach (var mirror in mirrorControllers)
        {
            if (mirror == null) continue;
            laserLinePoints?.SubscribeLinePoints(mirror.transform);
        }
    }

    private void HandleMirrorTapped(MirrorMoveController tapped)
    {
        Debug.Log($"[MirrorManager2] HandleMirrorTapped {tapped==null}");
        if (tapped == null) return;

        int idx = mirrorControllers.IndexOf(tapped);
        Debug.Log($"[MirrorManager2] Tapped mirror index: {idx}");
        if (idx < 0)
        {
            Debug.LogWarning($"[MirrorManager2] Tapped mirror not found in list: {tapped.name}");
            return;
        }

        RemoveMirrorsAfterIndex(idx);

        // Make tapped the current mirror
        currentMirror = tapped;
        currentLineController = currentMirror?.GetComponent<LineController>();
        if (currentLineController != null)
            SubscribeMirror(currentLineController);

        camerasManager?.MoveCameraToTarget(currentMirror.transform, 1.5f, Vector3.zero);
    }

    // Removes mirrors after the given index (exclusive). Cleans subscriptions and optionally destroys or deactivates.
    private void RemoveMirrorsAfterIndex(int index)
    {
        for (int i = mirrorControllers.Count - 1; i > index; i--)
        {
            Debug.Log($"[MirrorManager2] Removing mirror at index {i}");
            MirrorMoveController m = mirrorControllers[i];
            if (m == null)
            {
                mirrorControllers.RemoveAt(i);
                continue;
            }

            // 1) Change state
            m.SwitchMirrorState(MirrorState.Deactive);

            // 2) Unsubscribe inputs and tap event
            m.OnInputsUnsubsbcribe();
            m.OnMirrorTapped -= HandleMirrorTapped;

            // 3) Unsubscribe LineController events
            var line = m.GetComponent<LineController>();
            if (line != null)
                UnsubscribeMirror(line);

            // 4) Try to call laserLinePoints.UnsubscribeLinePoints if method exists
            if (laserLinePoints != null)
            {
                var mi = laserLinePoints.GetType().GetMethod("UnsubscribeLinePoints");
                if (mi != null)
                {
                    try
                    {
                        mi.Invoke(laserLinePoints, new object[] { m.transform });
                    }
                    catch
                    {
                        // ignore reflection errors
                    }
                }
            }

            // 5) Remove from list
            mirrorControllers.RemoveAt(i);

            // 6) Optional: destroy or deactivate. Keep commented so you choose behavior.
            // Destroy(m.gameObject);
            m.gameObject.SetActive(false);

            Debug.Log($"[MirrorManager2] Removed mirror {m.name} at index {i}");
        }
    }
}