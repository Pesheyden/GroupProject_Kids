using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class KillArea : MonoBehaviour
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
        
        Respawn(other);
    }

    public void Respawn(Collider other)
    {
        other.GetComponent<RespawnHandler>().Respawn();
    }
}
