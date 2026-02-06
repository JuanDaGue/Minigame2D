using System;
using UnityEngine;

public class MirrorSubscriptionManager : IMirrorSubscriptionManager
{
    private readonly MirrorCollection _mirrorCollection;
    private readonly LaserLinePoints _laserLinePoints;
    private readonly CamerasManager _camerasManager;
    
    public event Action<MirrorMoveController> OnMirrorTapped;
    public event Action<MirrorMoveController> OnMirrorHit;
    
    public MirrorSubscriptionManager(
        MirrorCollection mirrorCollection,
        LaserLinePoints laserLinePoints,
        CamerasManager camerasManager)
    {
        _mirrorCollection = mirrorCollection;
        _laserLinePoints = laserLinePoints;
        _camerasManager = camerasManager;
    }
    
    public void SubscribeAll()
    {
        foreach (var mirror in _mirrorCollection.Mirrors)
        {
            SubscribeMirror(mirror);
        }
    }
    
    public void UnsubscribeAll()
    {
        foreach (var mirror in _mirrorCollection.Mirrors)
        {
            UnsubscribeMirror(mirror);
        }
    }
    
    public void SubscribeMirror(MirrorMoveController mirror)
    {
        if (mirror == null) return;
        
        // Subscribe to tap events
        mirror.OnMirrorTapped -= HandleMirrorTapped;
        mirror.OnMirrorTapped += HandleMirrorTapped;
        
        // Subscribe to line controller events
        var lineController = mirror.GetComponent<LineController>();
        if (lineController != null)
        {
            lineController.OnMirrorHit -= HandleMirrorHit;
            lineController.OnMirrorHit += HandleMirrorHit;
        }
        
        // Subscribe to laser line points
        _laserLinePoints?.SubscribeLinePoints(mirror.transform);
        
        // Subscribe inputs
        mirror.OnInputsSubscribe();
        
        Debug.Log($"[MirrorSubscriptionManager] Subscribed to mirror: {mirror.name}");
    }
    
    public void UnsubscribeMirror(MirrorMoveController mirror)
    {
        if (mirror == null) return;
        
        // Unsubscribe from tap events
        mirror.OnMirrorTapped -= HandleMirrorTapped;
        
        // Unsubscribe from line controller events
        var lineController = mirror.GetComponent<LineController>();
        if (lineController != null)
        {
            lineController.OnMirrorHit -= HandleMirrorHit;
        }
        
        // Unsubscribe inputs
        mirror.OnInputsUnsubscribe();
        
        Debug.Log($"[MirrorSubscriptionManager] Unsubscribed from mirror: {mirror.name}");
    }
    
    private void HandleMirrorTapped(MirrorMoveController tapped)
    {
        OnMirrorTapped?.Invoke(tapped);
    }
    
    private void HandleMirrorHit(MirrorMoveController controller)
    {
        OnMirrorHit?.Invoke(controller);
    }
}