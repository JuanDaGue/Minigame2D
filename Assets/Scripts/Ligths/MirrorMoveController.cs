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