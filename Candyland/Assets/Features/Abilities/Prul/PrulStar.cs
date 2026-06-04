using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody))]
public class PrulStar : MonoBehaviour
{
    private ObjectPool<PrulStar> _pool;
    private Transform _pivot;
    private float _startForce;
    private float _lifeTime;
    
    private Rigidbody _rigidbody;
    private Coroutine _lifeCoroutine;
    
    public void Initialize(ObjectPool<PrulStar> pool, Transform pivot, float startForce, float lifeTime)
    {
        _pool = pool;
        _pivot = pivot;
        _startForce = startForce;
        _lifeTime = lifeTime;
        
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        _lifeCoroutine = StartCoroutine(LifeCoroutine());
        
        
        transform.position = _pivot.position;
        transform.rotation = _pivot.rotation;
        
        _rigidbody.AddForce(transform.forward * _startForce, ForceMode.Impulse);
    }

    private IEnumerator LifeCoroutine()
         {
             float eclapse = 0;
             while (eclapse < _lifeTime)
             {
                 eclapse += Time.deltaTime;
                 yield return null;
             }
             
             Deactivate();
         }


    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Destroyable"))
        {
            if (other.gameObject.TryGetComponent<Destroyable>(out var destroyable))
            {
                destroyable.Destroy(other.contacts[0].point);
            }
            else
            {
                Debug.LogError($"Object: {other.gameObject.name} was marked as destroyable but doesn't have the component {typeof(Destroyable)} attached to it");
            }
        }
        StopCoroutine(_lifeCoroutine);
        Deactivate();
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
        _pool.Release(this);
    }
}
