using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    [Foldout("Events")] [Tooltip("Triggers when all conditions are fulfilled")]
    public UnityEvent<Collider> OnTrigger;
    [Foldout("Events")] 
    public UnityEvent<Collider> OnTriggerEnterEvent;
    [Foldout("Events")] 
    public UnityEvent<Collider> OnTriggerStayEvent;
    [Foldout("Events")] 
    public UnityEvent<Collider> OnTriggerExitEvent;

    [Header("Settings")] 
    [SerializeField] private bool _canTriggerMultipleTimes = true;
    [SerializeField] private int _neededObjectsToTriggerCount = 1;
    [SerializeField] private bool _needToStayToTrigger;
    [SerializeField] [Tooltip("If null, all will be allowed")] [Tag] private string[] _allowedTags;
    
    [Header("Delays")] 
    [SerializeField] private float _onTriggerEnterDelay;
    [SerializeField] private float _onTriggerStayDelay;
    [SerializeField] private float _onTriggerExitDelay;


    private List<Collider> _colliders = new List<Collider>();
    private bool _triggered;
    
    private IEnumerator OnTriggerEnter(Collider other)
    {
        //Collider
        if (_allowedTags.Length > 0 && !_allowedTags.Any(other.CompareTag))
                yield break;
        if (_needToStayToTrigger)
            _colliders.Add(other);
        
        OnTriggerEnterEvent?.Invoke(other);
        
        //Conditions
        if(_neededObjectsToTriggerCount > _colliders.Count)
            yield break;
        if(!_canTriggerMultipleTimes && _triggered)

        //Trigger
        yield return new WaitForSeconds(_onTriggerEnterDelay);
        _triggered = true;
        _colliders.Clear();
        OnTrigger?.Invoke(other);
    }
    private IEnumerator OnTriggerStay(Collider other)
    {
        //Collider
        if (_allowedTags.Length > 0 && !_allowedTags.Any(other.CompareTag))
            yield break;
        
        yield return new WaitForSeconds(_onTriggerStayDelay);
        OnTriggerStayEvent?.Invoke(other);
    }
    private IEnumerator OnTriggerExit(Collider other)
    {
        //Collider
        if (_allowedTags.Length > 0 && !_allowedTags.Any(other.CompareTag))
            yield break;

        if (_needToStayToTrigger)
            _colliders.Remove(other);
        
        
        //Trigger
        yield return new WaitForSeconds(_onTriggerExitDelay);
        OnTriggerExitEvent?.Invoke(other);
    }
}
