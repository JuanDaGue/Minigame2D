using UnityEngine;
using System;

public class TouchInputManager : MonoBehaviour
{
    public static TouchInputManager Instance { get; private set; }

    public event Action<Vector2> OnTap;
    public event Action<Vector2, Vector2> OnDrag; // start, current
    public event Action<Vector2> OnSwipe; // direction normalized
    public event Action<float> OnPinch; // delta magnitude (positive = zoom in)

    [Header("Tuning")]
    [SerializeField] float tapMaxTime = 0.2f;
    [SerializeField] float tapMaxDistance = 20f;
    [SerializeField] float swipeMinDistance = 100f;

    Vector2 touchStartPos;
    float touchStartTime;
    bool dragging;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        #if UNITY_EDITOR
        HandleMouseAsTouch();
        #endif
        HandleTouches();
    }

    void HandleTouches()
    {
        if (Input.touchCount == 0) return;

        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = t.position;
                    touchStartTime = Time.time;
                    dragging = false;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (!dragging && Vector2.Distance(t.position, touchStartPos) > tapMaxDistance)
                        dragging = true;
                    if (dragging) OnDrag?.Invoke(touchStartPos, t.position);
                    break;

                case TouchPhase.Ended:
                    float dt = Time.time - touchStartTime;
                    float dist = Vector2.Distance(t.position, touchStartPos);
                    if (dt <= tapMaxTime && dist <= tapMaxDistance)
                        OnTap?.Invoke(t.position);
                    else if (dist >= swipeMinDistance)
                    {
                        Vector2 dir = (t.position - touchStartPos).normalized;
                        OnSwipe?.Invoke(dir);
                    }
                    dragging = false;
                    break;
            }
        }
        else if (Input.touchCount >= 2)
        {
            // Pinch / Zoom
            Touch a = Input.GetTouch(0);
            Touch b = Input.GetTouch(1);
            Vector2 prevA = a.position - a.deltaPosition;
            Vector2 prevB = b.position - b.deltaPosition;
            float prevDist = Vector2.Distance(prevA, prevB);
            float curDist = Vector2.Distance(a.position, b.position);
            float delta = curDist - prevDist;
            OnPinch?.Invoke(delta);
        }
    }

    // Editor convenience: treat mouse as single touch
    void HandleMouseAsTouch()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            touchStartTime = Time.time;
            dragging = false;
        }
        else if (Input.GetMouseButton(0))
        {
            if (!dragging && Vector2.Distance((Vector2)Input.mousePosition, touchStartPos) > tapMaxDistance)
                dragging = true;
            if (dragging) OnDrag?.Invoke(touchStartPos, Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            float dt = Time.time - touchStartTime;
            float dist = Vector2.Distance((Vector2)Input.mousePosition, touchStartPos);
            if (dt <= tapMaxTime && dist <= tapMaxDistance) OnTap?.Invoke(Input.mousePosition);
            else if (dist >= swipeMinDistance) OnSwipe?.Invoke(((Vector2)Input.mousePosition - touchStartPos).normalized);
            dragging = false;
        }

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f) OnPinch?.Invoke(scroll * 10f); // editor zoom
    }
}