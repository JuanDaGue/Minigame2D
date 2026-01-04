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

    [SerializeField] private States currentState = States.InMenu;
    public States CurrentState => currentState;


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
        SetState(States.InMenu);
        //DontDestroyOnLoad(gameObject);
    }

    public void SetState(States newState)
    {
        currentState = newState;

        switch (newState)
        {
            case States.InGame:
                Time.timeScale = 1;
                OnResume?.Invoke();
                break;

            case States.GameOver:
                OnGameOver?.Invoke();
                //Time.timeScale = 1;
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