using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class MarshAbility : MonoBehaviour
{
    [SerializeField] private Vector3 _targetPosition;
    [SerializeField] private bool _debug;
    [SerializeField] private Transform _pivot;
    [SerializeField] private Transform _projectile;
    [SerializeField] private Transform _platform;
    [SerializeField] private float _duration;
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private float _radius;
    [SerializeField] private float _shootDistance;

    private bool _projectileFlying;
    
    [Button]
    public void Activate()
    {
        var targets = Physics.OverlapSphere(transform.position, _radius, _targetLayers);

        if (targets.Length > 0)
        {
            _targetPosition = targets[0].transform.position;
        }
        else
        {
            if (!Physics.Raycast(_pivot.position + _pivot.forward * _shootDistance + Vector3.up, Vector3.down, out var hit))
            {
                Debug.LogWarning("Can't shoot there. Target point is not located");
            }

            _targetPosition = hit.point;
        }
        
        _platform.gameObject.SetActive(false);
        _projectileFlying = true;
        _projectile.gameObject.SetActive(true);
        StartCoroutine(CurveMoveCoroutine(_pivot.position, _targetPosition));
    }

    private IEnumerator CurveMoveCoroutine(Vector3 start, Vector3 end)
    {
        float t;
        float eclipse = 0; 
        while (eclipse < _duration)
        {
            Vector3 control = (start + end) * 0.5f + Vector3.up * Mathf.Sqrt( (start - end).sqrMagnitude) / 2;

            t = eclipse / _duration;
            
            _projectile.position = BezierCurve(start, control, end, t);

            eclipse += Time.deltaTime;
            yield return null;
        }
        
        OnHitTarget(end);
    }

    private void OnHitTarget(Vector3 position)
    {
        _projectileFlying = false;
        _projectile.gameObject.SetActive(false);
        
        _platform.position = position;
        
        _platform.gameObject.SetActive(true);
    }


    public static Vector3 BezierCurve(
        Vector3 start,
        Vector3 control,
        Vector3 end,
        float t)
    {
        float u = 1f - t;

        return u * u * start +
               2f * u * t * control +
               t * t * end;
    }


    private void OnAttack()
    {
        Activate();
    }


    private void OnDrawGizmos()
    {
        if(!_debug)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_targetPosition, 0.5f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_pivot.position + _pivot.forward * _shootDistance + Vector3.up, 0.25f);
        Gizmos.DrawRay(_pivot.position + _pivot.forward * _shootDistance + Vector3.up, Vector3.down);
    }
}
