using UnityEngine;
using DG.Tweening;

public class MirrorRotator : IMirrorRotator
{
    private readonly Transform _mirrorPoint;
    private readonly LineController _lineController;
    private readonly float _rotationDuration;
    
    public bool IsRotating => DOTween.IsTweening(_mirrorPoint);
    
    public MirrorRotator(Transform mirrorPoint, LineController lineController, float rotationDuration)
    {
        _mirrorPoint = mirrorPoint;
        _lineController = lineController;
        _rotationDuration = rotationDuration;
    }
    
    public void Rotate(Vector3 rotation)
    {
        if (_mirrorPoint == null || IsRotating) return;
        
        _mirrorPoint.DORotate(_mirrorPoint.eulerAngles + rotation, _rotationDuration, RotateMode.Fast)
            .OnUpdate(UpdateRayline)
            .OnComplete(() => Debug.Log($"[{_mirrorPoint.name}] Rotation complete"));
    }
    
    public void RotateToAngle(float angle)
    {
        if (_mirrorPoint == null || IsRotating) return;
        
        _mirrorPoint.DORotate(new Vector3(0, 0, angle), _rotationDuration, RotateMode.Fast)
            .OnUpdate(UpdateRayline);
    }
    
    private void UpdateRayline()
    {
        _lineController?.ShotRayline(_mirrorPoint.up, 100f);
    }
}