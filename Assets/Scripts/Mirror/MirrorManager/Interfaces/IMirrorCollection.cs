

using System.Collections.Generic;

public interface IMirrorCollection
{
    List<MirrorMoveController> Mirrors { get; }
    void AddMirror(MirrorMoveController mirror);
    void RemoveMirrorsAfter(int index);
    bool Contains(MirrorMoveController mirror);
}

