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
        currentTime -= delta;
        // Clamp to zero
        if (currentTime < 0f) currentTime = 0f;

        // Check interval event(s). Use <= because we count down.
        if (currentTime <= nextIntervalTime && nextIntervalTime > 0f)
        {
            Debug.Log($"[GlobalTime] Interval reached at time {currentTime:F2}s (nextIntervalTime={nextIntervalTime:F2})");
            SafeInvokeInterval();
            // schedule next interval (avoid infinite loop if interval <= 0)
            nextIntervalTime = Mathf.Max(0f, nextIntervalTime - eventInterval);
        }

        // Timer finished
        if (currentTime <= 0f)
        {
            Debug.Log("[GlobalTime] Timer finished");
            SafeInvokeFinished();
            GameManager.Instance.SetState(GameManager.States.GameOver);
        }
    }

    private void SafeInvokeInterval()
    {
        try
        {
            OnIntervalReached?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GlobalTime] Exception in OnIntervalReached handler: {ex}");
        }
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