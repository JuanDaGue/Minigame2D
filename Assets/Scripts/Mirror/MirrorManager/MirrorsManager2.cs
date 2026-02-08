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
    private MirrorCleanupSystem _cleanupSystem;
    private LaserLinePointUpdater _linePointUpdater;
    
    // State
    private MirrorMoveController _currentMirror;
    private LineController _currentLineController;
    private bool _isInitialized = false;
    
    // Properties
    public MirrorMoveController CurrentMirror => _currentMirror;
    public MirrorCollection MirrorCollection => _mirrorCollection;
    
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
            if (mirror != null)
            {
                _mirrorCollection.AddMirror(mirror);
                Debug.Log($"[MirrorManager2] Added initial mirror: {mirror.name}");
            }
        }
        
        // Initialize line point updater
        _linePointUpdater = new LaserLinePointUpdater(laserLinePoints);
        
        // Initialize subscription manager
        _subscriptionManager = new MirrorSubscriptionManager(
            _mirrorCollection, laserLinePoints, camerasManager);
        
        // Subscribe to events
        _subscriptionManager.OnMirrorTapped += HandleMirrorTapped;
        _subscriptionManager.OnMirrorHit += HandleMirrorHit;
        
        Debug.Log($"[MirrorManager2] Subscribed to MirrorTapped event");
        
        // Initialize cleanup system
         _cleanupSystem = new MirrorCleanupSystem(
        _mirrorCollection, _subscriptionManager, _linePointUpdater, this);
        
        // Initialize light reducer
        _lightReducer = new LightReducer(_mirrorCollection, lightReductionAmount);
        
        Debug.Log($"[MirrorManager2] Components initialized");
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
        
        // Configure mirror states correctly
        ConfigureMirrorStates();
        
        // Subscribe ALL mirrors (Setted mirrors need to receive taps too)
        _subscriptionManager.SubscribeAll();
        
        // Subscribe to global time events
        if (globalTime != null)
            globalTime.OnIntervalReached += () => _lightReducer.ReduceLights();
        
        Debug.Log($"[MirrorManager2] Initialized with {_mirrorCollection.Mirrors.Count} mirrors");
        DebugMirrorChain();
    }
    
    // Configure mirror states: Last mirror = Active, others = Setted
    private void ConfigureMirrorStates()
    {
        Debug.Log($"[MirrorManager2] Configuring {_mirrorCollection.Mirrors.Count} mirrors");
        
        for (int i = 0; i < _mirrorCollection.Mirrors.Count; i++)
        {
            var mirror = _mirrorCollection.Mirrors[i];
            if (mirror == null) continue;
            
            // Last mirror is Active (can move and emit)
            if (i == _mirrorCollection.Mirrors.Count - 1)
            {
                mirror.SwitchMirrorState(MirrorState.Active);
                SetCurrentMirror(mirror);
                Debug.Log($"[MirrorManager2] Mirror {i}: {mirror.name} → Active (Current)");
            }
            // All other mirrors are Setted (cannot move but can be tapped)
            else
            {
                mirror.SwitchMirrorState(MirrorState.Setted);
                Debug.Log($"[MirrorManager2] Mirror {i}: {mirror.name} → Setted");
            }
        }
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
        
        Debug.Log($"[MirrorManager2] ===== NEW MIRROR HIT =====");
        
        // Change previous current mirror to Setted
        if (_currentMirror != null)
        {
            _currentMirror.SwitchMirrorState(MirrorState.Setted);
            Debug.Log($"[MirrorManager2] Previous current {_currentMirror.name} → Setted");
        }
        
        // Add new mirror to collection
        _mirrorCollection.AddMirror(controller);
        
        // New mirror becomes Active
        controller.SwitchMirrorState(MirrorState.Active);
        
        // Set as current mirror
        SetCurrentMirror(controller);
        
        // Subscribe to new mirror
        _subscriptionManager.SubscribeMirror(controller);
        
        // Subscribe to line points
        _linePointUpdater.SubscribeMirrorToLinePoints(controller.transform);
        
        Debug.Log($"[MirrorManager2] New mirror added: {controller.name} → Active");
        DebugMirrorChain();
    }
    
    public void HandleMirrorTapped(MirrorMoveController tapped)
    {
        Debug.Log($"[MirrorManager2] ===== SETTED MIRROR TAPPED =====");
        Debug.Log($"[MirrorManager2] Tapped mirror: {tapped?.name}");
        
        if (tapped == null) return;
        
        // Verify it's a Setted mirror
        if (tapped.MirrorState != MirrorState.Setted)
        {
            Debug.LogWarning($"[MirrorManager2] {tapped.name} is not Setted (State: {tapped.MirrorState}) - ignoring tap");
            return;
        }
        
        // Find index of tapped mirror
        int tappedIndex = _mirrorCollection.IndexOf(tapped);
        if (tappedIndex < 0)
        {
            Debug.LogError($"[MirrorManager2] {tapped.name} not found in collection!");
            return;
        }
        
        Debug.Log($"[MirrorManager2] {tapped.name} found at index {tappedIndex}");
        Debug.Log($"[MirrorManager2] Total mirrors before cleanup: {_mirrorCollection.Mirrors.Count}");
        
        // Clean mirrors after the tapped one
        _cleanupSystem.CleanMirrorsAfterTap(tapped);
        
        // The tapped mirror should now become Active
        tapped.SwitchMirrorState(MirrorState.Active);
        
        // Set as current mirror
        SetCurrentMirror(tapped);
        
        // Resubscribe all mirrors after cleanup
        _subscriptionManager.ResubscribeAll();
        
        // Update line points
        UpdateAllLinePoints();
        
        Debug.Log($"[MirrorManager2] ===== CLEANUP COMPLETE =====");
        Debug.Log($"[MirrorManager2] {tapped.name} is now Active and current");
        DebugMirrorChain();
    }
    
    private void UpdateAllLinePoints()
    {
        _linePointUpdater?.UpdateLinePoints();
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
    
    // Debug method to show current mirror chain
    public void DebugMirrorChain()
    {
        Debug.Log($"=== Mirror Chain ({_mirrorCollection.Mirrors.Count} mirrors) ===");
        for (int i = 0; i < _mirrorCollection.Mirrors.Count; i++)
        {
            var mirror = _mirrorCollection.Mirrors[i];
            if (mirror != null)
            {
                string isCurrent = (mirror == _currentMirror) ? " [CURRENT]" : "";
                Debug.Log($"{i}: {mirror.name} - State: {mirror.MirrorState}, CanMove: {mirror.CanMoveObject}{isCurrent}");
            }
        }
    }
    
    [ContextMenu("Debug Subscriptions")]
    public void DebugSubscriptions()
    {
        Debug.Log($"=== Mirror Subscriptions Debug ===");
        Debug.Log($"Total mirrors: {_mirrorCollection.Mirrors.Count}");
        
        foreach (var mirror in _mirrorCollection.Mirrors)
        {
            if (mirror != null)
            {
                Debug.Log($"- {mirror.name}: State={mirror.MirrorState}, CanMove={mirror.CanMoveObject}");
            }
        }
    }
}