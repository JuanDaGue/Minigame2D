// IMirrorInputHandler.cs
using UnityEngine;


// IMirrorVisualFeedback.cs
public interface IMirrorVisualFeedback
{
    void ToggleLight();
    void FlashLight(float duration = 0.3f, float multiplier = 1.5f);
}