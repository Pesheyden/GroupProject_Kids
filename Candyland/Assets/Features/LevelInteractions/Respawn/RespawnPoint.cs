using System;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [Header("Settings")]
    public bool OnTriggerRespawn = true;
    [ShowIf("OnTriggerRespawn")][SerializeField] [Tag] private string[] _allowedTags;


    private void OnTriggerEnter(Collider other)
    {
        if(!OnTriggerRespawn)
            return;
        
        if (_allowedTags.Length > 0 && !_allowedTags.Any(other.CompareTag))
            return;
        
        SetSpawnPoint(other);
    }

    public void SetSpawnPoint(Collider other)
    {
        other.gameObject.GetComponent<RespawnHandler>().SetSpawnPoint(transform.position);
    }
    
}
