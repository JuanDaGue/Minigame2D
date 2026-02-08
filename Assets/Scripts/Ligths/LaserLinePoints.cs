using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LaserLinePoints : MonoBehaviour
{
    [Tooltip("List of mirror points with LightsController components")]
    public List<Transform> linePoints = new List<Transform>();

    [SerializeField] private LineRenderer generalLightLine;
    
    [Header("Line Settings")]
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Gradient lineGradient;
    [SerializeField] private Material lineMaterial;
    
    // Cache for performance
    private Dictionary<int, Transform> _mirrorIdCache = new Dictionary<int, Transform>();

    private void Awake()
    {
        InitializeLineRenderer();
        InitializeCache();
    }
    
    private void InitializeLineRenderer()
    {
        if (generalLightLine == null)
        {
            generalLightLine = GetComponent<LineRenderer>();
            if (generalLightLine == null)
            {
                generalLightLine = gameObject.AddComponent<LineRenderer>();
            }
        }
        
        // Configure line renderer
        if (generalLightLine != null)
        {
            generalLightLine.startWidth = lineWidth;
            generalLightLine.endWidth = lineWidth;
            generalLightLine.colorGradient = lineGradient;
            generalLightLine.material = lineMaterial;
            generalLightLine.positionCount = 0;
        }
    }
    
    private void InitializeCache()
    {
        _mirrorIdCache.Clear();
        foreach (var point in linePoints)
        {
            if (point != null)
            {
                _mirrorIdCache[point.gameObject.GetInstanceID()] = point;
            }
        }
    }

    public void SubscribeLinePoints(Transform point)
    {
        if (point == null) 
        {
            Debug.LogWarning("[LaserLinePoints] Attempted to subscribe null point");
            return;
        }

        int instanceId = point.gameObject.GetInstanceID();
        
        // Check cache for duplicates
        if (_mirrorIdCache.ContainsKey(instanceId))
        {
            Debug.Log($"[LaserLinePoints] Point already subscribed: {point.name}");
            return;
        }

        // Verify it's a mirror
        var mirrorController = point.GetComponent<MirrorMoveController>();
        if (mirrorController == null)
        {
            Debug.LogWarning($"[LaserLinePoints] {point.name} is not a mirror (no MirrorMoveController)");
            return;
        }

        // Add to collections
        linePoints.Add(point);
        _mirrorIdCache[instanceId] = point;
        
        Debug.Log($"[LaserLinePoints] Subscribed point: {point.name} (Total: {linePoints.Count})");
        
        // Optional: Sort points by some criteria (e.g., creation order)
        SortLinePoints();
    }

    public void UnsubscribeLinePoints(Transform point)
    {
        if (point == null) 
        {
            Debug.LogWarning("[LaserLinePoints] Attempted to unsubscribe null point");
            return;
        }

        int instanceId = point.gameObject.GetInstanceID();
        
        // Remove from cache
        if (_mirrorIdCache.ContainsKey(instanceId))
        {
            _mirrorIdCache.Remove(instanceId);
            Debug.Log($"[LaserLinePoints] Removed from cache: {point.name}");
        }
        
        // Remove from list
        if (linePoints.Contains(point))
        {
            linePoints.Remove(point);
            Debug.Log($"[LaserLinePoints] Unsubscribed point: {point.name} (Remaining: {linePoints.Count})");
        }
        else
        {
            Debug.LogWarning($"[LaserLinePoints] Point not found in list: {point.name}");
        }
        
        // Update visual if needed
        if (generalLightLine != null && generalLightLine.positionCount > linePoints.Count)
        {
            UpdateLineRenderer();
        }
    }

    public void ClearAllPoints()
    {
        Debug.Log($"[LaserLinePoints] Clearing all {linePoints.Count} points");
        
        linePoints.Clear();
        _mirrorIdCache.Clear();
        
        if (generalLightLine != null)
        {
            generalLightLine.positionCount = 0;
            // Optionally hide the line renderer
            generalLightLine.enabled = false;
        }
    }

   public void DrawLaserRay(Vector3 firePoint, Vector3 hitPoint)
    {
        if (generalLightLine == null) return;

        var positions = new List<Vector3>();

        foreach (var mirror in linePoints)
        {
            if (mirror != null)
                positions.Add(mirror.position);
        }

        positions.Add(hitPoint);

        generalLightLine.positionCount = positions.Count;
        for (int i = 0; i < positions.Count; i++)
        {
            generalLightLine.SetPosition(i, positions[i]);
        }
        }
        
    // New method: Update line renderer based on current points (without hit point)
    public void UpdateLineRenderer()
    {
        if (generalLightLine == null) return;
        
        var positions = new List<Vector3>();
        
        // Add all mirror points
        foreach (var mirror in linePoints)
        {
            if (mirror != null)
                positions.Add(mirror.position);
        }
        
        // Update line renderer
        if (positions.Count > 0)
        {
            generalLightLine.enabled = true;
            generalLightLine.positionCount = positions.Count;
            for (int i = 0; i < positions.Count; i++)
            {
                generalLightLine.SetPosition(i, positions[i]);
            }
        }
        else
        {
            generalLightLine.enabled = false;
            generalLightLine.positionCount = 0;
        }
        
        Debug.Log($"[LaserLinePoints] Updated line with {positions.Count} points");
    }
    
    // New method: Rebuild line from a list of transforms
    public void RebuildLinePoints(List<Transform> mirrorTransforms)
    {
        Debug.Log($"[LaserLinePoints] Rebuilding line from {mirrorTransforms?.Count} transforms");
        
        ClearAllPoints();
        
        if (mirrorTransforms == null || mirrorTransforms.Count == 0)
            return;
        
        foreach (var transform in mirrorTransforms)
        {
            if (transform != null)
            {
                SubscribeLinePoints(transform);
            }
        }
        
        UpdateLineRenderer();
    }
    
    // New method: Get active mirror transforms
    public List<Transform> GetActiveMirrorTransforms()
    {
        return new List<Transform>(linePoints);
    }
    
    // New method: Check if a transform is already subscribed
    public bool IsSubscribed(Transform point)
    {
        if (point == null) return false;
        return _mirrorIdCache.ContainsKey(point.gameObject.GetInstanceID());
    }
    
    // New method: Remove all points after a specific index
    public void RemovePointsAfterIndex(int index)
    {
        if (index < 0 || index >= linePoints.Count)
        {
            Debug.LogWarning($"[LaserLinePoints] Invalid index for RemovePointsAfterIndex: {index}");
            return;
        }
        
        int removedCount = linePoints.Count - (index + 1);
        for (int i = linePoints.Count - 1; i > index; i--)
        {
            var point = linePoints[i];
            if (point != null)
            {
                int instanceId = point.gameObject.GetInstanceID();
                _mirrorIdCache.Remove(instanceId);
            }
            linePoints.RemoveAt(i);
        }
        
        Debug.Log($"[LaserLinePoints] Removed {removedCount} points after index {index}");
        UpdateLineRenderer();
    }
    
    // New method: Sort points (you can customize sorting logic)
    private void SortLinePoints()
    {
        // Example: Sort by X position (left to right)
        // linePoints = linePoints.OrderBy(p => p?.position.x ?? float.MaxValue).ToList();
        
        // Or sort by some identifier in the name
        // linePoints = linePoints.OrderBy(p => GetMirrorNumber(p?.name)).ToList();
    }
    
    // Helper method to extract mirror number from name
    private int GetMirrorNumber(string mirrorName)
    {
        if (string.IsNullOrEmpty(mirrorName)) return int.MaxValue;
        
        // Extract numbers from string (e.g., "Mirror (1)" -> 1)
        var numbers = System.Text.RegularExpressions.Regex.Matches(mirrorName, @"\d+");
        if (numbers.Count > 0 && int.TryParse(numbers[0].Value, out int result))
        {
            return result;
        }
        return int.MaxValue;
    }
    
    // Debug method to log current points
    public void DebugLogPoints()
    {
        Debug.Log($"=== LaserLinePoints ({linePoints.Count} points) ===");
        for (int i = 0; i < linePoints.Count; i++)
        {
            string pointName = linePoints[i]?.name ?? "NULL";
            Debug.Log($"{i}: {pointName}");
        }
    }
}