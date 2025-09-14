using UnityEngine;

namespace MonkeyPlayground.Components
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class GraspableFeature : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;

        private RigidbodyType2D _originalBodyType;
        private int _originalSortingOrder;
        private Color _originalColor;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;
        }

        public void OnPickup()
        {
            _originalBodyType = _rigidbody.bodyType;
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.linearVelocity = Vector2.zero;
            _collider.enabled = false;

            _originalSortingOrder = _spriteRenderer.sortingOrder;
            _spriteRenderer.sortingOrder = 100;
        }

        public void OnDrop()
        {
            _rigidbody.bodyType = _originalBodyType;
            _collider.enabled = true;
            _spriteRenderer.sortingOrder = _originalSortingOrder;
            _spriteRenderer.color = _originalColor;
        }
        
    }
}