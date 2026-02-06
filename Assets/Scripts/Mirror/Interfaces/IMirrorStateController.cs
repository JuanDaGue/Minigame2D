// IMirrorInputHandler.cs
using UnityEngine;


// IMirrorStateController.cs
public interface IMirrorStateController
{
    MirrorState CurrentState { get; }
    void SwitchState(MirrorState newState);
    bool CanMoveObject { get; }
}

