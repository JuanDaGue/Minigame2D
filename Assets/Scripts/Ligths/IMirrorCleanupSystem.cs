// IMirrorCleanupSystem.cs
public interface IMirrorCleanupSystem
{
    void CleanMirrorsAfterTap(MirrorMoveController tappedMirror);
    void RemoveMirror(MirrorMoveController mirror);
    void ResetMirrorForMovement(MirrorMoveController mirror);
}

