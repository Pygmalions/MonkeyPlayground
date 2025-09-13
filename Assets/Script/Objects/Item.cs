using System.Collections.Generic;
using System.Threading;
using MonkeyPlayground.Data;
using UnityEngine;

namespace MonkeyPlayground.Objects
{
    [DisallowMultipleComponent]
    public abstract class Item : PerceptibleObject<ItemData>
    {
        private static int _id;
        
        [SerializeField] public string itemName;
        [SerializeField] public string itemDescription;
        [SerializeField] public List<string> itemFunctions = new();
        [SerializeField] public int itemId = 0;
        
        private PositionData _position;
        private SizeData _size;

        private void Reset()
        {
            itemId = Interlocked.Increment(ref _id);
        }

        protected virtual void Update()
        {
            _position = new PositionData(transform);
            _size = new SizeData(transform);
        }
        
        protected override ItemData OnGenerateData()
        {
            return new ItemData
            {
                Name = itemName,
                Id = itemId,
                Description = itemDescription,
                Size = _size,
                Position = _position,
                Functions = itemFunctions
            };
        }
    }
}