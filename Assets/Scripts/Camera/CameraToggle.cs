using UnityEngine;
using Unity.Cinemachine;

public class CameraToggle : MonoBehaviour
{
    [SerializeField] private CinemachineCamera camA;
    [SerializeField] private CinemachineCamera camB;

    private bool usingCamA = true;

    // void Start()
    // {
    //     SetCameraPriority();
    // }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
           
            SetCameraPriority();
        }
    }

    public  void SetCameraPriority()
    {
        usingCamA = !usingCamA;
        camA.Priority = usingCamA ? 10 : 0;
        camB.Priority = usingCamA ? 0 : 10;
    }
}