using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip pauseClip;
    [SerializeField] private AudioClip gameOverClip;

    private void OnEnable()
    {
        GameManager.Instance.OnPause += PlayPauseSound;
        GameManager.Instance.OnGameOver += PlayGameOverSound;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPause -= PlayPauseSound;
        GameManager.Instance.OnGameOver -= PlayGameOverSound;
    }

    private void PlayPauseSound() => musicSource.PlayOneShot(pauseClip);
    private void PlayGameOverSound() => musicSource.PlayOneShot(gameOverClip);
}