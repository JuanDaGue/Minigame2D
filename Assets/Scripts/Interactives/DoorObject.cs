using UnityEngine;
public class DoorObject : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;
    private static readonly int OpenDoorTrigger = Animator.StringToHash("OpenDoor");
    [SerializeField] private IUnLookDoor unLookDoor;

    void Start()
    {
        if (unLookDoor == null)
        {
            unLookDoor = FindFirstObjectByType<IUnLookDoor>();
        }
        if(unLookDoor != null)
        {
            unLookDoor.OnOpenDoor += OpenDoor;
        }
    }
    public void OpenDoor()
    {
        Debug.Log("[DoorObject] Opening Door");
        //doorAnimator.SetTrigger(OpenDoorTrigger);
        gameObject.SetActive(false);
    }
    
}