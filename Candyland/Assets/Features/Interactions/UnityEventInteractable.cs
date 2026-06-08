using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UnityEventInteractable : MonoBehaviour, IInteractable
{
    [Foldout("Event")] 
    public UnityEvent<PlayerInput> OnInteractionStart;
    [Foldout("Event")] 
    public UnityEvent<PlayerInput> OnInteractionCanceled;
    [Foldout("Event")] 
    public UnityEvent<PlayerInput> OnInteraction;
    
    [Header("Settings")] 
    [SerializeField] private int _neededObjectsToTriggerCount = 1;
    [SerializeField] private bool _needToHoldToTrigger;
    [SerializeField] private float _triggerDelay;


    private List<PlayerInput> _playerInputs = new List<PlayerInput>();
    
    public void Started(PlayerInput playerInput)
    {
        if(_playerInputs.Any(pi => playerInput == pi))
            return;
            
        _playerInputs.Add(playerInput);
        
        OnInteractionStart?.Invoke(playerInput);
        
        //Conditions
        if(_neededObjectsToTriggerCount > _playerInputs.Count)
            return;
        
        _playerInputs.Clear();
        OnInteraction?.Invoke(playerInput);
    }

    public void Canceled(PlayerInput playerInput)
    {
        if (_needToHoldToTrigger)
            _playerInputs.Remove(playerInput);
        
        OnInteractionCanceled?.Invoke(playerInput);
    }
}
