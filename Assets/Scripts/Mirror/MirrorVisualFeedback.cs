using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class MirrorVisualFeedback : IMirrorVisualFeedback
{
    private readonly Light2D _tapLight;
    private readonly MonoBehaviour _coroutineRunner;
    private Coroutine _lightCoroutine;
    
    public MirrorVisualFeedback(Light2D tapLight, MonoBehaviour coroutineRunner)
    {
        _tapLight = tapLight;
        _coroutineRunner = coroutineRunner;
    }
    
    public void ToggleLight()
    {
        if (_tapLight == null) return;
        
        float targetIntensity = (_tapLight.intensity < 0.5f) ? 1.0f : 0.0f;
        
        if (_lightCoroutine != null)
        {
            _coroutineRunner.StopCoroutine(_lightCoroutine);
        }
        
        _lightCoroutine = _coroutineRunner.StartCoroutine(
            AnimateLightIntensity(_tapLight.intensity, targetIntensity, 0.2f));
    }
    
    public void FlashLight(float duration = 0.3f, float multiplier = 1.5f)
    {
        if (_tapLight == null) return;
        
        float originalIntensity = _tapLight.intensity;
        float flashIntensity = originalIntensity * multiplier;
        
        if (_lightCoroutine != null)
        {
            _coroutineRunner.StopCoroutine(_lightCoroutine);
        }
        
        _lightCoroutine = _coroutineRunner.StartCoroutine(
            FlashLightCoroutine(originalIntensity, flashIntensity, duration));
    }
    
    private IEnumerator AnimateLightIntensity(float startIntensity, float targetIntensity, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            _tapLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }
        
        _tapLight.intensity = targetIntensity;
        _lightCoroutine = null;
    }
    
    private IEnumerator FlashLightCoroutine(float originalIntensity, float flashIntensity, float duration)
    {
        // Flash up
        float elapsedTime = 0f;
        while (elapsedTime < duration * 0.3f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (duration * 0.3f);
            _tapLight.intensity = Mathf.Lerp(originalIntensity, flashIntensity, t);
            yield return null;
        }
        
        // Flash down
        elapsedTime = 0f;
        while (elapsedTime < duration * 0.7f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (duration * 0.7f);
            _tapLight.intensity = Mathf.Lerp(flashIntensity, originalIntensity, t);
            yield return null;
        }
        
        _tapLight.intensity = originalIntensity;
        _lightCoroutine = null;
    }
}