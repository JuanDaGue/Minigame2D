using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class IRotateObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour switchObjectBehaviour; // assign an object that implements ISwitch (Inspector friendly)
    private ISwitch SwitchObject => switchObjectBehaviour as ISwitch;

    [SerializeField] private Transform pivot; // optional pivot; if null, uses this.transform
    [Header("Rotation")]
    [SerializeField] private float rotationAngle = 90f;      // degrees per activation
    [SerializeField] private float rotationDuration = 0.8f;  // base duration
    [SerializeField] private Ease rotationEase = Ease.InQuad; // accelerating feel

    [Header("Blinking Light")]
    [SerializeField] private Light2D associatedLight;
    [SerializeField] private float lightMinIntensity = 0f;
    [SerializeField] private float lightMaxIntensity = 1.5f;
    [SerializeField] private float blinkOnDuration = 0.15f;
    [SerializeField] private float blinkOffDuration = 0.35f;
    [SerializeField] private int blinkRepeat = -1; // -1 = infinite

    [Header("Blinking Line")]
    [SerializeField] private LineRenderer associatedLine;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float lineMinAlpha = 0f;
    [SerializeField] private float lineMaxAlpha = 1f;

    // Internal
    private Tween rotationTween;
    private Sequence blinkSequence;
    private bool isRotating = false;

    private void Start()
    {
        if (pivot == null) pivot = transform;

        // If user assigned a MonoBehaviour that implements ISwitch, subscribe
        if (SwitchObject != null)
        {
            SwitchObject.RotateMirrorEvent += RotateObject;
            Debug.Log($"[IRotateObject] Subscribed to RotateMirrorEvent on {SwitchObject}");
        }
        else if (switchObjectBehaviour != null)
        {
            Debug.LogWarning("[IRotateObject] Assigned switchObjectBehaviour does not implement ISwitch");
        }
        else
        {
            // Try to find an ISwitch in scene (optional)
            ISwitch found = FindFirstObjectByType<MonoBehaviour>() as ISwitch;
            if (found != null)
            {
                found.RotateMirrorEvent += RotateObject;
                Debug.Log("[IRotateObject] Found ISwitch in scene and subscribed");
            }
            else
            {
                Debug.Log("[IRotateObject] No ISwitch assigned or found in scene");
            }
        }

        // Prepare blink sequence but don't start it yet
        PrepareBlinkSequence();
    }

    private void OnDestroy()
    {
        // Unsubscribe safely
        if (SwitchObject != null)
        {
            SwitchObject.RotateMirrorEvent -= RotateObject;
            Debug.Log("[IRotateObject] Unsubscribed from RotateMirrorEvent");
        }

        // Kill tweens to avoid leaks
        rotationTween?.Kill();
        blinkSequence?.Kill();
    }

    /// <summary>
    /// Called by the switch event. Starts rotation tween and triggers blink.
    /// </summary>
    public void RotateObject()
    {
        Debug.Log($"[IRotateObject] RotateObject called on {name}");

        if (pivot == null)
        {
            Debug.LogWarning("[IRotateObject] pivot is null, aborting rotation");
            return;
        }

        // Prevent overlapping rotations
        if (rotationTween != null && rotationTween.IsActive() && rotationTween.IsPlaying())
        {
            Debug.Log("[IRotateObject] Rotation already in progress, skipping new rotation");
            return;
        }

        // Compute target rotation (local Z)
        Vector3 startEuler = pivot.localEulerAngles;
        float startZ = startEuler.z;
        float targetZ = startZ + rotationAngle;

        // Create tween with accelerating ease
        rotationTween = pivot.DOLocalRotate(new Vector3(startEuler.x, startEuler.y, targetZ),
                                            rotationDuration)
                              .SetEase(rotationEase)
                              .OnStart(() =>
                              {
                                  isRotating = true;
                                  Debug.Log("[IRotateObject] Rotation started");
                                  // Start blink when rotation starts
                                  if (blinkSequence != null && !blinkSequence.IsPlaying())
                                      blinkSequence.Restart();
                              })
                              .OnUpdate(() =>
                              {
                                  // Optional: you can add per-frame logic here (e.g., update line)
                              })
                              .OnComplete(() =>
                              {
                                  isRotating = false;
                                  Debug.Log("[IRotateObject] Rotation complete");
                                  // Optionally stop blinking after a short delay
                                  if (blinkSequence != null && blinkRepeat < 0)
                                  {
                                      // keep blinking if infinite; otherwise stop
                                  }
                              });

        // If you want the rotation to accelerate over time more strongly,
        // you can chain a second tween that shortens duration or increases ease.
        // The chosen Ease.InQuad already gives an accelerating feel.
    }

    /// <summary>
    /// Prepares a looping blink sequence for Light2D and LineRenderer.
    /// </summary>
    private void PrepareBlinkSequence()
    {
        // Kill previous
        blinkSequence?.Kill();

        blinkSequence = DOTween.Sequence();
        blinkSequence.SetAutoKill(false);
        blinkSequence.Pause();

        // Light blink: animate intensity up then down
        if (associatedLight != null)
        {
            // Ensure starting intensity
            associatedLight.intensity = lightMinIntensity;

            // Tween to max
            Tween lightOn = DOTween.To(() => associatedLight.intensity,
                                       x => associatedLight.intensity = x,
                                       lightMaxIntensity,
                                       blinkOnDuration)
                                   .SetEase(Ease.OutQuad);

            // Tween back to min
            Tween lightOff = DOTween.To(() => associatedLight.intensity,
                                        x => associatedLight.intensity = x,
                                        lightMinIntensity,
                                        blinkOffDuration)
                                    .SetEase(Ease.InQuad);

            blinkSequence.Append(lightOn);
            blinkSequence.Append(lightOff);
        }

        // Line blink: animate alpha of line color
        if (associatedLine != null)
        {
            // Ensure line has initial color
            lineColor = associatedLine.startColor is Color c ? c : lineColor;

            // Helper to set alpha
            void SetLineAlpha(float a)
            {
                Color c = lineColor;
                c.a = a;
                associatedLine.startColor = c;
                associatedLine.endColor = c;
            }

            // Start with min alpha
            SetLineAlpha(lineMinAlpha);

            Tween lineOn = DOTween.To(() => lineMinAlpha,
                                      x => SetLineAlpha(x),
                                      lineMaxAlpha,
                                      blinkOnDuration)
                                 .SetEase(Ease.OutQuad);

            Tween lineOff = DOTween.To(() => lineMaxAlpha,
                                       x => SetLineAlpha(x),
                                       lineMinAlpha,
                                       blinkOffDuration)
                                  .SetEase(Ease.InQuad);

            // If both light and line exist, append them to run in parallel with Join
            if (associatedLight != null)
            {
                // lightOn already appended; join lineOn to the same time
                blinkSequence.Join(lineOn);
                blinkSequence.Append(lineOff);
            }
            else
            {
                // No light: just use line sequence
                blinkSequence.Append(lineOn);
                blinkSequence.Append(lineOff);
            }
        }

        // Looping
        if (blinkRepeat < 0)
            blinkSequence.SetLoops(-1, LoopType.Restart);
        else if (blinkRepeat > 0)
            blinkSequence.SetLoops(blinkRepeat, LoopType.Restart);

        Debug.Log("[IRotateObject] Blink sequence prepared");
    }

    // Optional public controls
    public void StartBlinking() => blinkSequence?.Play();
    public void StopBlinking() => blinkSequence?.Pause();
    public void KillBlinking() => blinkSequence?.Kill();

    // Optional: call this to trigger rotation manually (e.g., from inspector)
    [ContextMenu("Trigger Rotate")]
    private void TriggerRotate() => RotateObject();
}