using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal; // Light2D

public class IGems : MonoBehaviour, IInteractivesObject
{
    [SerializeField] private GlobalTime globalTime;
    [SerializeField] private float pickupDuration = 1.6f;
    [SerializeField] private float lightOnIntensity = 2f;
    [SerializeField] private float lightFadeDuration = 1.4f;

    public Gems gemType = Gems.diamante;
    public Gems GemType => gemType;

    private SpriteRenderer sr;
    private Collider2D col;
    private Light2D light2D;
    private Transform player;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        light2D = GetComponent<Light2D>();
    }

    void Start()
    {
        // Cache player for performance
        var p = GameObject.FindGameObjectWithTag("Mirror");
        player = p != null ? p.transform : null;

        // If you want a random type at spawn:
        gemType = (Gems)Random.Range(0, System.Enum.GetValues(typeof(Gems)).Length);

        // Apply visuals for the chosen type
        ApplyTypeVisuals();
    }

    public bool CanInteract() => true;

    public void OnInteract()
    {
        Debug.Log("[IGems] Gem Activated: " + gemType);

        // Apply effect (time)
        float timeToAdd = GetTimeForType(gemType);
        if (globalTime != null) globalTime.AddTime(timeToAdd);

        // Disable further interactions
        if (col != null) col.enabled = false;

        // Animate light: pulse up then fade out
        if (light2D != null)
        {
            // Ensure starting intensity is 0 so it "turns on"
            light2D.intensity = 0f;
            Sequence lightSeq = DOTween.Sequence();
            lightSeq.Append(DOTween.To(() => light2D.intensity, x => light2D.intensity = x, lightOnIntensity, lightFadeDuration).SetEase(Ease.OutQuad));
            lightSeq.Append(DOTween.To(() => light2D.intensity, x => light2D.intensity = x, 0f, lightFadeDuration).SetEase(Ease.InQuad));
        }

        // Move + scale + fade sprite to player
        Vector3 targetPos = player != null ? player.position : transform.position;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(targetPos, pickupDuration).SetEase(Ease.InQuad));
        seq.Join(transform.DOScale(Vector3.zero, pickupDuration));
        if (sr != null) seq.Join(sr.DOFade(0f, pickupDuration));
        seq.OnComplete(() => Destroy(gameObject));
    }

    private void ApplyTypeVisuals()
    {
        Color c = Color.white;
        switch (gemType)
        {
            case Gems.diamante: c = new Color(0.8f, 0.9f, 1f); break;
            case Gems.rubí:     c = new Color(1f, 0.2f, 0.3f); break;
            case Gems.zafiro:   c = new Color(0.2f, 0.5f, 1f); break;
            case Gems.esmeralda:c = new Color(0.2f, 1f, 0.4f); break;
        }

        if (sr != null) sr.color = c;
        if (light2D != null) light2D.color = c;
    }

    private float GetTimeForType(Gems type)
    {
        switch (type)
        {
            case Gems.diamante: return 20f;
            case Gems.rubí:     return 15f;
            case Gems.zafiro:   return 10f;
            case Gems.esmeralda:return 5f;
            default: return 0f;
        }
    }
}