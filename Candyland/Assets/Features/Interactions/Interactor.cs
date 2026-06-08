using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Interactor : MonoBehaviour
{
        [Header("Settings")] 
        [SerializeField] private bool _debug;
        [SerializeField] private float _interactionRadius;
        [SerializeField] private LayerMask _interactionLayer;
        [Tag] [SerializeField] private string[] _includedTags;

        private PlayerInput _playerInput;
        private InputAction _interact;
        private IInteractable[] _lastInteractables;


        private void Awake()
        {
                _playerInput = GetComponent<PlayerInput>();
                _interact = _playerInput.actions["Interact"];
        }

        private void OnInteractStarted(InputAction.CallbackContext ctx)
        {
                var interactions = Physics.OverlapSphere(transform.position, _interactionRadius, _interactionLayer);
                if (interactions.Length == 0)
                        return;
                
                var interaction  = interactions[0];

                foreach (var tag in _includedTags)
                {
                        if (!interaction.CompareTag(tag)) continue;
                        
                        _lastInteractables = interaction.GetComponents<IInteractable>();
                        foreach (var lastInteractable in _lastInteractables)
                        {
                                lastInteractable.Started(_playerInput);
                        }
                }
        }
        
        private void OnInteractCanceled(InputAction.CallbackContext ctx)
        {
                if(_lastInteractables.Length == 0)
                        return;
                
                foreach (var lastInteractable in _lastInteractables)
                {
                        lastInteractable.Canceled(_playerInput);
                }
        }
        private void OnDrawGizmos()
        {
                if (!_debug)
                        return;
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }


        private void OnEnable()
        {
                _interact.started += OnInteractStarted;
                _interact.canceled += OnInteractCanceled;
        }
        private void OnDisable()
        {
                _interact.started -= OnInteractStarted;
                _interact.canceled -= OnInteractCanceled;
        }
}
