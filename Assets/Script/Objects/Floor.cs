using System;
using MonkeyPlayground.Data;
using UnityEngine;

namespace MonkeyPlayground.Objects
{
    [DisallowMultipleComponent]
    public class Floor : PerceptibleObject<FloorData>
    {
        private int _leftEndX;
        private int _rightEndX;
        private int _y;

        private void Update()
        {
            var position = transform.position;
            var size = transform.localScale;
            _leftEndX = (int)(position.x - size.x / 2.0f);
            _rightEndX = (int)(position.x + size.x / 2.0f);
            _y = (int)MathF.Ceiling(position.y + size.y / 2.0f);
        }
        
        protected override FloorData OnGenerateData()
        {
            return new FloorData
            {
                LeftEndX = _leftEndX,
                RightEndX = _rightEndX,
                Y = _y
            };
        }
    }
}