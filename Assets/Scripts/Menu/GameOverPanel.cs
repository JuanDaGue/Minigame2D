using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOverPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnGameOver += RestartGame;
    }

    private void RestartGame()
    {
        StartCoroutine(RestartCoroutine());
    }

    IEnumerator RestartCoroutine()
    {
        yield return new WaitForSecondsRealtime(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDestroy()
    {
        GameManager.Instance.OnGameOver -= RestartGame;
    }
    void OnDisable()
    {
        GameManager.Instance.OnGameOver -= RestartGame;
    }


}
