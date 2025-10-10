using MonkeyPlayground.Models;
using UnityEngine;

namespace MonkeyPlayground.Objects
{
    [DisallowMultipleComponent, RequireComponent(typeof(EdgeCollider2D))]
    public class Floor : PerceptibleObject<FloorData>
    {
        private EdgeCollider2D _collider;
        private FloorData _data;
        private SpriteRenderer _sprite;
        
        private void Start()
        {
            _collider = GetComponent<EdgeCollider2D>();
            _sprite = GetComponent<SpriteRenderer>();
            if (_sprite)
                _sprite.enabled = false;
        }

        private void Update()
        {
            _data = new FloorData
            {
                LeftEndX = _collider.bounds.center.x - _collider.bounds.extents.x,
                RightEndX = _collider.bounds.center.x + _collider.bounds.extents.x,
                Y = _collider.bounds.max.y
            };
        }

        protected override FloorData OnGenerateData()
        {
            return _data;
        }
    }
}