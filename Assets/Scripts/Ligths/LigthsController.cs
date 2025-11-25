using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System;

public class LigthsController : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    [SerializeField] private Light2D fireLight;
    [SerializeField] private LineRenderer fireLine;
    public SpriteRenderer fireLiteSprite;

    [Header("Light settings")]
    [SerializeField] private float fireLightIntensity = 15f;
    [SerializeField] private float fireLightRange = 20f;
    [SerializeField] private float fireLightSpotAngle = 45f;

    [Header("Raycast")]
    public LayerMask layerMask;

    [HideInInspector] public bool isFireLightOn = false;

    private MirrorManager mirrorManager;
    private MirrorMoveController mirrorMoveController;

    [Header("Tweening")]
    public float OnTimeduration = 3f;
    public Ease ease = Ease.Linear;

    // Intensity tween
    private Tween intensityTween;
    private float currentIntensity = 0f;

    // Line draw tween
    private Tween lineTween;
    private const string TargetLayerName = "Emitter";

    void Awake()
    {
        if (fireLight == null) fireLight = GetComponentInChildren<Light2D>();
        if (fireLine == null) fireLine = GetComponentInChildren<LineRenderer>();
        if (fireLiteSprite == null) fireLiteSprite = GetComponentInChildren<SpriteRenderer>();
        mirrorMoveController = GetComponent<MirrorMoveController>();
    }

    void Start()
    {
        mirrorManager = FindFirstObjectByType<MirrorManager>();
        if (layerMask == 0) layerMask = LayerMask.GetMask("Default");

        // initial values
        currentIntensity = isFireLightOn ? fireLightIntensity : 0f;
        ApplyLightAttributes(0f);

        if (isFireLightOn) ForceActivate(); else ForceDeactivate();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) ToggleFireLight();
        if (isFireLightOn) ShootFireLine();
    }

    public void ToggleFireLight()
    {
        if (isFireLightOn) ForceDeactivate(); else ForceActivate();
    }

    public void ForceActivate()
    {
        if (fireLight == null) return;

        // Kill previous intensity tween
        intensityTween?.Kill();

        // Tween currentIntensity to target
        intensityTween = DOVirtual.Float(currentIntensity, fireLightIntensity, OnTimeduration, v => currentIntensity = v)
                             .SetEase(ease)
                             .OnComplete(() => Debug.Log("Light intensity reached " + currentIntensity));
        intensityTween.Play();

        isFireLightOn = true;
        if (fireLine != null) fireLine.enabled = true;
        if (fireLiteSprite != null) fireLiteSprite.enabled = true;
        if (mirrorMoveController != null) mirrorMoveController.canMoveObject = true;
        
    }

    public void ForceDeactivate()
    {
        if (fireLight == null) return;

        isFireLightOn = false;
        if (fireLine != null) fireLine.enabled = false;
        if (fireLiteSprite != null) fireLiteSprite.enabled = false;
        if (mirrorMoveController != null) mirrorMoveController.canMoveObject = false;

        // Kill previous intensity tween and tween back to zero
        intensityTween?.Kill();
        intensityTween = DOVirtual.Float(currentIntensity, 0f, Mathf.Min(0.5f, OnTimeduration * 0.5f), v => currentIntensity = v)
                             .SetEase(Ease.OutQuad)
                             .OnComplete(() => Debug.Log("Light turned off"));
        intensityTween.Play();

        // Optionally kill line draw too
        lineTween?.Kill();
    }

    void ApplyLightAttributes(float distance)
    {
        if (fireLight == null) return;

        // Apply animated intensity that DOTween controls
        fireLight.intensity = currentIntensity;

        fireLight.pointLightOuterRadius = Mathf.Max(0.01f, distance);
        fireLight.pointLightInnerRadius = distance * 0.8f;
        fireLight.pointLightOuterAngle = fireLightSpotAngle;
    }

    // Instant draw helper (kept for convenience)
    void DrawLineInstant(Vector2 startPos, Vector2 endPos)
    {
        if (fireLine == null) return;
        fireLine.positionCount = 2;
        fireLine.SetPosition(0, startPos);
        fireLine.SetPosition(1, endPos);
    }

    // Start a DOTween-driven animation of the LineRenderer endpoint from origin -> target over OnTimeduration
    void StartDrawLineTo_DOTween(Vector2 origin, Vector2 target)
    {
        if (fireLine == null) return;

        // Kill previous line tween
        lineTween?.Kill();

        fireLine.positionCount = 2;
        fireLine.SetPosition(0, origin);

        Vector2 start = origin;
        Vector2 end = target;

        // Tween t from 0->1, each update set the endpoint to Lerp(start, end, t)
        lineTween = DOVirtual.Float(0f, 1f, OnTimeduration, t =>
        {
            Vector2 p = Vector2.Lerp(start, end, t);
            fireLine.SetPosition(1, p);
        })
        .SetEase(ease)
        .OnComplete(() =>
        {
            fireLine.SetPosition(1, end);
            // Clear reference
            lineTween = null;
        });

        lineTween.Play();
    }

    void ShootFireLine()
    {
        if (mirrorMoveController != null && !mirrorMoveController.canMoveObject)
            return;

        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 dir = firePoint != null ? (Vector2)firePoint.up : (Vector2)transform.up;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, fireLightRange, layerMask);
        if (hit.collider != null)
        {
            // Animate line to hit point over OnTimeduration
            StartDrawLineTo_DOTween(origin, hit.point);

            ApplyLightAttributes(hit.distance);

            if (mirrorManager != null && hit.collider.gameObject.CompareTag("Mirror"))
            {
                mirrorManager.SetActiveMirror(hit.collider.gameObject);

                int targetLayerIndex = LayerMask.NameToLayer(TargetLayerName);
                if (targetLayerIndex == -1)
                {
                    Debug.LogError($"El Layer '{TargetLayerName}' no existe en la configuración de Tags & Layers del proyecto.");
                    return;
                }

                gameObject.layer = targetLayerIndex;
            }
        }
        else
        {
            Vector2 target = origin + dir * fireLightRange;
            StartDrawLineTo_DOTween(origin, target);
            ApplyLightAttributes(fireLightRange);
        }
    }

    void OnDrawGizmos()
    {
        if (isFireLightOn && firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, (Vector2)firePoint.position + (Vector2)firePoint.up * fireLightRange);
        }
    }
}