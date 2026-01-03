using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

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

    // runtime
    private bool isDragging = false;

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
        //Debug.Log($"[MirrorMoveController] Awake on {name}");
    }

    public  void OnInputsSusbcribe()
    {
        // Subscribe to the touch manager events (requires TouchInputManager to expose these events)
        if (TouchInputManager.Instance != null)
        {
            TouchInputManager.Instance.OnTap += HandleTap;
            TouchInputManager.Instance.OnDragDelta += HandleDragDelta; // per-frame delta
            TouchInputManager.Instance.OnDragEnd += HandleDragEnd;
            TouchInputManager.Instance.OnPinch += HandlePinch;
            //Debug.Log($"[MirrorMoveController] Subscribed to TouchInputManager events on {name}");
        }
        else
        {
            Debug.LogWarning("[MirrorMoveController] TouchInputManager.Instance is null on OnEnable");
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
            //Debug.Log($"[MirrorMoveController] Unsubscribed from TouchInputManager events on {name}");
        }
    }

    private void Update()
    {
        if (!canMoveObject || mirrorPoint == null) return;
        if (DOTween.IsTweening(mirrorPoint)) return;

        lineController?.ShotRayline(mirrorPoint.up, 100f);

        // Editor keyboard fallback
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            //Debug.Log($"[MirrorMoveController] Left key pressed on {name}");
            RotateMirror(Vector3.forward * rotationAmount);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            //Debug.Log($"[MirrorMoveController] Right key pressed on {name}");
            RotateMirror(Vector3.back * rotationAmount);
        }
    }

    private void HandleTap(Vector2 screenPos)
    {
        Debug.Log($"[MirrorMoveController] HandleTap at {screenPos} on {name}");

        // Ignore taps over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[MirrorMoveController] Tap over UI - ignoring");
            return;
        }

        if (!canMoveObject || mirrorPoint == null)
        {
            Debug.Log("[MirrorMoveController] Tap ignored - cannot move or mirrorPoint null");
            return;
        }

        if (!IsScreenPosOverThisMirror(screenPos))
        {
            Debug.Log("[MirrorMoveController] Tap not over this mirror - ignoring");
            return;
        }

        // Only treat as tap if not currently dragging
        if (isDragging)
        {
            Debug.Log("[MirrorMoveController] Tap ignored because dragging is active");
            return;
        }

        Debug.Log("[MirrorMoveController] Tap accepted - rotating mirror right");
        RotateMirror(Vector3.back * rotationAmount);
    }

    // Use per-frame delta for smooth, intuitive rotation
    private void HandleDragDelta(Vector2 previousScreen, Vector2 currentScreen)
    {
        //Debug.Log($"[MirrorMoveController] HandleDragDelta prev:{previousScreen} cur:{currentScreen} on {name}");

        if (!canMoveObject || mirrorPoint == null)
        {
            //Debug.Log("[MirrorMoveController] Drag ignored - cannot move or mirrorPoint null");
            return;
        }

        // If drag hasn't been flagged as started for this mirror, check start hit
        if (!isDragging)
        {
            // if (!IsScreenPosOverThisMirror(previousScreen))
            // {
            //     Debug.Log("[MirrorMoveController] Drag start not over this mirror - ignoring until a valid start");
            //     return;
            // }
            isDragging = true;
            //Debug.Log("[MirrorMoveController] Dragging started for mirror");
        }

        Vector2 delta = currentScreen - previousScreen;
        float rotationDelta = delta.x * dragToRotationSpeed;
        if (invertDrag) rotationDelta = -rotationDelta;

        // Apply rotation around local Z (adjust axis if your mirror uses a different axis)
        Vector3 e = mirrorPoint.eulerAngles;
        mirrorPoint.eulerAngles = new Vector3(e.x, e.y, e.z + rotationDelta);

        //Debug.Log($"[MirrorMoveController] Applied rotationDelta {rotationDelta:F2} -> new Z {mirrorPoint.eulerAngles.z:F2}");
        lineController?.ShotRayline(mirrorPoint.up, 100f);
    }

    private void HandleDragEnd(Vector2 screenPos)
    {
        if (isDragging)
        {
            isDragging = false;
            //Debug.Log("[MirrorMoveController] Drag ended");
            // Optionally: snap to nearest angle or start a tween to smooth final rotation
        }
    }

    private void HandlePinch(float delta)
    {
        Debug.Log($"[MirrorMoveController] HandlePinch delta {delta:F2} on {name}");
        rotationAmount += delta * pinchSensitivity;
        rotationAmount = Mathf.Clamp(rotationAmount, minRotationAmount, maxRotationAmount);
        Debug.Log($"[MirrorMoveController] rotationAmount adjusted to {rotationAmount:F2}");
    }

    private void RotateMirror(Vector3 rotation)
    {
        if (mirrorPoint == null)
        {
            //Debug.LogWarning("[MirrorMoveController] RotateMirror called but mirrorPoint is null");
            return;
        }
        if (DOTween.IsTweening(mirrorPoint))
        {
            //Debug.Log("[MirrorMoveController] RotateMirror skipped because a tween is active");
            return;
        }

        //Debug.Log($"[MirrorMoveController] Starting RotateMirror by {rotation} on {name}");
        mirrorPoint.DORotate(mirrorPoint.eulerAngles + rotation, rotationDuration, RotateMode.Fast)
            .OnUpdate(() => lineController?.ShotRayline(mirrorPoint.up, 100f))
            .OnComplete(() => Debug.Log("[MirrorMoveController] Rotation complete"));
    }

    public void SwitchMirrorState(MirrorState newState)
    {
        mirrorState = newState;
        Debug.Log($"[MirrorMoveController] SwitchMirrorState -> {mirrorState}");
        switch (mirrorState)
        {
            case MirrorState.Active:
                Debug.Log("[MirrorMoveController] Mirror is now Active");
                mirrorLight?.ForceActivate();
                canMoveObject = true;
                break;
            case MirrorState.Deactive:
                Debug.Log("[MirrorMoveController] Mirror is now Deactive");
                mirrorLight?.ForceDeactivate();
                canMoveObject = false;
                break;
            case MirrorState.Setted:
                Debug.Log("[MirrorMoveController] Mirror is now Setted");
                mirrorLight?.ForceActivate();
                canMoveObject = false;
                break;
        }
    }

    private bool IsScreenPosOverThisMirror(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[MirrorMoveController] No main camera found for raycast");
            return false;
        }

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            bool hitThis = hit.transform == mirrorPoint || hit.transform.IsChildOf(mirrorPoint);
            Debug.Log($"[MirrorMoveController] Raycast hit {hit.transform.name} (this mirror? {hitThis})");
            return hitThis;
        }

        Debug.Log("[MirrorMoveController] Raycast hit nothing");
        return false;
    }

    // Optional editor mouse callbacks for quick debug (still rely on raycast)
    private void OnMouseDown()
    {
        Debug.Log($"[MirrorMoveController] OnMouseDown on {gameObject.name}");
    }

    private void OnMouseDrag()
    {
        Debug.Log($"[MirrorMoveController] OnMouseDrag on {gameObject.name}");
    }
}