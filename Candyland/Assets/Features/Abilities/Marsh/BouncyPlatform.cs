using System;
using BSOAP.Variables;
using NaughtyAttributes;
using UnityEngine;

public class BouncyPlatform : MonoBehaviour
{
    public float Force;
    private void OnTriggerEnter(Collider other)
    {
        if(!other.TryGetComponent<Rigidbody>(out var rb))
            return;
        
        rb.AddForce(transform.up * Force, ForceMode.Impulse);
    }
}
