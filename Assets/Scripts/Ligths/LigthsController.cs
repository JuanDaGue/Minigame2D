using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System;
using System.Collections.Generic;

public class LigthsController : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    [Header("Light Components")]

    [SerializeField] private Light2D fireLight;
    [SerializeField] private Light2D SecondaryLight;
    //[SerializeField] private LineRenderer fireLine;
    public SpriteRenderer mirrorSprite;

    [Header("Light settings")]
    [SerializeField] private float fireLightIntensity = 15f;
    [SerializeField] private float fireLightRange = 20f;
    [SerializeField] private float fireLightSpotAngle = 45f;

    [Header("Raycast")]
    public LayerMask layerMask;

    

    //private MirrorManager mirrorManager;
    private MirrorMoveController mirrorMoveController;
    

    [Header("Tweening")]
    public float OnTimeduration = 3f;
    public Ease ease = Ease.Linear;

    // Intensity tween
    private Tween intensityTween;
    [SerializeField] private float currentIntensity = 0f;

    // Line draw tween
    private Tween lineTween;
    //private const string TargetLayerName = "Emitter";

    void Awake()
    {
        if (fireLight == null) fireLight = GetComponentInChildren<Light2D>();
        //if (fireLine == null) fireLine = GetComponentInChildren<LineRenderer>();
        if (mirrorSprite == null)
        {
            mirrorSprite = GetComponentInChildren<SpriteRenderer>();
            
        }
        mirrorSprite.DOFade(0f, Mathf.Min(0.5f, OnTimeduration * 0.5f)).SetEase(Ease.OutQuad);
        mirrorMoveController = GetComponent<MirrorMoveController>();
    }



    public void ForceActivate()
{
    if (fireLight == null || SecondaryLight == null || mirrorSprite == null) return;

    intensityTween?.Kill();

    intensityTween = DOVirtual.Float(currentIntensity, fireLightIntensity, OnTimeduration, v =>
    {
        currentIntensity = v;
        fireLight.intensity = v;
        SecondaryLight.intensity = v;
    })
    .SetEase(ease)
    .OnComplete(() => Debug.Log("Light intensity reached " + currentIntensity));

    // Fade sprite in
    mirrorSprite.DOFade(1f, OnTimeduration).SetEase(ease);

    intensityTween.Play();
}

public void ForceDeactivate()
{
    if (fireLight == null || SecondaryLight == null || mirrorSprite == null) return;

    intensityTween?.Kill();

    intensityTween = DOVirtual.Float(currentIntensity, 0f, Mathf.Min(0.5f, OnTimeduration * 0.5f), v =>
    {
        currentIntensity = v;
        fireLight.intensity = v;
        SecondaryLight.intensity = v;
    })
    .SetEase(Ease.OutQuad)
    .OnComplete(() => Debug.Log("Light turned off"));

    // Fade sprite out
    intensityTween.Play();
    mirrorSprite.DOFade(0f, Mathf.Min(0.5f, OnTimeduration * 0.5f)).SetEase(Ease.OutQuad);

}

    public void ApplyLightAttributes(float distance)
    {
        if (fireLight == null || SecondaryLight == null) return;
        // Apply animated intensity that DOTween controls
        fireLight.intensity = currentIntensity;
        SecondaryLight.intensity = currentIntensity;
        fireLight.pointLightOuterRadius = Mathf.Max(0.01f, distance);
        fireLight.pointLightInnerRadius = distance * 0.8f;
        fireLight.pointLightOuterAngle = fireLightSpotAngle;
    }

    public void IntensityController(float newIntensity)
    {
        if(newIntensity== currentIntensity || currentIntensity<=0) return;
        intensityTween?.Kill();

        intensityTween = DOVirtual.Float(currentIntensity, newIntensity, Mathf.Min(0.5f, OnTimeduration * 0.5f), v =>
        {
            currentIntensity = v;
            fireLight.intensity = v;
            SecondaryLight.intensity = v;
        })
        .SetEase(Ease.InElastic)
        .OnComplete(() => Debug.Log("Light turned off"));

        // Fade sprite out
        intensityTween.Play();
    }
    void OnDrawGizmos()
    {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, (Vector2)firePoint.position + (Vector2)firePoint.up * fireLightRange);
    }
}