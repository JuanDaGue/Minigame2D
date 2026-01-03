using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ISwitch : MonoBehaviour, IInteractivesObject
{
    public event Action RotateMirrorEvent;
    [SerializeField] private Light2D light2D;
    void Start()
    {
        if (light2D == null)
        {
            light2D = GetComponent<Light2D>();
        }
        if (light2D != null)
        {
            //Debug.LogWarning("[ISwitch] No Light2D component found on this GameObject.");
            light2D.intensity = 0f;
        }
    }
    public bool CanInteract()
    {
        return true;
    }
    public void OnInteract()
    {
        Debug.Log("[UnLookDoor] Door Unlocked");
        RotateMirrorEvent?.Invoke();
        light2D.intensity = 2.5f;
        //Destroy(this.gameObject);
    }
}
