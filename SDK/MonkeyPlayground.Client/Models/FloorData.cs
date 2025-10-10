namespace MonkeyPlayground.Client.Models;

public record struct FloorData()
{
    /// <summary>
    /// X position of the left end of this floor.
    /// </summary>
    public required float LeftEndX { get; init; }
    
    /// <summary>
    /// X position of the right end of this floor.
    /// </summary>
    public required float RightEndX { get; init; }
    
    /// <summary>
    /// Y position of this floor.
    /// </summary>
    public required float Y { get; init; }
}