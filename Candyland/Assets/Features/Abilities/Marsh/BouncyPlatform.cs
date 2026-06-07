using System;
using BSOAP.Variables;
using UnityEngine;

public class BouncyPlatform : MonoBehaviour
{
    [SerializeField] private float _force;
    private void OnTriggerEnter(Collider other)
    {
        if(!other.TryGetComponent<Rigidbody>(out var rb))
            return;
        
        rb.AddForce(transform.up * _force, ForceMode.Impulse);
    }
}
