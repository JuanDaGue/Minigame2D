using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LigthsController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform firePoint;
    [SerializeField] private Light2D fireLight;
    [Range(0.0f, 30.0f)]
    [Tooltip("The Intensity of the fire light")]
    [SerializeField] private float fireLightIntensity = 15.0f;
    [Range(0.0f, 50.0f)]
    [Tooltip("The range of the fire light")]
    [SerializeField] private float fireLightRange =20.0f;
    [Range(0.0f, 180.0f)]
    [Tooltip("The angle of the fire light")]
    [SerializeField]  private float fireLightSpotAngle = 45.0f;
    public bool isFireLightOn = false;
    [SerializeField] private LineRenderer fireLine;
    public LayerMask layerMask;
    private MirrorMoveController mirrorMoveController;
    public SpriteRenderer fireLiteSprite;

 

    void Start()
    {
        // fireLight = GetComponent<Light2D>();
        // fireLine = GetComponent<LineRenderer>();
        // LayerMask is a struct (int) and can't be null; treat 0 as unset and assign the Default layer mask.
        fireLiteSprite = GetComponentInChildren<SpriteRenderer>();
        mirrorMoveController = GetComponent<MirrorMoveController>();
        if (layerMask == 0)
        {
            layerMask = LayerMask.GetMask("Default");
        }
//
        Set2DLigthsAttributes(fireLightRange); 
        if(isFireLightOn)
        {
            ActivateFireLight();
        }
        else
        {
            DeactivateFireLight();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isFireLightOn)
            {
                DeactivateFireLight();
            }
            else
            {
                ActivateFireLight();

            }
        }
        if (isFireLightOn)
        {
            ShootFireLine();
        }
    }
        void ActivateFireLight()
        {
            isFireLightOn = true;
            //GetComponent<Light2D>().enabled = true;
            fireLine.enabled = true;
            fireLight.intensity = fireLightIntensity;
            fireLiteSprite.enabled = true;

            //fireLiteSprite.gameObject.layer = LayerMask.NameToLayer("Mirror");

        }
        void DeactivateFireLight()
        {
            isFireLightOn = false;
            //GetComponent<Light2D>().enabled = false;
            fireLine.enabled = false;      
            fireLight.intensity = 0; 
            fireLiteSprite.enabled = false;

            //fireLiteSprite.gameObject.layer = LayerMask.NameToLayer("Default");


        }
    void DrawLine(Vector2 starpos, Vector2 endPos)
    {
        fireLine.SetPosition(0, starpos);
        fireLine.SetPosition(1, endPos);
        
    }
    void ShootFireLine()
    {
        //Debug.Log("initial position " + (Vector2)firePoint.position);
        RaycastHit2D hit = Physics2D.Raycast((Vector2)firePoint.position, (Vector2)firePoint.up, fireLightRange, layerMask.value);
        if (hit.collider != null)
        {
            Debug.Log("Hit " + hit.collider.name);
            DrawLine((Vector2)firePoint.position, hit.point);
            
            Set2DLigthsAttributes(hit.distance);
            if (hit.collider.name == "Mirror"){
                ActivateMirror(hit);
            }
        }
        else
        {
            Debug.Log("Missed");
            DrawLine((Vector2)firePoint.position, (Vector2)firePoint.position + (Vector2)firePoint.up * fireLightRange);
        }
    }
    void Set2DLigthsAttributes(float distance)
    {
        fireLight.intensity = fireLightIntensity;
        fireLight.pointLightOuterRadius = distance;
        fireLight.pointLightInnerRadius = distance * 0.8f;
        fireLight.pointLightOuterAngle = fireLightSpotAngle;

    }
    void OnDrawGizmos()
    {
        if (isFireLightOn)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, (Vector2)firePoint.position + (Vector2)firePoint.up * fireLightRange);
        }
    }
    void ActivateMirror(RaycastHit2D hit)
    {
        
        
        if ( mirrorMoveController != null)
        {
            mirrorMoveController.canMoveObject = false;
        }
        GameObject nextMirror = hit.collider.gameObject;
        nextMirror.GetComponent<MirrorMoveController>().canMoveObject = true;
        nextMirror.GetComponent<LigthsController>().isFireLightOn = true;
        nextMirror.GetComponent<LigthsController>().ActivateFireLight();
    }
}

// using UnityEngine;
// using UnityEngine.Rendering.Universal;
// using DG.Tweening;

// [RequireComponent(typeof(Light2D))]
// public class LightsController : MonoBehaviour
// {
//     [Header("References")]
//     public Transform firePoint;
//     [SerializeField] private Light2D fireLight;
//     [SerializeField] private LineRenderer fireLine;
//     [SerializeField] private MirrorMoveController mirrorMoveController;
//     public SpriteRenderer fireLiteSprite;

//     [Header("Light settings")]
//     [Range(0f, 30f)] public float fireLightIntensity = 15f;
//     [Range(0f, 50f)] public float fireLightRange = 20f;
//     [Range(0f, 180f)] public float fireLightSpotAngle = 45f;

//     [Header("Tweens")]
//     public float turnOnDuration = 0.35f;
//     public float turnOffDuration = 0.25f;
//     public Ease onEase = Ease.OutQuad;
//     public Ease offEase = Ease.InQuad;
//     // Parpadeo mucho más lento
//     public float flickerDuration = 3.2f;
//     public float flickerStrength = 0.08f;

//     [Header("Range offset")]
//     // Offset para que el radio visual sea mayor que la distancia de hit
//     public float rangeOffset = 0.3f;

//     [Header("Runtime")]
//     public bool isFireLightOn = false;
//     public LayerMask layerMask;

//     // Internal
//     private Sequence currentSequence;
//     private Material lineMaterialInstance;
//     private Color lineStartColor;
//     private Color lineEndColor;
//     private Tween continuousFlickerTween;

//     void Awake()
//     {
//         if (fireLight == null) fireLight = GetComponent<Light2D>();
//         if (fireLine == null) fireLine = GetComponentInChildren<LineRenderer>();
//         if (fireLiteSprite == null) fireLiteSprite = GetComponentInChildren<SpriteRenderer>();
//         if (mirrorMoveController == null) mirrorMoveController = GetComponent<MirrorMoveController>();

//         if (layerMask == 0) layerMask = LayerMask.GetMask("Default");

//         if (fireLine != null && fireLine.sharedMaterial != null)
//         {
//             lineMaterialInstance = new Material(fireLine.sharedMaterial);
//             fireLine.material = lineMaterialInstance;
//             lineStartColor = fireLine.startColor;
//             lineEndColor = fireLine.endColor;
//         }

//         ApplyImmediateOff();
//     }

//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.F))
//         {
//             ToggleLight();
//         }

//         if (isFireLightOn)
//         {
//             ShootFireLine();
//         }
//     }

//     public void ToggleLight()
//     {
//         if (isFireLightOn) DeactivateFireLight();
//         else ActivateFireLight();
//     }

//     public void ActivateFireLight()
//     {
//         isFireLightOn = true;
//         currentSequence?.Kill();

//         // stop any continuous flicker before creating new sequence
//         continuousFlickerTween?.Kill();

//         currentSequence = DOTween.Sequence();

//         // Light intensity tween
//         currentSequence.Join(DOTween.To(() => fireLight.intensity, v => fireLight.intensity = v, fireLightIntensity, turnOnDuration).SetEase(onEase));
//         // Radius tween (outer radius)
//         currentSequence.Join(DOTween.To(() => fireLight.pointLightOuterRadius, v => fireLight.pointLightOuterRadius = v, fireLightRange, turnOnDuration).SetEase(onEase));
//         // Inner radius follow proportionally
//         currentSequence.Join(DOTween.To(() => fireLight.pointLightInnerRadius, v => fireLight.pointLightInnerRadius = v, fireLightRange * 0.8f, turnOnDuration).SetEase(onEase));

//         // Show line and fade in its alpha if material exists
//         if (fireLine != null)
//         {
//             fireLine.enabled = true;
//             if (lineMaterialInstance != null)
//             {
//                 float from = 0f;
//                 float to = 1f;
//                 lineMaterialInstance.SetColor("_Color", new Color(lineStartColor.r, lineStartColor.g, lineStartColor.b, from));
//                 currentSequence.Join(DOTween.To(() => lineMaterialInstance.color.a, a => {
//                     Color c = lineMaterialInstance.color;
//                     c.a = a;
//                     lineMaterialInstance.color = c;
//                 }, to, turnOnDuration).SetEase(onEase));
//             }
//         }

//         // Sprite fade in and slow flicker pulse
//         if (fireLiteSprite != null)
//         {
//             fireLiteSprite.enabled = true;
//             Color c = fireLiteSprite.color;
//             fireLiteSprite.color = new Color(c.r, c.g, c.b, 0f);
//             currentSequence.Join(fireLiteSprite.DOFade(1f, turnOnDuration).SetEase(onEase));
//             // punch scale primero (visceral), luego iniciar parpadeo lento en loop (pingpong alpha)
//             currentSequence.Append(fireLiteSprite.transform.DOPunchScale(Vector3.one * flickerStrength, flickerDuration * 0.5f, 8, 0.25f));
//             currentSequence.OnComplete(() =>
//             {
//                 // looped slow flicker: ping-pong alpha entre 0.9 y 1.0 para sutil parpadeo
//                 continuousFlickerTween = fireLiteSprite.DOFade(0.85f, flickerDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
//             });
//         }

//         currentSequence.Play();
//     }

//     public void DeactivateFireLight()
//     {
//         isFireLightOn = false;
//         currentSequence?.Kill();
//         continuousFlickerTween?.Kill();

//         currentSequence = DOTween.Sequence();

//         // Light intensity -> 0
//         currentSequence.Join(DOTween.To(() => fireLight.intensity, v => fireLight.intensity = v, 0f, turnOffDuration).SetEase(offEase));
//         // Radius -> small epsilon to avoid zero weirdness
//         currentSequence.Join(DOTween.To(() => fireLight.pointLightOuterRadius, v => fireLight.pointLightOuterRadius = v, 0.01f, turnOffDuration).SetEase(offEase));
//         currentSequence.Join(DOTween.To(() => fireLight.pointLightInnerRadius, v => fireLight.pointLightInnerRadius = v, 0.01f, turnOffDuration).SetEase(offEase));

//         // Line fade out then disable
//         if (fireLine != null && lineMaterialInstance != null)
//         {
//             currentSequence.Join(DOTween.To(() => lineMaterialInstance.color.a, a => {
//                 Color col = lineMaterialInstance.color;
//                 col.a = a;
//                 lineMaterialInstance.color = col;
//             }, 0f, turnOffDuration).SetEase(offEase));
//             currentSequence.OnComplete(() => fireLine.enabled = false);
//         }
//         else if (fireLine != null)
//         {
//             currentSequence.OnComplete(() => fireLine.enabled = false);
//         }

//         // Sprite fade out
//         if (fireLiteSprite != null)
//         {
//             currentSequence.Join(fireLiteSprite.DOFade(0f, turnOffDuration).SetEase(offEase));
//             currentSequence.OnComplete(() => fireLiteSprite.enabled = false);
//         }

//         currentSequence.Play();
//     }

//     void ApplyImmediateOff()
//     {
//         fireLight.intensity = isFireLightOn ? fireLightIntensity : 0f;
//         fireLight.pointLightOuterRadius = isFireLightOn ? fireLightRange : 0.01f;
//         fireLight.pointLightInnerRadius = isFireLightOn ? fireLightRange * 0.8f : 0.01f;
//         if (fireLine != null) fireLine.enabled = isFireLightOn;
//         if (fireLiteSprite != null)
//         {
//             fireLiteSprite.enabled = isFireLightOn;
//             Color c = fireLiteSprite.color;
//             fireLiteSprite.color = new Color(c.r, c.g, c.b, isFireLightOn ? 1f : 0f);
//         }
//         if (lineMaterialInstance != null)
//         {
//             Color col = lineMaterialInstance.color;
//             col.a = isFireLightOn ? 1f : 0f;
//             lineMaterialInstance.color = col;
//         }
//     }

//     void DrawLine(Vector2 startPos, Vector2 endPos)
//     {
//         if (fireLine == null) return;
//         fireLine.SetPosition(0, startPos);
//         fireLine.SetPosition(1, endPos);
//     }

//     void ShootFireLine()
//     {
//         if (firePoint == null) return;

//         RaycastHit2D hit = Physics2D.Raycast((Vector2)firePoint.position, (Vector2)firePoint.up, fireLightRange, layerMask.value);

//         if (hit.collider != null)
//         {
//             // visual distance slightly larger than hit distance
//             float distanceVisible = hit.distance + rangeOffset;
//             // Clamp so it doesn't exceed configured max (optional). Remove Mathf.Min(...) if you want unlimited growth.
//             distanceVisible = Mathf.Min(distanceVisible, fireLightRange * 1.5f); // allow some extra but cap
//             Vector2 endPoint = (Vector2)firePoint.position + (Vector2)firePoint.up * distanceVisible;
//             DrawLine((Vector2)firePoint.position, endPoint);
//             Set2DLightsAttributes(distanceVisible);

//             if (hit.collider.name == "Mirror")
//             {
//                 if (mirrorMoveController != null) mirrorMoveController.canMoveObject = false;
//                 ActivateMirror(hit);
//             }
//         }
//         else
//         {
//             float distanceVisible = fireLightRange;
//             DrawLine((Vector2)firePoint.position, (Vector2)firePoint.position + (Vector2)firePoint.up * distanceVisible);
//             Set2DLightsAttributes(distanceVisible);
//         }
//     }

//     void Set2DLightsAttributes(float distance)
//     {
//         // Usa el distance recibido (ya con offset). No sobrescribimos fireLightRange salvo como límite en ShootFireLine.
//         fireLight.pointLightOuterRadius = distance;
//         fireLight.pointLightInnerRadius = distance * 0.8f;
//         fireLight.pointLightOuterAngle = fireLightSpotAngle;
//     }

//     void ActivateMirror(RaycastHit2D hit)
//     {
//         var nextMirror = hit.collider.gameObject;
//         var mirrorCtrl = nextMirror.GetComponent<MirrorMoveController>();
//         if (mirrorCtrl != null) mirrorCtrl.canMoveObject = true;

//         var nextLight = nextMirror.GetComponent<LightsController>();
//         if (nextLight != null)
//         {
//             nextLight.isFireLightOn = true;
//             nextLight.ActivateFireLight();
//         }
//     }

//     void OnDestroy()
//     {
//         if (lineMaterialInstance != null)
//         {
//             Destroy(lineMaterialInstance);
//             lineMaterialInstance = null;
//         }
//         currentSequence?.Kill();
//         continuousFlickerTween?.Kill();
//     }
// }