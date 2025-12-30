using UnityEngine;
using System.Collections.Generic;

public class LaserLinePoints : MonoBehaviour
{
    [Tooltip("List of mirror points with LightsController components")]
    public List<Transform> linePoints = new List<Transform>();

    [SerializeField] private LineRenderer generalLightLine;

  public void SubscribeLinePoints(Transform point)
{
    if (point == null) return;

    // Prevent duplicates by GameObject
    if (linePoints.Exists(p => p != null && p.gameObject.GetInstanceID() == point.gameObject.GetInstanceID()))
        return;

    if (point.GetComponent<MirrorMoveController>() != null)
    {
        linePoints.Add(point);
        Debug.Log("Subscribed point: " + point.name);
    }
}

    public void UnsubscribeLinePoints(Transform point)
    {
        if (point != null && linePoints.Contains(point))
        {
            linePoints.Remove(point);
        }
    }

    public void ClearLinePoints()
    {
        linePoints.Clear();
        if (generalLightLine != null)
            generalLightLine.positionCount = 0;
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
}