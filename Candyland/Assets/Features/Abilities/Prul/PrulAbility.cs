using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PrulAbility : MonoBehaviour
{
        [Header("References")] 
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _starsParent;

        [Header("Star")] 
        [SerializeField] private GameObject _starPrefab;
        [SerializeField] private float _shootForce;
        [SerializeField] private float _lifeTime;
        
        
        private ObjectPool<PrulStar> _starPool;
        private List<PrulStar> _activeStarts;


        private void Awake()
        {
                _starPool = new ObjectPool<PrulStar>(CreatePoolItem);
        }
        
        private void Activate()
        {
                _starPool.Get().Activate();
        }
        
        //Pool callbacks
        public PrulStar CreatePoolItem()
        {
                var star = Instantiate(_starPrefab, _starsParent).GetComponent<PrulStar>();
                star.Initialize(_starPool, _pivot, _shootForce, _lifeTime);
                return star;
        }

        //Input callbacks
        private void OnAttack()
        {
                Activate();
        }
}
