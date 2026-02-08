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
        bool canProcess = _stateController.CanMoveObject && !_rotator.IsRotating;
        return canProcess;
    }
    
    public void HandleTap(Vector2 screenPos)
    {
        Debug.Log($"[{_mirror.name}] ===== HANDLE TAP START =====");
        Debug.Log($"[{_mirror.name}] Current State: {_stateController.CurrentState}");
        
        if (!CanProcessInput() && _stateController.CurrentState != MirrorState.Setted) 
        {
            Debug.Log($"[{_mirror.name}] Cannot process input and not Setted, returning");
            return;
        }
        
        // Ignore UI taps
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log($"[{_mirror.name}] Tap over UI - ignoring");
            return;
        }
        
        bool isOverMirror = _tapDetector.IsTapOverMirror(screenPos);
        
        // Visual feedback for any tap over mirror
        if (isOverMirror && _visualFeedback != null)
        {
            _visualFeedback.ToggleLight();
        }
        
        // CRITICAL: Only Setted mirrors should trigger cleanup
        if (_stateController.CurrentState != MirrorState.Setted)
        {
            if (isOverMirror && _stateController.CurrentState == MirrorState.Active)
            {
                Debug.Log($"[{_mirror.name}] Active mirror tapped - rotating only");
                // Active mirrors still rotate
                _rotator.Rotate(Vector3.back * _rotationAmount);
            }
            else
            {
                Debug.Log($"[{_mirror.name}] Tap ignored - not a Setted mirror (State: {_stateController.CurrentState})");
            }
            return;
        }
        
        // Only proceed with mirror tap event if not dragging AND tap is over this mirror
        if (_isDragging)
        {
            Debug.Log($"[{_mirror.name}] Tap ignored - currently dragging");
            return;
        }
        
        if (!isOverMirror)
        {
            Debug.Log($"[{_mirror.name}] Tap ignored - not over this mirror");
            return;
        }
        
        Debug.Log($"[{_mirror.name}] ✓✓✓ SETTED MIRROR TAP - Firing cleanup event ✓✓✓");
        
        // Setted mirrors don't rotate - they trigger cleanup
        // _rotator.Rotate(Vector3.back * _rotationAmount);
        
        // Fire the tap event for cleanup
        OnMirrorTapped?.Invoke(_mirror);
        
        Debug.Log($"[{_mirror.name}] ===== HANDLE TAP END =====");
    }
    
    public void HandleDragDelta(Vector2 previousScreen, Vector2 currentScreen)
    {
        if (!_stateController.CanMoveObject || _rotator.IsRotating) return;
        
        if (!_isDragging)
        {
            _isDragging = true;
            Debug.Log($"[{_mirror.name}] Dragging started");
        }
        
        Vector2 delta = currentScreen - previousScreen;
        float rotationDelta = delta.x * _dragSpeed;
        if (_invertDrag) rotationDelta = -rotationDelta;
        
        if (_mirror.mirrorPointRef != null)
        {
            Vector3 e = _mirror.mirrorPointRef.eulerAngles;
            _mirror.mirrorPointRef.eulerAngles = new Vector3(e.x, e.y, e.z + rotationDelta);
            
            _mirror.lineController?.ShotRayline(_mirror.mirrorPointRef.up, 100f);
        }
    }
    
    public void HandleDragEnd(Vector2 screenPos)
    {
        if (_isDragging)
        {
            _isDragging = false;
            Debug.Log($"[{_mirror.name}] Dragging ended");
        }
    }
    
    public void HandlePinch(float delta)
    {
        _rotationAmount += delta * 0.02f;
        _rotationAmount = Mathf.Clamp(_rotationAmount, 1f, 90f);
        Debug.Log($"[{_mirror.name}] Rotation amount updated to: {_rotationAmount}");
    }
    
    public void UpdateRotationAmount(float amount)
    {
        _rotationAmount = amount;
    }
}