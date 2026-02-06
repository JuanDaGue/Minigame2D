// IMirrorManager.cs
public interface IMirrorManager
{
    MirrorMoveController CurrentMirror { get; }
    void Initialize();
    void HandleMirrorHit(MirrorMoveController controller);
    void HandleMirrorTapped(MirrorMoveController tapped);
}

