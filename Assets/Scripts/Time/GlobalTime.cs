using System;
using Unity.VisualScripting;
using UnityEngine;

public class GlobalTime : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float totalTime = 120f;
    [SerializeField] private float eventInterval = 20f;      // how often to fire OnIntervalReached
    [SerializeField] private float reductionAmount = 2f;     // optional amount to reduce interval by
    [SerializeField] private bool startPaused = false;

    [Header("Runtime (read only)")]
    [SerializeField] private float currentTime;
    private bool finishedFired = false;

    // Events
    public event Action OnIntervalReached;
    public event Action OnTimerFinished;

    // Internal
    private float nextIntervalTime;
    private bool isPaused;

    private void Awake()
    {
        // initialize values
        currentTime = totalTime;
        nextIntervalTime = Mathf.Max(0f, currentTime - eventInterval);
        isPaused = startPaused;
        Debug.Log($"[GlobalTime] Awake: totalTime={totalTime}, eventInterval={eventInterval}, startPaused={startPaused}");
    }

    private void Start()
    {
        Debug.Log("[GlobalTime] Start: timer started");
    }

    private void Update()
    {
        if (isPaused) return;
        if (currentTime <= 0f) return;

        Tick(Time.deltaTime);
    }

private void Tick(float delta)
{
    if (finishedFired) return; // already finished, nothing to do

    currentTime -= delta;
    if (currentTime < 0f) currentTime = 0f;

    // Handle one or more interval events that may have been crossed this frame
    // Only process intervals if eventInterval is positive
    if (eventInterval > 0f)
    {
        while (currentTime <= nextIntervalTime && nextIntervalTime > 0f)
        {
            Debug.Log($"[GlobalTime] Interval reached at time {currentTime:F2}s (nextIntervalTime={nextIntervalTime:F2})");
            SafeInvokeInterval();
            nextIntervalTime = Mathf.Max(0f, nextIntervalTime - eventInterval);
            // safety: break if somehow stuck
            if (eventInterval <= 0f) break;
        }
    }

    // Timer finished (run once)
    if (currentTime <= 0f && !finishedFired)
    {
        finishedFired = true;
        Debug.Log("[GlobalTime] Timer finished (firing once)");
        SafeInvokeFinished();

        // stop ticking to avoid re-entrancy
        isPaused = true;

        // call GameManager safely
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameManager.States.GameOver);
        }
        else
        {
            Debug.LogWarning("[GlobalTime] GameManager.Instance is null when trying to set GameOver.");
        }
    }
}


    private void SafeInvokeInterval()
    {
        OnIntervalReached?.Invoke();
    }

    private void SafeInvokeFinished()
    {
        try
        {
            OnTimerFinished?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GlobalTime] Exception in OnTimerFinished handler: {ex}");
        }
    }

    // Public control API

    /// <summary> Pause the timer. </summary>
    public void Pause()
    {
        isPaused = true;
        Debug.Log("[GlobalTime] Paused");
    }

    /// <summary> Resume the timer. </summary>
    public void Resume()
    {
        isPaused = false;
        Debug.Log("[GlobalTime] Resumed");
    }

    /// <summary> Reset timer to initial totalTime and recompute next interval. </summary>
    public void ResetTimer()
    {
        currentTime = totalTime;
        nextIntervalTime = Mathf.Max(0f, currentTime - eventInterval);
        isPaused = startPaused;
        finishedFired = false;
        Debug.Log("[GlobalTime] Reset timer");
    }


    /// <summary> Reduce the event interval by reductionAmount (clamped). </summary>
    public void ReduceInterval(float amount)
    {
        eventInterval = Mathf.Max(0.01f, eventInterval - amount);
        // recompute next interval relative to current time
        nextIntervalTime = Mathf.Max(0f, currentTime - eventInterval);
        Debug.Log($"[GlobalTime] Reduced eventInterval to {eventInterval:F2}, nextIntervalTime={nextIntervalTime:F2}");
    }

    /// <summary> Add or remove time from the current timer. Positive adds, negative subtracts. </summary>
    public void AddTime(float seconds)
    {
        currentTime = Mathf.Clamp(currentTime + seconds, 0f, Mathf.Infinity);
        nextIntervalTime = Mathf.Max(0f, currentTime - eventInterval);
        Debug.Log($"[GlobalTime] AddTime({seconds}) -> currentTime={currentTime:F2}");
    }

    /// <summary> Read-only accessor for other systems. </summary>
    public float GetCurrentTime() => currentTime;
    public float GetEventInterval() => eventInterval;
    public bool IsPaused() => isPaused;
}