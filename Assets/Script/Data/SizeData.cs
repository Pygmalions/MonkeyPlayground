using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace MonkeyPlayground.Data
{
    public record struct SizeData()
    {
        public required int Width { get; init; }

		public required int Height { get; init; }

        [SetsRequiredMembers]
        public SizeData(Transform transform) : this()
        {
            Width = (int)transform.localScale.x;
            Height = (int)transform.localScale.y;
        }
    }
}