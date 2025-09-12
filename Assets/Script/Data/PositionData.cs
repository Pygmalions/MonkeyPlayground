using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace MonkeyPlayground.Data
{
    public record struct PositionData()
    {
        public required int X { get; init; }

        public required int Y { get; init; }

        [SetsRequiredMembers]
        public PositionData(Transform transform) : this()
        {
            X = (int)transform.position.x;
            Y = (int)transform.position.y;
        }
    }
}