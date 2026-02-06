using UnityEngine;
using UnityEngine.EventSystems;

public class MirrorInputHandler : IMirrorInputHandler
{
    private readonly MirrorMoveController _mirror;
    private readonly IMirrorStateController _stateController;
    private readonly IMirrorTapDetector _tapDetector;
    private readonly IMirrorVisualFeedback _visualFeedback;
    private readonly IMirrorRotator _rotator;
    private readonly float _dragSpeed;
    private readonly bool _invertDrag;
    
    private bool _isDragging = false;
    private float _rotationAmount;
    
    public event System.Action<MirrorMoveController> OnMirrorTapped;
    
    public MirrorInputHandler(
        MirrorMoveController mirror,
        IMirrorStateController stateController,
        IMirrorTapDetector tapDetector,
        IMirrorVisualFeedback visualFeedback,
        IMirrorRotator rotator,
        float dragSpeed,
        bool invertDrag,
        float rotationAmount)
    {
        _mirror = mirror;
        _stateController = stateController;
        _tapDetector = tapDetector;
        _visualFeedback = visualFeedback;
        _rotator = rotator;
        _dragSpeed = dragSpeed;
        _invertDrag = invertDrag;
        _rotationAmount = rotationAmount;
    }
    
    public bool CanProcessInput()
    {
        return _stateController.CanMoveObject && !_rotator.IsRotating;
    }
    
    public void HandleTap(Vector2 screenPos)
    {
        if (!CanProcessInput()) return;
        
        // Ignore UI taps
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log($"[{_mirror.name}] Tap over UI - ignoring");
            return;
        }
        
        bool isOverMirror = _tapDetector.IsTapOverMirror(screenPos);
        Debug.Log($"[{_mirror.name}] Is tap over this mirror? {isOverMirror}");
        
        // Visual feedback for any tap over mirror
        if (isOverMirror && _visualFeedback != null)
        {
            _visualFeedback.ToggleLight();
        }
        
        // Only rotate if conditions are met
        if (_isDragging || !isOverMirror) return;
        
        Debug.Log($"[{_mirror.name}] Tap accepted - rotating mirror");
        _rotator.Rotate(Vector3.back * _rotationAmount);
        OnMirrorTapped?.Invoke(_mirror);
    }
    
    public void HandleDragDelta(Vector2 previousScreen, Vector2 currentScreen)
    {
        if (!_stateController.CanMoveObject || _rotator.IsRotating) return;
        
        if (!_isDragging)
        {
            _isDragging = true;
        }
        
        Vector2 delta = currentScreen - previousScreen;
        float rotationDelta = delta.x * _dragSpeed;
        if (_invertDrag) rotationDelta = -rotationDelta;
        
        Vector3 e = _mirror.mirrorPointRef.eulerAngles;
        _mirror.mirrorPointRef.eulerAngles = new Vector3(e.x, e.y, e.z + rotationDelta);
        
        _mirror.lineController?.ShotRayline(_mirror.mirrorPointRef.up, 100f);
    }
    
    public void HandleDragEnd(Vector2 screenPos)
    {
        if (_isDragging)
        {
            _isDragging = false;
        }
    }
    
    public void HandlePinch(float delta)
    {
        _rotationAmount += delta * 0.02f; // Default sensitivity
        _rotationAmount = Mathf.Clamp(_rotationAmount, 1f, 90f);
    }
    
    public void UpdateRotationAmount(float amount)
    {
        _rotationAmount = amount;
    }
}