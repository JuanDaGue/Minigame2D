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
        //Debug.Log($"[MirrorSubscriptionManager] Subscribing ALL {_mirrorCollection.Mirrors.Count} mirrors");
        
        foreach (var mirror in _mirrorCollection.Mirrors)
        {
            if (mirror != null)
            {
                SubscribeMirror(mirror);
            }
        }
    }
    
    public void UnsubscribeAll()
    {
        foreach (var mirror in _mirrorCollection.Mirrors)
        {
            if (mirror != null)
            {
                UnsubscribeMirror(mirror);
            }
        }
    }
    
    public void SubscribeMirror(MirrorMoveController mirror)
    {
        if (mirror == null) return;
        
        //Debug.Log($"[MirrorSubscriptionManager] Subscribing mirror: {mirror.name}");
        
        // Unsubscribe first to avoid duplicates
        mirror.OnMirrorTapped -= HandleMirrorTapped;
        mirror.OnMirrorTapped += HandleMirrorTapped;
        
        // Subscribe to line controller events
        var lineController = mirror.GetComponent<LineController>();
        if (lineController != null)
        {
            lineController.OnMirrorHit -= HandleMirrorHit;
            lineController.OnMirrorHit += HandleMirrorHit;
        }
        
        // Subscribe inputs ALWAYS (for all mirrors)
        mirror.OnInputsSubscribe();
        
        // Subscribe to laser line points
        _laserLinePoints?.SubscribeLinePoints(mirror.transform);
        
        //Debug.Log($"[MirrorSubscriptionManager] ✓ Subscribed to mirror: {mirror.name}");
    }
    
    public void UnsubscribeMirror(MirrorMoveController mirror)
    {
        if (mirror == null) return;
        
        //Debug.Log($"[MirrorSubscriptionManager] Unsubscribing mirror: {mirror.name}");
        
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
        
        //Debug.Log($"[MirrorSubscriptionManager] ✓ Unsubscribed from mirror: {mirror.name}");
    }
    
    private void HandleMirrorTapped(MirrorMoveController tapped)
    {
        //Debug.Log($"[MirrorSubscriptionManager] Mirror tapped event: {tapped?.name}");
        OnMirrorTapped?.Invoke(tapped);
    }
    
    private void HandleMirrorHit(MirrorMoveController controller)
    {
        //Debug.Log($"[MirrorSubscriptionManager] Mirror hit event: {controller?.name}");
        OnMirrorHit?.Invoke(controller);
    }
    
    // Resubscribe all mirrors (for after cleanup)
    public void ResubscribeAll()
    {
        //Debug.Log($"[MirrorSubscriptionManager] Resubscribing ALL mirrors");
        
        // First unsubscribe all
        UnsubscribeAll();
        
        // Then subscribe all
        SubscribeAll();
    }
}