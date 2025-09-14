using System;
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
        
        private PositionData _latestPosition;
        private SizeData _latestSize;

        private void Reset()
        {
            itemId = Interlocked.Increment(ref _id);
        }

        protected virtual void Update()
        {
            _latestPosition = new PositionData
            {
                X = (int)MathF.Round(transform.position.x + 0.5f),
                Y = (int)MathF.Round(transform.position.y),
            };
            _latestSize = new SizeData(transform);
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