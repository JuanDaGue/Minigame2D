// IMirrorInputHandler.cs
using UnityEngine;

public interface IMirrorInputHandler
{
    event System.Action<MirrorMoveController> OnMirrorTapped; // 👈 añadir aquí

    void HandleTap(Vector2 screenPos);
    void HandleDragDelta(Vector2 previousScreen, Vector2 currentScreen);
    void HandleDragEnd(Vector2 screenPos);
    void HandlePinch(float delta);
    bool CanProcessInput();
}