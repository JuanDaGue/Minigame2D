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
        
        if (!_stateController.CanMoveObject && _stateController.CurrentState != MirrorState.Setted)
        {
            Debug.Log($"[{_mirror.name}] Mirror cannot move and is not Setted - ignoring tap");
            return false;
        }
        
        if (_camera == null || _mirrorPoint == null) 
        {
            Debug.LogWarning($"[{_mirror.name}] No camera or mirror point available");
            return false;
        }
        
        try
        {
            // Convert screen to world point
            Vector3 worldPoint = ConvertScreenToWorld(screenPos);
            Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);
            
            Debug.Log($"[{_mirror.name}] Checking tap at world: {worldPoint2D}");
            
            // Try both methods
            bool isOverMirror = CheckOverlapPoint(worldPoint2D);
            
            if (!isOverMirror)
            {
                // Fallback to raycast
                isOverMirror = CheckRaycast(screenPos);
            }
            
            Debug.Log($"[{_mirror.name}] Is tap over this mirror? {isOverMirror}");
            return isOverMirror;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[{_mirror.name}] Error in tap detection: {e.Message}");
            return false;
        }
    }
    
    private Vector3 ConvertScreenToWorld(Vector2 screenPos)
    {
        // Get the distance from camera to mirror in world space
        float zDepth = Mathf.Abs(_camera.transform.position.z);
        
        // If using orthographic camera for 2D
        if (_camera.orthographic)
        {
            Vector3 worldPoint = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 
                _camera.nearClipPlane));
            return new Vector3(worldPoint.x, worldPoint.y, _mirrorPoint.position.z);
        }
        else
        {
            // For perspective camera
            Ray ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
            Plane plane = new Plane(Vector3.forward, _mirrorPoint.position);
            float distance;
            
            if (plane.Raycast(ray, out distance))
            {
                return ray.GetPoint(distance);
            }
            
            return Vector3.zero;
        }
    }
    
    private bool CheckOverlapPoint(Vector2 worldPoint)
    {
        // Use a small radius to account for precision issues
        float checkRadius = 0.1f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPoint, checkRadius);
        
        Debug.Log($"[{_mirror.name}] Found {hits.Length} colliders at point {worldPoint}:");
        
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            
            Debug.Log($"  - {hit.name} (Parent: {hit.transform.parent?.name})");
            
            // Check multiple ways this could be our mirror
            if (IsOurMirror(hit.transform))
            {
                Debug.Log($"[{_mirror.name}] ✓ Hit detected on THIS mirror!");
                return true;
            }
        }
        
        return false;
    }
    
    private bool CheckRaycast(Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, _layerMask);
        
        Debug.Log($"[{_mirror.name}] Raycast found {hits.Length} hits");
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                Debug.Log($"  - Hit: {hit.collider.name}");
                if (IsOurMirror(hit.collider.transform))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private bool IsOurMirror(Transform hitTransform)
    {
        // Check direct match
        if (hitTransform == _mirror.transform || hitTransform == _mirrorPoint)
            return true;
        
        // Check if it's a child of our mirror
        if (hitTransform.IsChildOf(_mirror.transform))
            return true;
        
        // Check if it's the mirrorPoint or its child
        if (_mirrorPoint != null && 
            (hitTransform == _mirrorPoint || hitTransform.IsChildOf(_mirrorPoint)))
            return true;
        
        // Check by name pattern (fallback)
        if (hitTransform.name.Contains(_mirror.name) || 
            _mirror.name.Contains(hitTransform.name))
            return true;
        
        return false;
    }
}