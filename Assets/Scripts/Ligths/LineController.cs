using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEditor;

public class LineController : MonoBehaviour
{
    private Transform firePoint;
    [SerializeField] private LaserLinePoints laserLinePoints;
    [SerializeField] private LigthsController ligthsController;
    public LayerMask layerMask;
    public event Action<MirrorMoveController> OnMirrorHit;
    public void Initialize(Transform firePointTransform)
    {
        firePoint = firePointTransform;
    }
    void Awake()
    {
        if (firePoint == null)
        {
            firePoint = this.transform;
        }
        if (ligthsController == null)
        {
            ligthsController = GetComponent<LigthsController>();
        }
    }
    public void ShotRayline(Vector3 direction, float maxDistance)
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, maxDistance, layerMask);
        Debug.DrawRay(firePoint.position, direction * maxDistance, Color.red, 0.5f);

        if (hit.collider != null)
        {
            laserLinePoints.DrawLaserRay(firePoint.position, hit.point);
            Debug.Log("Ray hit: " + hit.collider.name);
            MirrorMoveController mirror = hit.collider.GetComponent<MirrorMoveController>();
            float distance = Vector3.Distance(firePoint.position, hit.point);
            //Debug.Log("Hit distance: " + distance);
            ligthsController.ApplyLightAttributes(distance);    
            if (mirror != null && mirror.CompareTag("Mirror"))
            {
                Debug.Log("Hit a mirror: " + mirror.name);

                OnMirrorHit?.Invoke(mirror);   
            }
            
            
        }
        else
        {
            Debug.Log("Ray did not hit any collider.");
            ligthsController.ForceDeactivate();
        }
    }
    void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, firePoint.up * 10f);
        }
    }

}
