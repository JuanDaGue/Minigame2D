using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup pauseMenuGroup;
    [SerializeField] private CanvasGroup gameOverGroup;
    [SerializeField] private float fadeDuration = 2f;

    private Tween currentTween;

    private void Start()
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

    private void ShowPauseMenu() => FadeCanvasGroup(pauseMenuGroup, true, fadeDuration);
    private void HidePauseMenu() => FadeCanvasGroup(pauseMenuGroup, false, fadeDuration);
    private void ShowGameOverScreen() => FadeCanvasGroup(gameOverGroup, false, fadeDuration);

    /// Helper to fade CanvasGroup using DOTween
    private void FadeCanvasGroup(CanvasGroup group, bool visible, float duration)
    {
        if (group == null) return;

        // Stop any existing tween on this group
        if (currentTween != null && currentTween.IsActive()) currentTween.Kill();

        // If showing, ensure GameObject is active and start from alpha 0
        if (visible)
        {
            group.gameObject.SetActive(true);
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            currentTween = group.DOFade(1f, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    group.interactable = true;
                    group.blocksRaycasts = true;
                });
        }
        else // hiding
        {
            // Immediately prevent interaction while fading out
            group.interactable = false;
            group.blocksRaycasts = false;

            currentTween = group.DOFade(0f, duration)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    // Optionally deactivate the GameObject to save performance
                    group.gameObject.SetActive(false);
                });
        }
    }
}