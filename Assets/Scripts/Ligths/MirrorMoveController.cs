using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Rendering.Universal;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(LineController))]
public class MirrorMoveController : MonoBehaviour
{
    [Header("Basic")]
    [SerializeField] private float rotationAmount = 10.0f;
    [SerializeField] public float rotationDuration = 0.3f;
    [SerializeField] public Transform mirrorPoint;
    [SerializeField] private bool CanMoveObject = true;

    private LineController lineController;
    [SerializeField] private LigthsController mirrorLight;
    [SerializeField] private MirrorState mirrorState;

    [Header("Drag / Pinch Settings")]
    [SerializeField] private float dragToRotationSpeed = 0.1f; // degrees per pixel
    [SerializeField] private bool invertDrag = false; // flip direction if needed
    [SerializeField] private float pinchSensitivity = 0.02f;
    [SerializeField] private float minRotationAmount = 1f;
    [SerializeField] private float maxRotationAmount = 90f;
    public event Action<MirrorMoveController> OnMirrorTapped;
    public LayerMask layerMask;
    
    // runtime
    public Light2D OnTapLigth2D;
    private bool isDragging = false;
    private Coroutine lightToggleCoroutine;
    private Camera mainCamera;

    public float RotationAmount
    {
        get => rotationAmount;
        set
        {
            rotationAmount = value;
            Debug.Log($"[MirrorMoveController] RotationAmount set to {rotationAmount}");
        }
    }

    public MirrorState MirrorState
    {
        get => mirrorState;
        set
        {
            mirrorState = value;
            Debug.Log($"[MirrorMoveController] MirrorState set to {mirrorState}");
        }
    }

    public bool canMoveObject
    {
        get => CanMoveObject;
        set
        {
            CanMoveObject = value;
            //Debug.Log($"[MirrorMoveController] canMoveObject set to {CanMoveObject}");
        }
    }

    private void Awake()
    {
        lineController = GetComponent<LineController>();
        if (lineController == null) Debug.LogWarning($"[MirrorMoveController] No LineController on {name}");
        if (mirrorLight == null) mirrorLight = GetComponent<LigthsController>();
        
        // Cache the camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning($"[MirrorMoveController] No main camera found. Looking for any camera...");
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    public void OnInputsSusbcribe()
    {
        if (TouchInputManager.Instance != null)
        {
            TouchInputManager.Instance.OnTap += HandleTap;
            TouchInputManager.Instance.OnDragDelta += HandleDragDelta;
            TouchInputManager.Instance.OnDragEnd += HandleDragEnd;
            TouchInputManager.Instance.OnPinch += HandlePinch;
        }
        else
        {
            Debug.LogWarning("[MirrorMoveController] TouchInputManager.Instance is null");
        }
    }

    public void OnInputsUnsubsbcribe()
    {
        if (TouchInputManager.Instance != null)
        {
            TouchInputManager.Instance.OnTap -= HandleTap;
            TouchInputManager.Instance.OnDragDelta -= HandleDragDelta;
            TouchInputManager.Instance.OnDragEnd -= HandleDragEnd;
            TouchInputManager.Instance.OnPinch -= HandlePinch;
        }
    }

    private void Update()
    {
        if (!canMoveObject || mirrorPoint == null) return;
        if (DOTween.IsTweening(mirrorPoint)) return;
        if (isDragging) return;
        if (lineController == null) return;
        if (mirrorLight == null) return;
        if (mirrorState != MirrorState.Active) return;
        lineController?.ShotRayline(mirrorPoint.up, 100f);

        // Editor keyboard fallback
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            RotateMirror(Vector3.forward * rotationAmount);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            RotateMirror(Vector3.back * rotationAmount);
        }
    }

    private void HandleTap(Vector2 screenPos)
    {
        // Get camera reference
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // Debug: Log the actual screen position
        Debug.Log($"[{name}] HandleTap at screen: {screenPos}, Camera: {mainCamera.name}, Viewport: {mainCamera.ScreenToViewportPoint(screenPos)}");

        // Ignore taps over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log($"[{name}] Tap over UI - ignoring");
            return;
        }

        // Check if tap is over this mirror
        bool isOverMirror = IsScreenPosOverThisMirror2D(screenPos);
        
        Debug.Log($"[{name}] Is tap over this mirror? {isOverMirror}");

        // Toggle light intensity if tap is over this mirror
        if (isOverMirror && OnTapLigth2D != null)
        {
            ToggleLightWithAnimation();
            Debug.Log($"[{name}] Light toggled to {OnTapLigth2D.intensity}");
        }

        // Only proceed with mirror rotation if conditions are met
        if (!canMoveObject || mirrorPoint == null)
        {
            Debug.Log($"[{name}] Tap ignored - cannot move or mirrorPoint null");
            return;
        }

        // Only treat as rotation tap if not currently dragging AND tap is over this mirror
        if (isDragging || !isOverMirror)
        {
            if (isDragging) Debug.Log($"[{name}] Tap ignored - currently dragging");
            if (!isOverMirror) Debug.Log($"[{name}] Tap ignored - not over this mirror");
            return;
        }

        Debug.Log($"[{name}] Tap accepted - rotating mirror right");
        //RotateMirror(Vector3.back * rotationAmount);
        OnMirrorTapped?.Invoke(this);
    }

    private void HandleDragDelta(Vector2 previousScreen, Vector2 currentScreen)
    {
        if (!canMoveObject || mirrorPoint == null) return;

        if (!isDragging)
        {
            isDragging = true;
        }

        Vector2 delta = currentScreen - previousScreen;
        float rotationDelta = delta.x * dragToRotationSpeed;
        if (invertDrag) rotationDelta = -rotationDelta;

        Vector3 e = mirrorPoint.eulerAngles;
        mirrorPoint.eulerAngles = new Vector3(e.x, e.y, e.z + rotationDelta);

        lineController?.ShotRayline(mirrorPoint.up, 100f);
    }

    private void HandleDragEnd(Vector2 screenPos)
    {
        if (isDragging)
        {
            isDragging = false;
        }
    }

    private void HandlePinch(float delta)
    {
        rotationAmount += delta * pinchSensitivity;
        rotationAmount = Mathf.Clamp(rotationAmount, minRotationAmount, maxRotationAmount);
    }

    private void RotateMirror(Vector3 rotation)
    {
        if (mirrorPoint == null) return;
        if (DOTween.IsTweening(mirrorPoint)) return;

        mirrorPoint.DORotate(mirrorPoint.eulerAngles + rotation, rotationDuration, RotateMode.Fast)
            .OnUpdate(() => lineController?.ShotRayline(mirrorPoint.up, 100f))
            .OnComplete(() => Debug.Log($"[{name}] Rotation complete"));
    }

    public void SwitchMirrorState(MirrorState newState)
    {
        mirrorState = newState;
        Debug.Log($"[{name}] SwitchMirrorState -> {mirrorState}");
        switch (mirrorState)
        {
            case MirrorState.Active:
                Debug.Log($"[{name}] Mirror is now Active");
                mirrorLight?.ForceActivate();
                canMoveObject = true;
                break;
            case MirrorState.Deactive:
                Debug.Log($"[{name}] Mirror is now Deactive");
                mirrorLight?.ForceDeactivate();
                canMoveObject = false;
                break;
            case MirrorState.Setted:
                Debug.Log($"[{name}] Mirror is now Setted");
                mirrorLight?.ForceActivate();
                canMoveObject = false;
                break;
        }
    }

    private bool IsScreenPosOverThisMirror2D(Vector2 screenPos)
    {
        if(mirrorState == MirrorState.Deactive)
        {
            Debug.Log($"[{name}] Mirror is Deactive - ignoring tap detection");
            return false;
        }
        if (mainCamera == null || mirrorPoint == null) 
        {
            mainCamera = Camera.main;
            if (mainCamera == null) 
            {
                Debug.LogWarning($"[{name}] No camera available");
                return false;
            }
        }

        try
        {
            // For 2D, we need to use a specific z-depth. 
            // The z-depth should be the distance from camera to your objects
            // If your mirrors are at z = 0 and camera is at z = -10, use 10
            // Or better, use the mirror's z position relative to camera
            float zDepth = Mathf.Abs(mainCamera.transform.position.z - mirrorPoint.position.z);
            if (Mathf.Approximately(zDepth, 0)) zDepth = 10f; // Default fallback
            
            Vector3 worldPoint3D = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDepth));
            Vector2 worldPoint2D = new Vector2(worldPoint3D.x, worldPoint3D.y);
            
            // Debug the conversion
            Debug.Log($"[{name}] Screen {screenPos} -> World {worldPoint2D}, zDepth: {zDepth}, Mirror pos: {mirrorPoint.position}");

            // Check for colliders at this point
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint2D);
            
            // Debug: log all hits
            if (hits.Length > 0)
            {
                Debug.Log($"[{name}] Found {hits.Length} colliders at point {worldPoint2D}:");
                foreach (Collider2D hit in hits)
                {
                    Debug.Log($"  - {hit.name} ({hit.transform.position})");
                }
            }
            else
            {
                Debug.Log($"[{name}] No colliders found at point {worldPoint2D}");
            }
            
            foreach (Collider2D hit in hits)
            {
                // Check if this is OUR mirror
                if (hit.transform == this.transform || hit.transform == mirrorPoint || 
                    hit.transform.IsChildOf(this.transform) || (mirrorPoint != null && hit.transform.IsChildOf(mirrorPoint)))
                {
                    Debug.Log($"[{name}] ✓ Hit detected on THIS mirror!");
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[{name}] Error in IsScreenPosOverThisMirror2D: {e.Message}");
            return false;
        }
        
        return false;
    }

    // Alternative: Use raycast instead of OverlapPointAll
    private bool IsScreenPosOverThisMirror2D_Raycast(Vector2 screenPos)
    {
        if (mainCamera == null || mirrorPoint == null) 
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return false;
        }

        // Create a ray from screen point
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0));
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, layerMask);
        
        Debug.Log($"[{name}] Raycast from screen {screenPos}, found {hits.Length} hits");
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                Debug.Log($"  - Hit: {hit.collider.name}");
                if (hit.collider.transform == this.transform || hit.collider.transform == mirrorPoint ||
                    hit.collider.transform.IsChildOf(this.transform))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private void ToggleLightWithAnimation()
    {
        if (OnTapLigth2D == null) return;
        
        float targetIntensity = (OnTapLigth2D.intensity < 0.5f) ? 1.0f : 0.0f;
        
        if (lightToggleCoroutine != null)
        {
            StopCoroutine(lightToggleCoroutine);
        }
        
        lightToggleCoroutine = StartCoroutine(AnimateLightIntensity(OnTapLigth2D.intensity, targetIntensity, 0.2f));
    }

    private IEnumerator AnimateLightIntensity(float startIntensity, float targetIntensity, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);
            OnTapLigth2D.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }
        
        OnTapLigth2D.intensity = targetIntensity;
        lightToggleCoroutine = null;
    }

    private void OnMouseDown()
    {
        // For editor testing
        if (!Application.isPlaying) return;
        
        Vector2 mousePos = Input.mousePosition;
        Debug.Log($"[{name}] OnMouseDown at {mousePos}");
        HandleTap(mousePos);
    }
}