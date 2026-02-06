using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(LineController))]
public class MirrorMoveController : MonoBehaviour
{
    [Header("Basic")]
    [SerializeField] private float rotationAmount = 10.0f;
    [SerializeField] private float rotationDuration = 0.3f;
    [SerializeField] private Transform mirrorPoint;
    [SerializeField] private bool canMoveObject = true;
    
    [Header("Drag Settings")]
    [SerializeField] private float dragToRotationSpeed = 0.1f;
    [SerializeField] private bool invertDrag = false;
    
    [Header("References")]
    [SerializeField] private LigthsController mirrorLight;
    [SerializeField] private MirrorState mirrorState;
    [SerializeField] private Light2D onTapLight2D;
    [SerializeField] private LayerMask layerMask;
    
    // Components
    private LineController _lineController;
    private Camera _mainCamera;
    
    // SOLID components
    private IMirrorStateController _stateController;
    private IMirrorTapDetector _tapDetector;
    private IMirrorRotator _rotator;
    private IMirrorVisualFeedback _visualFeedback;
    private IMirrorInputHandler _inputHandler;
    
    // Properties
    public Transform mirrorPointRef => mirrorPoint;
    public LineController lineController => _lineController;
    public event Action<MirrorMoveController> OnMirrorTapped;
    
    private void Awake()
    {
        InitializeComponents();
        InitializeSOLIDComponents();
    }
    
    private void InitializeComponents()
    {
        _lineController = GetComponent<LineController>();
        if (mirrorLight == null) mirrorLight = GetComponent<LigthsController>();
        
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogWarning($"[{name}] No main camera found");
            _mainCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    private void InitializeSOLIDComponents()
    {
        // Initialize state controller
        _stateController = new MirrorStateController(this, mirrorLight);
        _stateController.SwitchState(mirrorState);
        
        // Initialize tap detector
        _tapDetector = new MirrorTapDetector(this, _mainCamera, mirrorPoint, _stateController, layerMask);
        
        // Initialize rotator
        _rotator = new MirrorRotator(mirrorPoint, _lineController, rotationDuration);
        
        // Initialize visual feedback
        _visualFeedback = new MirrorVisualFeedback(onTapLight2D, this);
        
        // Initialize input handler
        _inputHandler = new MirrorInputHandler(
            this, _stateController, _tapDetector, _visualFeedback, _rotator,
            dragToRotationSpeed, invertDrag, rotationAmount);
            
        _inputHandler.OnMirrorTapped += (mirror) => OnMirrorTapped?.Invoke(mirror);
    }
    
    public void OnInputsSubscribe()
    {
        if (TouchInputManager.Instance != null)
        {
            TouchInputManager.Instance.OnTap += _inputHandler.HandleTap;
            TouchInputManager.Instance.OnDragDelta += _inputHandler.HandleDragDelta;
            TouchInputManager.Instance.OnDragEnd += _inputHandler.HandleDragEnd;
            TouchInputManager.Instance.OnPinch += _inputHandler.HandlePinch;
        }
    }
    
    public void OnInputsUnsubscribe()
    {
        if (TouchInputManager.Instance != null)
        {
            TouchInputManager.Instance.OnTap -= _inputHandler.HandleTap;
            TouchInputManager.Instance.OnDragDelta -= _inputHandler.HandleDragDelta;
            TouchInputManager.Instance.OnDragEnd -= _inputHandler.HandleDragEnd;
            TouchInputManager.Instance.OnPinch -= _inputHandler.HandlePinch;
        }
    }
    
    private void Update()
    {
        if (!_inputHandler.CanProcessInput() || mirrorPoint == null) return;
        if (_rotator.IsRotating) return;
        
        // Update rayline when not rotating or dragging
        _lineController?.ShotRayline(mirrorPoint.up, 100f);
        
        // Editor fallback
        HandleKeyboardInput();
    }
    
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            _rotator.Rotate(Vector3.forward * rotationAmount);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            _rotator.Rotate(Vector3.back * rotationAmount);
        }
    }
    
    public void SwitchMirrorState(MirrorState newState)
    {
        _stateController.SwitchState(newState);
    }
    
    // Public properties
    public float RotationAmount
    {
        get => rotationAmount;
        set
        {
            rotationAmount = value;
            if (_inputHandler is MirrorInputHandler handler)
            {
                handler.UpdateRotationAmount(value);
            }
        }
    }
    
    public MirrorState MirrorState => _stateController.CurrentState;
    
    public bool CanMoveObject
    {
        get => canMoveObject;
        set => canMoveObject = value;
    }
    
    // Editor testing
    private void OnMouseDown()
    {
        if (!Application.isPlaying) return;
        
        Vector2 mousePos = Input.mousePosition;
        _inputHandler.HandleTap(mousePos);
    }
}