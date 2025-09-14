using UnityEngine;
using MonkeyPlayground.Data;
using MonkeyPlayground.Components;
using UnityEngine.Serialization;

namespace MonkeyPlayground.Objects.Items
{

    public class Box : Item
    {
        public Box()
        {
            itemName = "Box";
        }
        
        private Collider2D _monkeyCollider;
        private Collider2D _boxSolidCollider;
        private Rigidbody2D _boxRigidBody;
        private RigidbodyType2D _originalBodyType;
        private CollapsibleFeature _collapsible;
        private const float PlatformTopBuffer = 0.05f;
        
        private void Awake()
        {
            _boxSolidCollider = GetComponent<Collider2D>();
            _boxRigidBody = GetComponent<Rigidbody2D>();
            TryGetComponent(out _collapsible);
        }

        /// <summary>
        /// 在Update中持续检查猴子与箱子的相对位置
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if (_monkeyCollider == null)
            {
                return;
            }

            float monkeyFeetY = _monkeyCollider.bounds.min.y;
            float boxTopY = _boxSolidCollider.bounds.max.y;

            if (monkeyFeetY < boxTopY - PlatformTopBuffer)
            {
                Physics2D.IgnoreCollision(_boxSolidCollider, _monkeyCollider, true);
            }
            else
            {
                Physics2D.IgnoreCollision(_boxSolidCollider, _monkeyCollider, false);
            }
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _monkeyCollider = other;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (_monkeyCollider != null)
                {
                    Physics2D.IgnoreCollision(_boxSolidCollider, _monkeyCollider, false);
                }

                _monkeyCollider = null;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                ContactPoint2D contact = collision.GetContact(0);
                if (contact.normal.y < -0.5f)
                {
                    if (_collapsible != null)  _collapsible.TriggerSquash();
                    _boxSolidCollider = GetComponent<Collider2D>();
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (_collapsible != null)  _collapsible.TriggerRestore();
                _boxSolidCollider = GetComponent<Collider2D>();
            }
        }
        
    }
}