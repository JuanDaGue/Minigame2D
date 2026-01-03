using System;
using UnityEngine;

public class IUnLookDoor : MonoBehaviour, IInteractivesObject
{
    public event Action OnOpenDoor;
    public bool CanInteract()
    {
        return true;
    }
    public void OnInteract()
    {
        Debug.Log("[UnLookDoor] Door Unlocked");
        OnOpenDoor?.Invoke();
        Destroy(this.gameObject);
    }
}
