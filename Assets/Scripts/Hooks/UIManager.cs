using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup pauseMenuGroup;
    [SerializeField] private CanvasGroup gameOverGroup;

    private void OnEnable()
    {
        GameManager.Instance.OnPause += ShowPauseMenu;
        GameManager.Instance.OnResume += HidePauseMenu;
        GameManager.Instance.OnGameOver += ShowGameOverScreen;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPause -= ShowPauseMenu;
        GameManager.Instance.OnResume -= HidePauseMenu;
        GameManager.Instance.OnGameOver -= ShowGameOverScreen;
    }

    private void ShowPauseMenu() => SetCanvasGroup(pauseMenuGroup, true);
    private void HidePauseMenu() => SetCanvasGroup(pauseMenuGroup, false);
    private void ShowGameOverScreen() => SetCanvasGroup(gameOverGroup, true);

    /// <summary>
    /// Helper to toggle CanvasGroup visibility and interactivity
    /// </summary>
    private void SetCanvasGroup(CanvasGroup group, bool visible)
    {
        if (group == null) return;

        group.alpha = visible ? 1 : 0;              // Controls visibility
        group.interactable = visible;               // Blocks input if hidden
        group.blocksRaycasts = visible;             // Prevents clicks passing through
    }
}