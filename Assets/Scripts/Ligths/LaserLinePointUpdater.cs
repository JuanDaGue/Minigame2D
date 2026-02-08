using System.Collections.Generic;
using UnityEngine;
public class LaserLinePointUpdater : ILinePointUpdater
{
    private readonly LaserLinePoints _laserLinePoints;
    
    public LaserLinePointUpdater(LaserLinePoints laserLinePoints)
    {
        _laserLinePoints = laserLinePoints;
    }
    
    public void UpdateLinePoints()
    {
        if (_laserLinePoints == null) return;
        
        _laserLinePoints.UpdateLineRenderer();
    }
    
    public void SubscribeMirrorToLinePoints(Transform mirrorTransform)
    {
        if (_laserLinePoints == null || mirrorTransform == null) return;
        
        _laserLinePoints.SubscribeLinePoints(mirrorTransform);
    }
    
    public void UnsubscribeMirrorFromLinePoints(Transform mirrorTransform)
    {
        if (_laserLinePoints == null || mirrorTransform == null) return;
        
        _laserLinePoints.UnsubscribeLinePoints(mirrorTransform);
    }
    
    // New method to rebuild from a list
    public void RebuildLinePoints(List<Transform> mirrorTransforms)
    {
        if (_laserLinePoints == null) return;
        
        _laserLinePoints.RebuildLinePoints(mirrorTransforms);
    }
    
    // New method to remove points after index
    public void RemovePointsAfterIndex(int index)
    {
        if (_laserLinePoints == null) return;
        
        _laserLinePoints.RemovePointsAfterIndex(index);
    }
}