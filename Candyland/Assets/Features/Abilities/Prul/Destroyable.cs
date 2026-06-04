using System.Collections;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Collider _collider;
    [SerializeField] private GameObject _basicGameObject;
    [SerializeField] private GameObject _destroyedGameObject;

    [Header("Settings")] 
    [SerializeField] private float _explosionStrength;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private float _partsLifeTime;

    private Rigidbody[] _parts;
    public void Destroy(Vector3 contactPoint)
    {
        _basicGameObject.SetActive(false);
        _destroyedGameObject.SetActive(true);
        _collider.enabled = false;

        _parts = _destroyedGameObject.GetComponentsInChildren<Rigidbody>();
        foreach (var part in _parts)
        {
            part.AddExplosionForce(_explosionStrength, contactPoint, _explosionRadius);
        }

        StartCoroutine(LifeCoroutine());
    }
    
    private IEnumerator LifeCoroutine()
    {
        float eclapse = 0;
        while (eclapse < _partsLifeTime)
        {
            eclapse += Time.deltaTime;
            yield return null;
        }

        foreach (var part in _parts)
        {
            Destroy(part.gameObject);
        }
    }
}
