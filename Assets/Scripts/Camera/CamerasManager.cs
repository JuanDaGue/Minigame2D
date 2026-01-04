using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;
using System.Collections;

public class CamerasManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera camA;
    [SerializeField] private CinemachineCamera camB;
    [SerializeField] private Transform followProxy; // asignar en Inspector o se crea en Awake
    [SerializeField] private int tempPriority = 50;
    [SerializeField] private Ease ease = Ease.InOutSine;

    private bool usingCamA = true;

    void Awake()
    {
        // Crear proxy si no está asignado
        if (followProxy == null)
        {
            var go = new GameObject("FollowProxy");
            go.transform.position = new Vector3(0f, 0f, -10f); // z fijo para 2D
            followProxy = go.transform;
        }
    }

    public void SetCameraPriority()
    {
        usingCamA = !usingCamA;
        if (camA != null) camA.Priority = usingCamA ? 10 : 0;
        if (camB != null) camB.Priority = usingCamA ? 0 : 10;
    }

    // offset permite ajustar el encuadre (por ejemplo elevar la cámara)
    public void MoveCameraToTarget(Transform target, float duration, Vector3 offset)
    {
        if (target == null || followProxy == null)
        {
            Debug.LogWarning("target o followProxy es null");
            return;
        }

        var activeCam = usingCamA ? camA : camB;
        if (activeCam == null)
        {
            Debug.LogWarning("Cinemachine camera activa es null");
            return;
        }

        // Hacer live la cámara subiendo prioridad
        int originalPriority = activeCam.Priority;
        activeCam.Priority = tempPriority;

        // Asegurar que la cámara siga al proxy
        activeCam.Follow = followProxy;
        activeCam.LookAt = target;

        // Destino preservando la Z del proxy (solo X,Y cambian)
        Vector3 destination = new Vector3(target.position.x + offset.x, target.position.y + offset.y, followProxy.position.z);

        // Cancelar tweens previos y mover proxy
        followProxy.DOKill();
        followProxy.DOMove(destination, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                // Restaurar prioridad después de un pequeño retardo para asegurar blend
                StartCoroutine(RestorePriorityAfter(0.1f, activeCam, originalPriority, target));
            });
    }

    private IEnumerator RestorePriorityAfter(float seconds, CinemachineCamera cam, int originalPriority, Transform finalFollow)
    {
        yield return new WaitForSeconds(seconds);
        if (cam != null)
        {
            cam.Priority = originalPriority;
            // Opcional: que la cámara ahora siga directamente al target
            cam.Follow = finalFollow;
            cam.LookAt = finalFollow;
        }
    }
}