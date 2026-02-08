using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MirrorCleanupSystem : IMirrorCleanupSystem
{
    private readonly MirrorCollection _mirrorCollection;
    private readonly MirrorSubscriptionManager _subscriptionManager;
    private readonly ILinePointUpdater _linePointUpdater;
    private readonly MonoBehaviour _coroutineRunner;
    
    public MirrorCleanupSystem(
        MirrorCollection mirrorCollection,
        MirrorSubscriptionManager subscriptionManager,
        ILinePointUpdater linePointUpdater,
        MonoBehaviour coroutineRunner)
    {
        _mirrorCollection = mirrorCollection;
        _subscriptionManager = subscriptionManager;
        _linePointUpdater = linePointUpdater;
        _coroutineRunner = coroutineRunner;
    }
    
    public void CleanMirrorsAfterTap(MirrorMoveController tappedMirror)
    {
        if (tappedMirror == null) return;
        
        Debug.Log($"[MirrorCleanupSystem] Cleaning mirrors after tapped: {tappedMirror.name}");
        
        int tappedIndex = _mirrorCollection.IndexOf(tappedMirror);
        if (tappedIndex < 0) 
        {
            Debug.LogWarning($"[MirrorCleanupSystem] Tapped mirror not found in collection");
            return;
        }
        
        // Get mirrors to remove (all AFTER tapped index)
        var mirrorsToReset = new List<MirrorMoveController>();
        for (int i = _mirrorCollection.Mirrors.Count - 1; i > tappedIndex; i--)
        {
            if (i < _mirrorCollection.Mirrors.Count)
            {
                mirrorsToReset.Add(_mirrorCollection.Mirrors[i]);
            }
        }
        
        Debug.Log($"[MirrorCleanupSystem] Resetting {mirrorsToReset.Count} mirrors after index {tappedIndex}");
        
        // Reset each mirror with effects
        foreach (var mirror in mirrorsToReset)
        {
            StartResetMirrorCoroutine(mirror, tappedIndex);
        }
        
        Debug.Log($"[MirrorCleanupSystem] Cleanup initiated. {tappedMirror.name} will become Active");
    }
    
    private void StartResetMirrorCoroutine(MirrorMoveController mirror, int tappedIndex)
    {
        // Remove from collection immediately
        _mirrorCollection.Mirrors.Remove(mirror);
        
        // Start reset coroutine
        _coroutineRunner.StartCoroutine(ResetMirrorWithEffects(mirror, tappedIndex));
    }
    
    private IEnumerator ResetMirrorWithEffects(MirrorMoveController mirror, int originalIndex)
    {
        if (mirror == null) yield break;
        
        Debug.Log($"[MirrorCleanupSystem] Starting reset sequence for {mirror.name}");
        
        // 1. Play fade out effect on light
        yield return FadeOutMirrorLight(mirror, 0.5f);
        
        // 2. Unsubscribe from events
        _subscriptionManager.UnsubscribeMirror(mirror);
        
        // 3. Unsubscribe from line points
        _linePointUpdater?.UnsubscribeMirrorFromLinePoints(mirror.transform);
        
        // 4. Change mirror state to Deactive
        mirror.SwitchMirrorState(MirrorState.Deactive);
        
        // 5. Optional: Play particle effect or animation
        PlayResetEffects(mirror);
        
        // 6. Move mirror to reset position or disable it
        //yield return MoveMirrorToResetPosition(mirror, 0.3f);
        
        // 7. Finally disable the mirror
        //mirror.gameObject.SetActive(false);
        
        Debug.Log($"[MirrorCleanupSystem] Reset complete for {mirror.name}");
    }
    
    private IEnumerator FadeOutMirrorLight(MirrorMoveController mirror, float duration)
    {
        var lightController = mirror.GetComponent<LigthsController>();
        var tapLight = mirror.GetComponentInChildren<Light2D>();
        
        if (lightController != null || tapLight != null)
        {
            float elapsedTime = 0f;
            float startIntensity = tapLight?.intensity ?? 1f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                // Smooth fade out
                float intensity = Mathf.Lerp(startIntensity, 0f, t);
                
                if (tapLight != null)
                    tapLight.intensity = intensity;
                
                // Also fade main light if exists
                if (lightController != null)
                {
                    // Assuming LigthsController has an intensity property
                    // You might need to adjust this based on your actual implementation
                    lightController.SetIntensity(intensity);
                }
                
                yield return null;
            }
            
            // Ensure lights are off
            if (tapLight != null)
                tapLight.intensity = 0f;
                
            if (lightController != null)
                lightController.SetIntensity(0f);
        }
    }
    
    private IEnumerator MoveMirrorToResetPosition(MirrorMoveController mirror, float duration)
    {
        Vector3 originalPosition = mirror.transform.position;
        Vector3 targetPosition = GetResetPosition();
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // Smooth movement with ease out
            t = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease out
            mirror.transform.position = Vector3.Lerp(originalPosition, targetPosition, t);
            
            yield return null;
        }
        
        mirror.transform.position = targetPosition;
    }
    
    private Vector3 GetResetPosition()
    {
        // Define where reset mirrors go (off-screen or pool position)
        // You can customize this based on your game
        return new Vector3(-10f, -10f, 0f); // Example: off-screen left-bottom
    }
    
    private void PlayResetEffects(MirrorMoveController mirror)
    {
        // Play particle effect
        var particles = mirror.GetComponentInChildren<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }
        
        // Play sound effect
        var audioSource = mirror.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
        
        // Shrink animation
        _coroutineRunner.StartCoroutine(ShrinkMirror(mirror, 0.2f));
    }
    
    private IEnumerator ShrinkMirror(MirrorMoveController mirror, float duration)
    {
        Vector3 originalScale = mirror.transform.localScale;
        Vector3 targetScale = Vector3.zero;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            mirror.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        mirror.transform.localScale = targetScale;
    }
    
    public void RemoveMirror(MirrorMoveController mirror)
    {
        // Use the reset coroutine instead of immediate removal
        StartResetMirrorCoroutine(mirror, -1);
    }
    
    public void ResetMirrorForMovement(MirrorMoveController mirror)
    {
        if (mirror == null) return;
        
        // Reset mirror to Active state with effects
        _coroutineRunner.StartCoroutine(ResetToActiveWithEffects(mirror));
    }
    
    private IEnumerator ResetToActiveWithEffects(MirrorMoveController mirror)
    {
        // 1. Enable the mirror
        mirror.gameObject.SetActive(true);
        
        // 2. Reset scale
        mirror.transform.localScale = Vector3.one;
        
        // 3. Fade in light
        yield return FadeInMirrorLight(mirror, 0.5f);
        
        // 4. Change state to Active
        mirror.SwitchMirrorState(MirrorState.Active);
        
        // 5. Ensure inputs are subscribed
        mirror.OnInputsSubscribe();
        
        Debug.Log($"[MirrorCleanupSystem] Reset mirror for movement: {mirror.name}");
    }
    
    private IEnumerator FadeInMirrorLight(MirrorMoveController mirror, float duration)
    {
        var lightController = mirror.GetComponent<LigthsController>();
        var tapLight = mirror.GetComponentInChildren<Light2D>();
        
        if (lightController != null || tapLight != null)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                // Smooth fade in
                float intensity = Mathf.Lerp(0f, 1f, t);
                
                if (tapLight != null)
                    tapLight.intensity = intensity;
                
                if (lightController != null)
                    lightController.SetIntensity(intensity);
                
                yield return null;
            }
            
            // Ensure lights are full brightness
            if (tapLight != null)
                tapLight.intensity = 1f;
                
            if (lightController != null)
                lightController.SetIntensity(1f);
        }
    }
}