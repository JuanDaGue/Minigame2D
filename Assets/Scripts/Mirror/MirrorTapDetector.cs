using UnityEngine;

public class MirrorTapDetector : IMirrorTapDetector
{
    private readonly MirrorMoveController _mirror;
    private readonly Camera _camera;
    private readonly Transform _mirrorPoint;
    private readonly IMirrorStateController _stateController;
    private readonly LayerMask _layerMask;
    
    public MirrorTapDetector(MirrorMoveController mirror, Camera camera, 
                           Transform mirrorPoint, IMirrorStateController stateController,
                           LayerMask layerMask)
    {
        _mirror = mirror;
        _camera = camera;
        _mirrorPoint = mirrorPoint;
        _stateController = stateController;
        _layerMask = layerMask;
    }
    
    public bool IsTapOverMirror(Vector2 screenPos)
    {
        if (_stateController.CurrentState == MirrorState.Deactive)
        {
            Debug.Log($"[{_mirror.name}] Mirror is Deactive - ignoring tap detection");
            return false;
        }
        
        if (_camera == null || _mirrorPoint == null) 
        {
            Debug.LogWarning($"[{_mirror.name}] No camera or mirror point available");
            return false;
        }
        
        try
        {
            float zDepth = Mathf.Abs(_camera.transform.position.z - _mirrorPoint.position.z);
            if (Mathf.Approximately(zDepth, 0)) zDepth = 10f;
            
            Vector3 worldPoint3D = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDepth));
            Vector2 worldPoint2D = new Vector2(worldPoint3D.x, worldPoint3D.y);
            
            Debug.Log($"[{_mirror.name}] Screen {screenPos} -> World {worldPoint2D}, zDepth: {zDepth}");
            
            // Method 1: OverlapPoint
            return CheckOverlapPoint(worldPoint2D);
            
            // Method 2: Raycast (uncomment to use)
            // return CheckRaycast(screenPos);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{_mirror.name}] Error in tap detection: {e.Message}");
            return false;
        }
    }
    
    private bool CheckOverlapPoint(Vector2 worldPoint)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);
        
        if (hits.Length > 0)
        {
            Debug.Log($"[{_mirror.name}] Found {hits.Length} colliders:");
            foreach (Collider2D hit in hits)
            {
                Debug.Log($"  - {hit.name}");
                if (IsOurMirror(hit.transform))
                {
                    Debug.Log($"[{_mirror.name}] ✓ Hit detected on THIS mirror!");
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private bool CheckRaycast(Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, _layerMask);
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && IsOurMirror(hit.collider.transform))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private bool IsOurMirror(Transform hitTransform)
    {
        return hitTransform == _mirror.transform || 
               hitTransform == _mirrorPoint || 
               hitTransform.IsChildOf(_mirror.transform) || 
               (_mirrorPoint != null && hitTransform.IsChildOf(_mirrorPoint));
    }
}