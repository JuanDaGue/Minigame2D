// IMirrorInputHandler.cs
using UnityEngine;


// IMirrorRotator.cs
public interface IMirrorRotator
{
    void Rotate(Vector3 rotation);
    void RotateToAngle(float angle);
    bool IsRotating { get; }
}

