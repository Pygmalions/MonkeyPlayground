using System.Collections.Generic;
using UnityEngine;
using MonkeyPlayground.Components;

namespace MonkeyPlayground.Objects.Items
{
    public class Box : Item
    {
        public Box()
        {
            itemName = "Box";
        }
        
        private Collider2D _boxCollider;
        private RigidbodyType2D _originalBodyType;
        private CollapsibleFeature _collapsible;
        private const float PlatformTopBuffer = 0.05f;
        
        private readonly HashSet<Collider2D> _monkeyColliders = new();

        private void Awake()
        {
            _boxCollider = GetComponent<Collider2D>();
            TryGetComponent(out _collapsible);
        }
        
        protected override void Update()
        {
            base.Update();
            foreach (var monkeyCollider in _monkeyColliders)
            {
                var monkeyFeetY = monkeyCollider.bounds.min.y;
                var boxTopY = _boxCollider.bounds.max.y;

                Physics2D.IgnoreCollision(_boxCollider, monkeyCollider, monkeyFeetY < boxTopY - PlatformTopBuffer);
            }
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _monkeyColliders.Add(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) 
                return;
            if (!_monkeyColliders.Remove(other)) 
                return;
            Physics2D.IgnoreCollision(_boxCollider, other, false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                ContactPoint2D contact = collision.GetContact(0);
                if (contact.normal.y < -0.5f)
                {
                    if (_collapsible != null) 
                        _collapsible.TriggerSquash();
                    _boxCollider = GetComponent<Collider2D>();
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) 
                return;
            if (_collapsible != null) 
                _collapsible.TriggerRestore();
            _boxCollider = GetComponent<Collider2D>();
        }
    }
}