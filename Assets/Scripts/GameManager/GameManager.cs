using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum States
    {
        InGame,
        GameOver,
        InMenu,
        Pause
    }

    public States CurrentState { get; private set; }

    // 🔔 Events for other systems to subscribe to
    public event Action<States> OnStateChanged;
    public event Action OnGameOver;
    public event Action OnPause;
    public event Action OnResume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(States newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case States.InGame:
                Time.timeScale = 1;
                OnResume?.Invoke();
                break;

            case States.GameOver:
                Time.timeScale = 0;
                OnGameOver?.Invoke();
                break;

            case States.InMenu:
                Time.timeScale = 0;
                break;

            case States.Pause:
                Time.timeScale = 0;
                OnPause?.Invoke();
                break;
        }

        Debug.Log($"Game state changed to: {newState}");
        OnStateChanged?.Invoke(newState);
    }
}