using System.Collections.Generic;
using MonkeyPlayground.Data;
using UnityEngine;

namespace MonkeyPlayground.Objects
{
    [DisallowMultipleComponent]
    public abstract class Item : PerceptibleObject<ItemData>
    {
        [SerializeField] public string itemName;
        [SerializeField] public string itemDescription;
        [SerializeField] public List<string> itemFunctions = new();

        private PositionData _position;
        private SizeData _size;

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
                Description = itemDescription,
                Size = _size,
                Position = _position,
                Functions = itemFunctions
            };
        }
    }
}