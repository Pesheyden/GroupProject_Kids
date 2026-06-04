using System;
using NaughtyAttributes;
using UnityEngine;

public class Interactor : MonoBehaviour
{
        [Header("Settings")] 
        [SerializeField] private bool _debug;
        [SerializeField] private float _interactionRadius;
        [SerializeField] private LayerMask _interactionLayer;
        [Tag] [SerializeField] private string[] _includedTags;
        
        private void OnAttack()
        {
                var interaction = Physics.OverlapSphere(transform.position, _interactionRadius, _interactionLayer)[0];

                foreach (var tag in _includedTags)
                {
                        if (interaction.CompareTag(tag))
                                interaction.GetComponent<IInteractable>().Activate();
                }
        }

        private void OnDrawGizmos()
        {
                if (!_debug)
                        return;
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
}
