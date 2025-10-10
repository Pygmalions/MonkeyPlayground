using System.Collections.Generic;
using System.Threading;
using MonkeyPlayground.Models;
using UnityEngine;

namespace MonkeyPlayground.Objects
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D))]
    public abstract class Item : PerceptibleObject<ItemData>
    {
        private static int _id;
        
        [SerializeField] public string itemName;
        [SerializeField] public string itemDescription;
        [SerializeField] public List<string> itemFunctions = new();
        [SerializeField] public int itemId = 0;
        
        protected BoxCollider2D Collider;
        private PositionData _latestPosition;
        private SizeData _latestSize;

        protected virtual void Awake()
        {
            Collider = GetComponent<BoxCollider2D>();
        }

        protected virtual void Reset()
        {
            itemId = Interlocked.Increment(ref _id);
        }

        protected virtual void Update()
        {
            _latestPosition = new PositionData
            {
                X = Collider.bounds.center.x,
                Y = Collider.bounds.center.y - Collider.bounds.extents.y,
            };
            _latestSize = new SizeData
            {
                Width = Collider.size.x,
                Height = Collider.size.y,
            };
        }
        
        protected override ItemData OnGenerateData()
        {
            return new ItemData
            {
                Name = itemName,
                Id = itemId,
                Description = itemDescription,
                Size = _latestSize,
                Position = _latestPosition,
                Functions = itemFunctions
            };
        }
    }
}

