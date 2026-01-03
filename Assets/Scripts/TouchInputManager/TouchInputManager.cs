// TouchInputManager.cs
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class TouchInputManager : MonoBehaviour
{
    public static TouchInputManager Instance { get; private set; }

    public event Action<Vector2> OnTap;
    public event Action<Vector2, Vector2> OnDrag; // start, current
    public event Action<Vector2, Vector2> OnDragDelta; // previous, current (optional)
    public event Action<Vector2> OnDragEnd;
    public event Action<Vector2> OnTouchDown;
    public event Action<Vector2> OnTouchUp;
    public event Action<Vector2> OnSwipe;
    public event Action<float> OnPinch;

    [Header("Tuning")]
    [SerializeField] private float tapMaxTime = 0.2f;
    [SerializeField] private float tapMaxDistance = 20f;
    [SerializeField] private float swipeMinDistance = 100f;

    private Vector2 touchStartPos;
    private Vector2 previousDragPos;
    private float touchStartTime;
    private bool dragging;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[TouchInputManager] Duplicate instance destroyed on {name}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
        Debug.Log($"[TouchInputManager] Instance set on {name}");
    }

    private void Update()
    {
#if UNITY_EDITOR
        HandleMouseAsTouch();
#else
        HandleTouches();
#endif
        if (Application.isEditor)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Debug.Log($"[TouchInputManager] Editor pinch (scroll) delta: {scroll}");
                OnPinch?.Invoke(scroll * 10f);
            }
        }
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(); // mouse or touch pointer id not needed for simple check
    }

    private void HandleTouches()
    {
        if (Input.touchCount == 0) return;

        // If any touch is over UI, ignore it (optional)
        if (IsPointerOverUI(Input.GetTouch(0).position))
        {
            // Debug.Log("[TouchInputManager] Touch over UI - ignoring");
            return;
        }

        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = t.position;
                    previousDragPos = t.position;
                    touchStartTime = Time.time;
                    dragging = false;
                    //Debug.Log($"[TouchInputManager] Touch Began at {touchStartPos}");
                    OnTouchDown?.Invoke(t.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (!dragging && Vector2.Distance(t.position, touchStartPos) > tapMaxDistance)
                    {
                        dragging = true;
                        //Debug.Log($"[TouchInputManager] Drag started at {touchStartPos}");
                    }
                    if (dragging)
                    {
                        OnDrag?.Invoke(touchStartPos, t.position);
                        OnDragDelta?.Invoke(previousDragPos, t.position);
                        //Debug.Log($"[TouchInputManager] Dragging. Start: {touchStartPos}, Current: {t.position}");
                        previousDragPos = t.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    float dt = Time.time - touchStartTime;
                    float dist = Vector2.Distance(t.position, touchStartPos);
                    //Debug.Log($"[TouchInputManager] Touch Ended at {t.position} (dt={dt:F3}s, dist={dist:F1}px)");
                    OnTouchUp?.Invoke(t.position);

                    if (dragging)
                    {
                        OnDragEnd?.Invoke(t.position);
                        //Debug.Log("[TouchInputManager] Drag ended");
                    }
                    else
                    {
                        if (dt <= tapMaxTime && dist <= tapMaxDistance)
                        {
                            //Debug.Log($"[TouchInputManager] Tap detected at {t.position}");
                            OnTap?.Invoke(t.position);
                        }
                        else if (dist >= swipeMinDistance)
                        {
                            Vector2 dir = (t.position - touchStartPos).normalized;
                            //Debug.Log($"[TouchInputManager] Swipe detected dir {dir}");
                            OnSwipe?.Invoke(dir);
                        }
                    }

                    dragging = false;
                    break;
            }
        }
        else if (Input.touchCount >= 2)
        {
            Touch a = Input.GetTouch(0);
            Touch b = Input.GetTouch(1);

            // ignore pinch if either touch is over UI
            if (IsPointerOverUI(a.position) || IsPointerOverUI(b.position)) return;

            Vector2 prevA = a.position - a.deltaPosition;
            Vector2 prevB = b.position - b.deltaPosition;
            float prevDist = Vector2.Distance(prevA, prevB);
            float curDist = Vector2.Distance(a.position, b.position);
            float delta = curDist - prevDist;
            if (Mathf.Abs(delta) > 0.01f)
            {
                //Debug.Log($"[TouchInputManager] Pinch delta: {delta:F2}");
                OnPinch?.Invoke(delta);
            }
        }
    }

    // Editor convenience: treat mouse as single touch
    private void HandleMouseAsTouch()
    {
        Vector2 mousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI(mousePos)) return;
            touchStartPos = mousePos;
            previousDragPos = mousePos;
            touchStartTime = Time.time;
            dragging = false;
            //Debug.Log($"[TouchInputManager] Mouse Down at {touchStartPos}");
            OnTouchDown?.Invoke(mousePos);
        }
        else if (Input.GetMouseButton(0))
        {
            if (!dragging && Vector2.Distance(mousePos, touchStartPos) > tapMaxDistance)
            {
                dragging = true;
                //Debug.Log($"[TouchInputManager] Mouse Drag started at {touchStartPos}");
            }
            if (dragging)
            {
                OnDrag?.Invoke(touchStartPos, mousePos);
                OnDragDelta?.Invoke(previousDragPos, mousePos);
                //Debug.Log($"[TouchInputManager] Mouse Dragging. Start: {touchStartPos}, Current: {mousePos}");
                previousDragPos = mousePos;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (IsPointerOverUI(mousePos)) return;
            float dt = Time.time - touchStartTime;
            float dist = Vector2.Distance(mousePos, touchStartPos);
            //Debug.Log($"[TouchInputManager] Mouse Up at {mousePos} (dt={dt:F3}s, dist={dist:F1}px)");
            OnTouchUp?.Invoke(mousePos);

            if (dragging)
            {
                OnDragEnd?.Invoke(mousePos);
                //Debug.Log("[TouchInputManager] Mouse Drag ended");
            }
            else
            {
                if (dt <= tapMaxTime && dist <= tapMaxDistance)
                {
                    //Debug.Log($"[TouchInputManager] Mouse Tap at {mousePos}");
                    OnTap?.Invoke(mousePos);
                }
                else if (dist >= swipeMinDistance)
                {
                    Vector2 dir = (mousePos - touchStartPos).normalized;
                    //Debug.Log($"[TouchInputManager] Mouse Swipe dir {dir}");
                    OnSwipe?.Invoke(dir);
                }
            }

            dragging = false;
        }
    }
}