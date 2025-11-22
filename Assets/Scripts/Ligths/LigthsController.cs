using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

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

    // Keep a reference to the intensity tween so we can control it later
    private Tween intensityTween;

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
        isFireLightOn = true;
        if (fireLine != null) fireLine.enabled = true;
        if (fireLiteSprite != null) fireLiteSprite.enabled = true;
        if (mirrorMoveController != null) mirrorMoveController.canMoveObject = true;

        if (fireLight == null) return;

        // Kill previous tween safely
        intensityTween?.Kill();

        // Optional: ensure starting intensity (you can keep current if desired)
        // fireLight.intensity = 0f;

        // Create and cache the tween
        intensityTween = DOTween.To(() => fireLight.intensity,
                                   x => fireLight.intensity = x,
                                   fireLightIntensity,
                                   OnTimeduration)
                               .SetEase(ease)
                               .OnComplete(() => Debug.Log("Light intensity reached " + fireLight.intensity));
    }

    public void ForceDeactivate()
    {
        isFireLightOn = false;
        if (fireLine != null) fireLine.enabled = false;
        if (fireLiteSprite != null) fireLiteSprite.enabled = false;
        if (mirrorMoveController != null) mirrorMoveController.canMoveObject = false;

        if (fireLight == null) return;

        // If there's an active tween, kill it and tween back to zero smoothly
        intensityTween?.Kill();

        // Tween intensity back to 0 (cache if you need further control)
        intensityTween = DOTween.To(() => fireLight.intensity,
                                   x => fireLight.intensity = x,
                                   0f,
                                   Mathf.Min(0.5f, OnTimeduration * 0.5f))
                               .SetEase(Ease.OutQuad)
                               .OnComplete(() => Debug.Log("Light turned off"));
    }

    void ApplyLightAttributes(float distance)
    {
        if (fireLight == null) return;
        fireLight.intensity = isFireLightOn ? fireLightIntensity : 0f;
        fireLight.pointLightOuterRadius = Mathf.Max(0.01f, distance);
        fireLight.pointLightInnerRadius = distance * 0.8f;
        fireLight.pointLightOuterAngle = fireLightSpotAngle;
    }

    void DrawLine(Vector2 startPos, Vector2 endPos)
    {
        if (fireLine == null) return;
        fireLine.positionCount = 2;
        fireLine.SetPosition(0, startPos);
        fireLine.SetPosition(1, endPos);
    }

    void ShootFireLine()
    {
        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 dir = firePoint != null ? (Vector2)firePoint.up : (Vector2)transform.up;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, fireLightRange, layerMask);
        if (hit.collider != null)
        {
            DrawLine(origin, hit.point);
            ApplyLightAttributes(hit.distance);
            if (mirrorManager != null && hit.collider.gameObject.CompareTag("Mirror"))
            {
                mirrorManager.SetActiveMirror(hit.collider.gameObject);
            }
        }
        else
        {
            DrawLine(origin, origin + dir * fireLightRange);
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