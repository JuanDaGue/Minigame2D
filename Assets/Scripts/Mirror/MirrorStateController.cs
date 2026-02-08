using UnityEngine;

public class MirrorStateController : IMirrorStateController
{
    private readonly MirrorMoveController _mirror;
    private readonly LigthsController _mirrorLight;
    
    public MirrorState CurrentState { get; private set; }
    public bool CanMoveObject { get; private set; }
    
    public MirrorStateController(MirrorMoveController mirror, LigthsController mirrorLight)
    {
        _mirror = mirror;
        _mirrorLight = mirrorLight;
    }
    
    public void SwitchState(MirrorState newState)
    {
        CurrentState = newState;
        Debug.Log($"[{_mirror.name}] SwitchMirrorState -> {CurrentState}");
        
        switch (CurrentState)
        {
            case MirrorState.Active:
                Debug.Log($"[{_mirror.name}] Mirror is now Active");
                _mirrorLight?.ForceActivate();
                CanMoveObject = true;  // Active mirrors can move
                break;
            case MirrorState.Deactive:
                Debug.Log($"[{_mirror.name}] Mirror is now Deactive");
                _mirrorLight?.ForceDeactivate();
                CanMoveObject = false; // Deactive mirrors cannot move
                break;
            case MirrorState.Setted:
                Debug.Log($"[{_mirror.name}] Mirror is now Setted");
                _mirrorLight?.ForceActivate();
                CanMoveObject = false; // Setted mirrors cannot move but can be tapped for cleanup
                break;
        }
    }
}