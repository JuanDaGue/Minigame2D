using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class MirrorVisualEffects : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Movement Settings")]
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private Vector3 resetPosition = new Vector3(-10f, -10f, 0f);
    
    [Header("References")]
    [SerializeField] private Light2D mirrorLight;
    [SerializeField] private SpriteRenderer mirrorSprite;
    [SerializeField] private ParticleSystem resetParticles;
    [SerializeField] private AudioSource resetSound;
    
    private MirrorMoveController _mirrorController;
    private LigthsController _lightsController;
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Color _originalSpriteColor;
    
    private void Awake()
    {
        _mirrorController = GetComponent<MirrorMoveController>();
        _lightsController = GetComponent<LigthsController>();
        
        if (mirrorSprite != null)
        {
            _originalSpriteColor = mirrorSprite.color;
        }
        
        _originalPosition = transform.position;
        _originalScale = transform.localScale;
    }
    
    public void StartFadeOut()
    {
        StartCoroutine(FadeOutSequence());
    }
    
    public void StartFadeIn()
    {
        StartCoroutine(FadeInSequence());
    }
    
    private IEnumerator FadeOutSequence()
    {
        Debug.Log($"[MirrorVisualEffects] Starting fade out for {gameObject.name}");
        
        // 1. Play particles
        if (resetParticles != null)
        {
            resetParticles.Play();
        }
        
        // 2. Play sound
        if (resetSound != null)
        {
            resetSound.Play();
        }
        
        // 3. Fade light
        yield return FadeLight(1f, 0f, fadeDuration);
        
        // 4. Fade sprite
        yield return FadeSprite(1f, 0f, fadeDuration * 0.5f);
        
        // 5. Move to reset position
        yield return MoveToPosition(resetPosition, moveDuration);
        
        // 6. Shrink
        yield return ScaleObject(_originalScale, Vector3.zero, fadeDuration * 0.3f);
        
        Debug.Log($"[MirrorVisualEffects] Fade out complete for {gameObject.name}");
    }
    
    private IEnumerator FadeInSequence()
    {
        Debug.Log($"[MirrorVisualEffects] Starting fade in for {gameObject.name}");
        
        // 1. Reset position and scale
        transform.position = _originalPosition;
        transform.localScale = Vector3.zero;
        
        // 2. Grow
        yield return ScaleObject(Vector3.zero, _originalScale, fadeDuration * 0.3f);
        
        // 3. Fade in sprite
        yield return FadeSprite(0f, 1f, fadeDuration * 0.5f);
        
        // 4. Fade in light
        yield return FadeLight(0f, 1f, fadeDuration);
        
        Debug.Log($"[MirrorVisualEffects] Fade in complete for {gameObject.name}");
    }
    
    private IEnumerator FadeLight(float startIntensity, float endIntensity, float duration)
    {
        if (mirrorLight == null && _lightsController == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsedTime / duration);
            float intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            
            if (mirrorLight != null)
                mirrorLight.intensity = intensity;
            
            if (_lightsController != null)
                _lightsController.SetIntensity(intensity);
            
            yield return null;
        }
        
        // Ensure final values
        if (mirrorLight != null)
            mirrorLight.intensity = endIntensity;
        
        if (_lightsController != null)
            _lightsController.SetIntensity(endIntensity);
    }
    
    private IEnumerator FadeSprite(float startAlpha, float endAlpha, float duration)
    {
        if (mirrorSprite == null) yield break;
        
        float elapsedTime = 0f;
        Color startColor = _originalSpriteColor;
        startColor.a = startAlpha;
        Color endColor = _originalSpriteColor;
        endColor.a = endAlpha;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsedTime / duration);
            mirrorSprite.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        mirrorSprite.color = endColor;
    }
    
    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsedTime / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        transform.position = targetPosition;
    }
    
    private IEnumerator ScaleObject(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsedTime / duration);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        
        transform.localScale = endScale;
    }
    
    // Public method to reset visual state
    public void ResetVisuals()
    {
        if (mirrorLight != null)
            mirrorLight.intensity = 1f;
        
        if (mirrorSprite != null)
            mirrorSprite.color = _originalSpriteColor;
        
        transform.position = _originalPosition;
        transform.localScale = _originalScale;
        
        if (_lightsController != null)
            _lightsController.SetIntensity(1f);
    }
}