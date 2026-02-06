using System.Collections.Generic;
using UnityEngine;

public class MirrorManager2 : MonoBehaviour, IMirrorManager
{
    [Header("Dependencies")]
    [SerializeField] private LaserLinePoints laserLinePoints;
    [SerializeField] private GlobalTime globalTime;
    [SerializeField] private float lightReductionAmount = 0.1f;
    [SerializeField] private CamerasManager camerasManager;
    
    [Header("Mirror Configuration")]
    [SerializeField] private List<MirrorMoveController> initialMirrors = new List<MirrorMoveController>();
    [SerializeField] private MirrorMoveController startingMirror;
    
    // SOLID Components
    private MirrorCollection _mirrorCollection;
    private MirrorSubscriptionManager _subscriptionManager;
    private LightReducer _lightReducer;
    
    // State
    private MirrorMoveController _currentMirror;
    private LineController _currentLineController;
    private bool _isInitialized = false;
    
    // Properties
    public MirrorMoveController CurrentMirror => _currentMirror;
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // Initialize collection with initial mirrors
        _mirrorCollection = new MirrorCollection();
        foreach (var mirror in initialMirrors)
        {
            _mirrorCollection.AddMirror(mirror);
        }
        
        // Initialize subscription manager
        _subscriptionManager = new MirrorSubscriptionManager(
            _mirrorCollection, laserLinePoints, camerasManager);
        _subscriptionManager.OnMirrorTapped += HandleMirrorTapped;
        _subscriptionManager.OnMirrorHit += HandleMirrorHit;
        
        // Initialize light reducer
        _lightReducer = new LightReducer(_mirrorCollection, lightReductionAmount);
    }
    
    public void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        
        // Set starting mirror
        if (startingMirror == null && initialMirrors.Count > 0)
            startingMirror = initialMirrors[0];
        
        if (startingMirror != null && !_mirrorCollection.Contains(startingMirror))
            _mirrorCollection.AddMirror(startingMirror);
        
        // Activate starting mirror
        if (startingMirror != null)
        {
            SetCurrentMirror(startingMirror);
            startingMirror.SwitchMirrorState(MirrorState.Active);
        }
        
        // Subscribe to all mirrors
        _subscriptionManager.SubscribeAll();
        
        // Subscribe to global time events
        if (globalTime != null)
            globalTime.OnIntervalReached += () => _lightReducer.ReduceLights();
        
        Debug.Log($"[MirrorManager2] Initialized with {_mirrorCollection.Mirrors.Count} mirrors");
    }
    
    private void SetCurrentMirror(MirrorMoveController mirror)
    {
        if (mirror == null) return;
        
        // Update current references
        _currentMirror = mirror;
        _currentLineController = mirror.GetComponent<LineController>();
        
        // Move camera to new mirror
        camerasManager?.MoveCameraToTarget(mirror.transform, 1.5f, Vector3.zero);
        
        // Set layer for emission
        mirror.gameObject.layer = LayerMask.NameToLayer("Emitter");
        
        Debug.Log($"[MirrorManager2] Current mirror set to: {mirror.name}");
    }
    
    public void HandleMirrorHit(MirrorMoveController controller)
    {
        if (controller == null || _mirrorCollection.Contains(controller)) return;
        
        // Add new mirror to collection
        _mirrorCollection.AddMirror(controller);
        
        // Update state of previous current mirror
        if (_currentMirror != null)
        {
            _currentMirror.SwitchMirrorState(MirrorState.Setted);
        }
        
        // Set new current mirror
        SetCurrentMirror(controller);
        controller.SwitchMirrorState(MirrorState.Active);
        
        // Subscribe to new mirror
        _subscriptionManager.SubscribeMirror(controller);
        
        Debug.Log($"[MirrorManager2] New mirror hit: {controller.name}");
    }
    
    public void HandleMirrorTapped(MirrorMoveController tapped)
    {
        if (tapped == null) return;
        
        int idx = _mirrorCollection.IndexOf(tapped);
        if (idx < 0)
        {
            Debug.LogWarning($"[MirrorManager2] Tapped mirror not found: {tapped.name}");
            return;
        }
        
        // Remove mirrors after the tapped one
        _mirrorCollection.RemoveMirrorsAfter(idx);
        
        // Unsubscribe from removed mirrors
        for (int i = _mirrorCollection.Mirrors.Count - 1; i > idx; i--)
        {
            if (i < _mirrorCollection.Mirrors.Count)
            {
                var mirror = _mirrorCollection.Mirrors[i];
                _subscriptionManager.UnsubscribeMirror(mirror);
            }
        }
        
        // Set tapped mirror as current
        SetCurrentMirror(tapped);
        
        Debug.Log($"[MirrorManager2] Mirror tapped: {tapped.name}, removed mirrors after index {idx}");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (globalTime != null)
            globalTime.OnIntervalReached -= () => _lightReducer.ReduceLights();
        
        if (_subscriptionManager != null)
        {
            _subscriptionManager.OnMirrorTapped -= HandleMirrorTapped;
            _subscriptionManager.OnMirrorHit -= HandleMirrorHit;
            _subscriptionManager.UnsubscribeAll();
        }
    }
    
    // Helper method for adding mirrors at runtime
    public void AddMirror(MirrorMoveController mirror)
    {
        _mirrorCollection.AddMirror(mirror);
        _subscriptionManager.SubscribeMirror(mirror);
    }
}