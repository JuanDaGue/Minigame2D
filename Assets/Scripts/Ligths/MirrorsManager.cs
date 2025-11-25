using System.Collections.Generic;
using UnityEngine;

public class MirrorManager : MonoBehaviour
{
    [Tooltip("Lista de objetos que tienen LigthsController (espejos)")]
    public List<GameObject> mirrors = new List<GameObject>();

    private GameObject activeMirror;


    // void Start()
    // {
    //     // Opcional: si hay alguno marcado en la lista, activamos el primero
    //     if (mirrors.Count > 0)
    //         SetActiveMirror(mirrors[0]);
    // }

    public void SetActiveMirror(GameObject mirror)
    {
        if (mirror == activeMirror) return;
    
        
    
        // Activar el seleccionado (si tiene LigthsController)
        if (mirror != null)
        {
            if (activeMirror != null)
            {
                var mmc = activeMirror.GetComponent<MirrorMoveController>();
                if (mmc != null)
                    mmc.canMoveObject = false;
            }
            var lc = mirror.GetComponent<LigthsController>();
            if (lc != null)
            {
                lc.ForceActivate();
            }
        }
        activeMirror = mirror;
    }
    public void DeactiveAllMirrors()
    {
        // Desactivar todos
        for (int i = 0; i < mirrors.Count; i++)
        {
            var go = mirrors[i];
            if (go == null) continue;
            var lc = go.GetComponent<LigthsController>();
            if (lc != null)
            {
                lc.ForceDeactivate(); // método seguro para desactivar
            }
        }
        activeMirror = null;
    }

    // Helper para registrar dinámicamente espejos (opcional)
    public void RegisterMirror(GameObject mirror)
    {
        if (!mirrors.Contains(mirror)) mirrors.Add(mirror);
    }

    public void UnregisterMirror(GameObject mirror)
    {
        if (mirrors.Contains(mirror)) mirrors.Remove(mirror);
        if (activeMirror == mirror) activeMirror = null;
    }
}