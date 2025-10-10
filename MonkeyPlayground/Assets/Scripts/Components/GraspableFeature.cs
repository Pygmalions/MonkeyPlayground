using System;
using System.Linq;
using UnityEngine;

namespace MonkeyPlayground.Components
{
    [DisallowMultipleComponent,
     RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class GraspableFeature : MonoBehaviour
    {
        private Rigidbody2D _rigidBody;
        private BoxCollider2D _collider;
        private SpriteRenderer _spriteRenderer;

        private RigidbodyType2D _originalBodyType;
        private RigidbodyConstraints2D _originalConstraints;
        private int _originalSortingOrder;
        private int _originalSortingLayer;

        /// <summary>
        /// Whether this object is currently picked up by the monkey.
        /// </summary>
        public bool IsPickedUp { get; private set; }

        private Collider2D[] _colliders;
        
        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            _colliders = GetComponents<Collider2D>()
                .Where(target => target.enabled)
                .ToArray();
        }

        public static GraspableFeature FindNearestGraspableObject(Collider2D climberCollider)
        {
            var climberTransform = climberCollider.transform;
            
            var regionPosition = new Vector2()
            {
                x = climberTransform.position.x,
                y = climberTransform.position.y + climberCollider.bounds.extents.y * 0.5f
            };
            
            var regionSize = new Vector2()
            {
                x = climberCollider.bounds.size.x + 1.0f,
                y = climberCollider.bounds.size.y * 0.8f
            };
        
            return Physics2D.OverlapBoxAll(regionPosition, regionSize, 0)
                .Select(target => (Collider: target, Feature: target.GetComponent<GraspableFeature>()))
                .Where(target => target.Feature)
                .OrderBy(target => 
                    Mathf.Abs(target.Collider.bounds.center.x - climberTransform.position.x))
                .Select(target => target.Feature)
                .FirstOrDefault();
        }
        
        public void Grab(Collider2D holderCollider)
        {
            if (IsPickedUp)
                throw new InvalidOperationException("This object is already grabbed.");
            IsPickedUp = true;
            
            var holderTransform = holderCollider.transform;
            
            var newPosition = new Vector3
            {
                x = 0,
                y = holderCollider.bounds.extents.y + _collider.bounds.extents.y,
                z = 0
            };
            
            _originalBodyType = _rigidBody.bodyType;
            _originalConstraints = _rigidBody.constraints;
            _rigidBody.bodyType = RigidbodyType2D.Kinematic;
            _rigidBody.linearVelocity = Vector2.zero;
            _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
            foreach (var targetCollider in _colliders)
            {
                targetCollider.enabled = false;
            }

            transform.SetParent(holderTransform);
            transform.localPosition = newPosition;

            _originalSortingOrder = _spriteRenderer.sortingOrder;
            _originalSortingLayer = _spriteRenderer.sortingLayerID;
            _spriteRenderer.sortingLayerID = SortingLayer.NameToID("HoldingItem");
        }

        public void Drop()
        {
            if (!IsPickedUp)
                throw new InvalidOperationException("This object is not grabbed.");
            
            transform.SetParent(null);
            
            _rigidBody.bodyType = _originalBodyType;
            _rigidBody.constraints = _originalConstraints;
            
            _spriteRenderer.sortingOrder = _originalSortingOrder;
            _spriteRenderer.sortingLayerID = _originalSortingLayer;
            
            foreach (var targetCollider in _colliders)
                targetCollider.enabled = true;
            
            IsPickedUp = false;
        }
    }
}