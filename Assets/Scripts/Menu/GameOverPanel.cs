using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    private Coroutine restartCoroutine;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += RestartGame;
            Debug.Log("[GameOverPanel] Subscribed to OnGameOver event");
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("[GameOverPanel] OnDisable unsubscribing from OnGameOver");
            GameManager.Instance.OnGameOver -= RestartGame;
        }

        // Ensure coroutine is stopped when object is disabled
        if (restartCoroutine != null)
        {
            StopCoroutine(restartCoroutine);
            restartCoroutine = null;
        }
    }

    private void RestartGame()
    {
        // If a restart is already scheduled, ignore or restart the timer:
        Debug.Log("[GameOverPanel] RestartGame called");
        if (restartCoroutine != null)
        {
            // Option A: ignore duplicate calls
            return;

            // Option B: restart timer instead
            // StopCoroutine(restartCoroutine);
            // restartCoroutine = StartCoroutine(RestartCoroutine());
        }

        restartCoroutine = StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        yield return new WaitForSecondsRealtime(3f);

        // Use LoadSceneAsync to avoid a frame hitch on large scenes
        var asyncOp = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        // If you want to block until load completes, uncomment the next line:
        // while (!asyncOp.isDone) yield return null;

        restartCoroutine = null;
    }
}