using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FootStepsEmitter : MonoBehaviour
{
    [SerializeField] private EventReference _footstepEvent;
    public void PlayFootstep()
    {
        RuntimeManager.PlayOneShot(_footstepEvent,transform.position);
    }
}
