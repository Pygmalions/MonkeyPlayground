using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace MonkeyPlayground.Data;

public record struct FloorData()
{
    /// <summary>
    /// X position of the left end of this floor.
    /// </summary>
    public required int LeftEndX { get; init; }
    
    /// <summary>
    /// X position of the right end of this floor.
    /// </summary>
    public required int RightEndX { get; init; }
    
    /// <summary>
    /// Y position of this floor.
    /// </summary>
    public required int Y { get; init; }

    [SetsRequiredMembers]
    public FloorData(Transform transform) : this()
    {
        LeftEndX = (int)transform.position.x;
        RightEndX = (int)(transform.position.x + transform.localScale.x);
        Y = (int)transform.position.y;
    }
}