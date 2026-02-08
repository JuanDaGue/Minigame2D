using UnityEngine;
public interface ILinePointUpdater
{
    void UpdateLinePoints();
    void SubscribeMirrorToLinePoints(Transform mirrorTransform);
    void UnsubscribeMirrorFromLinePoints(Transform mirrorTransform);
}