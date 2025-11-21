
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class CameraSwitcher2D : MonoBehaviour
{
    // Use GameObject here so the script compiles even if Cinemachine package is not present;
    // the actual Cinemachine component (if present) will be found via reflection at runtime.
    public GameObject closeCam;
    public GameObject farCam;
    public int closePriority = 20;
    public int farPriority = 10;

    // Test method to set initial priorities
    private void Start()
    {
        SetPriority(closeCam, closePriority);
        SetPriority(farCam, farPriority);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Toggling Camera");
            Toggle();
        }
    }
    // Llamar desde el Button OnClick para alternar

    public void SwitchToClose()
    {
        SetPriority(closeCam, closePriority);
        SetPriority(farCam, farPriority);
    }

    public void SwitchToFar()
    {
        SetPriority(closeCam, farPriority);
        SetPriority(farCam, closePriority);
    }

    // Alternar simple
    public void Toggle()
    {
        int cP = GetPriority(closeCam);
        int fP = GetPriority(farCam);
        if (cP > fP) SwitchToFar();
        else SwitchToClose();
    }

    private void SetPriority(GameObject camObj, int priority)
    {
        if (camObj == null) return;

        var vmType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("Cinemachine.CinemachineVirtualCamera"))
            .FirstOrDefault(t => t != null);

        if (vmType == null)
        {
            vmType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Cinemachine.CinemachineVirtualCameraBase"))
                .FirstOrDefault(t => t != null);
        }

        if (vmType == null) return; // Cinemachine not available

        var comp = camObj.GetComponent(vmType);
        if (comp == null) return;

        var prop = vmType.GetProperty("Priority", BindingFlags.Instance | BindingFlags.Public);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(comp, priority, null);
            return;
        }

        var field = vmType.GetField("m_Priority", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(comp, priority);
        }
    }

    private int GetPriority(GameObject camObj)
    {
        if (camObj == null) return 0;

        var vmType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("Cinemachine.CinemachineVirtualCamera"))
            .FirstOrDefault(t => t != null);

        if (vmType == null)
        {
            vmType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Cinemachine.CinemachineVirtualCameraBase"))
                .FirstOrDefault(t => t != null);
        }

        if (vmType == null) return 0;

        var comp = camObj.GetComponent(vmType);
        if (comp == null) return 0;

        var prop = vmType.GetProperty("Priority", BindingFlags.Instance | BindingFlags.Public);
        if (prop != null && prop.CanRead)
        {
            var val = prop.GetValue(comp, null);
            return Convert.ToInt32(val);
        }

        var field = vmType.GetField("m_Priority", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field != null)
        {
            var val = field.GetValue(comp);
            return Convert.ToInt32(val);
        }

        return 0;
    }
}
