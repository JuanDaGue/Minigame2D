using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MirrorCollection : IMirrorCollection
{
    public List<MirrorMoveController> Mirrors { get; private set; } = new List<MirrorMoveController>();
    
    public void AddMirror(MirrorMoveController mirror)
    {
        if (mirror == null || Mirrors.Contains(mirror)) return;
        Mirrors.Add(mirror);
    }
    
    public void RemoveMirrorsAfter(int index)
    {
        for (int i = Mirrors.Count - 1; i > index; i--)
        {
            if (i >= 0 && i < Mirrors.Count)
            {
                var mirror = Mirrors[i];
                Mirrors.RemoveAt(i);
                
                // Optional cleanup
                if (mirror != null)
                {
                    mirror.gameObject.SetActive(false);
                    Object.Destroy(mirror.gameObject);
                }
            }
        }
    }
    
    public bool Contains(MirrorMoveController mirror)
    {
        return Mirrors.Contains(mirror);
    }
    
    public MirrorMoveController GetMirrorAtIndex(int index)
    {
        if (index >= 0 && index < Mirrors.Count)
            return Mirrors[index];
        return null;
    }
    
    public int IndexOf(MirrorMoveController mirror)
    {
        return Mirrors.IndexOf(mirror);
    }
}