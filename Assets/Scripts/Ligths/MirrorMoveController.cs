using UnityEngine;
using DG.Tweening; // Make sure DOTween is imported

public class MirrorMoveController : MonoBehaviour
{
    public float rotationAmount = 10.0f;
    public float rotationDuration = 0.3f;
    public Transform mirrorPoint;
    [SerializeField] private bool CanMoveObject = true;

    public bool canMoveObject
    {
        get { return CanMoveObject; }
        set { CanMoveObject = value; }
    }

    void Start()
    {
        mirrorPoint = this.transform;
    }

    void Update()
    {
        if (!canMoveObject || DOTween.IsTweening(mirrorPoint)) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Left Arrow Pressed");
            RotateMirror(Vector3.forward * rotationAmount);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Right Arrow Pressed");
            RotateMirror(Vector3.back * rotationAmount);
        }
    }

    void RotateMirror(Vector3 rotation)
    {
        mirrorPoint.DORotate(mirrorPoint.eulerAngles + rotation, rotationDuration, RotateMode.Fast);
    }
}

// using UnityEngine;
// using DG.Tweening;

// public class MirrorMoveController : MonoBehaviour
// {
//     [Header("Rotation")]
//     public float rotationAmount = 10f;            // degrees per "step"
//     public float rotationDuration = 0.25f;       // tween duration
//     public float dragThreshold = 10f;            // px minimum to register a drag
//     public float maxStepMultiplier = 3f;         // clamp how big a step can be based on drag

//     [Header("References")]
//     public Transform mirrorPoint;
//     [SerializeField] private bool CanMoveObject = true;

//     public bool canMoveObject
//     {
//         get => CanMoveObject;
//         set => CanMoveObject = value;
//     }

//     // internal
//     private Vector2 pointerStart;
//     private bool isDragging;
//     private Tween rotationTween;

//     void Start()
//     {
//         if (mirrorPoint == null) mirrorPoint = transform;
//     }

//     void Update()
//     {
        
//         if(Input.GetKeyDown(KeyCode.LeftArrow))
//         {
//             RotateLeftStep();
//         }
//         else if(Input.GetKeyDown(KeyCode.RightArrow))
//         {
//             RotateRightStep();
//         }
//         if (!canMoveObject) return;

//         // mouse input
//         if (Input.GetMouseButtonDown(0))
//         {
//             pointerStart = Input.mousePosition;
//             isDragging = true;
//         }

//         if (isDragging && Input.GetMouseButtonUp(0))
//         {
//             Vector2 pointerEnd = Input.mousePosition;
//             HandleDrag(pointerStart, pointerEnd);
//             isDragging = false;
//         }

//         // touch input (single touch)
//         if (Input.touchCount == 1)
//         {
//             Touch t = Input.GetTouch(0);
//             if (t.phase == TouchPhase.Began)
//             {
//                 pointerStart = t.position;
//                 isDragging = true;
//             }
//             else if (isDragging && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
//             {
//                 HandleDrag(pointerStart, t.position);
//                 isDragging = false;
//             }
//         }
//     }

//     private void HandleDrag(Vector2 start, Vector2 end)
//     {
//         float deltaY = end.y - start.y;

//         // ignore tiny drags
//         if (Mathf.Abs(deltaY) < dragThreshold) return;

//         // Map drag magnitude to steps; positive deltaY = drag up, negative = drag down
//         // You wanted: drag down -> rotate right; drag up -> rotate left
//         // We treat "right" as negative around Z (back) and "left" as positive around Z (forward)
//         float sign = deltaY > 0 ? 1f : -1f; // up -> +1, down -> -1

//         // bigger drag -> bigger multiplier, clamp it
//         float multiplier = Mathf.Clamp(Mathf.Abs(deltaY) / (dragThreshold * 1.5f), 1f, maxStepMultiplier);

//         // compute final degrees to rotate: drag up => rotate left (positive z), drag down => rotate right (negative z)
//         float degrees = sign * rotationAmount * multiplier;

//         // Invert sign so drag up becomes left (positive), drag down becomes right (negative)
//         // If your coordinate system differs, flip sign here.
//         float targetZ = mirrorPoint.eulerAngles.z + degrees;

//         // Kill existing tween and create new one
//         rotationTween?.Kill();

//         rotationTween = mirrorPoint.DORotate(new Vector3(mirrorPoint.eulerAngles.x,
//                                                          mirrorPoint.eulerAngles.y,
//                                                          targetZ),
//                                              rotationDuration,
//                                              RotateMode.FastBeyond360)
//                                   .SetEase(Ease.OutQuad);
//     }

//     // Optional: public methods to rotate with keyboard like your original example
//     public void RotateLeftStep()
//     {
//         if (!canMoveObject) return;
//         RotateBy(rotationAmount);
//     }

//     public void RotateRightStep()
//     {
//         if (!canMoveObject) return;
//         RotateBy(-rotationAmount);
//     }

//     private void RotateBy(float degrees)
//     {
//         rotationTween?.Kill();
//         float targetZ = mirrorPoint.eulerAngles.z + degrees;
//         rotationTween = mirrorPoint.DORotate(new Vector3(0, 0, targetZ), rotationDuration, RotateMode.FastBeyond360)
//                                   .SetEase(Ease.OutQuad);
//     }
// }