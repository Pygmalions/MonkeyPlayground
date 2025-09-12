namespace MonkeyPlayground.Data;

public record struct SceneData
{
    /// <summary>
    /// Indicates whether the game is completed.
    /// If the monkey has reached the banana, it will be true;
    /// otherwise false.
    /// </summary>
    public required bool IsCompleted { get; init; }
    
    public required MonkeyData Monkey { get; init; }
    
    public required ItemData[] Items { get; init; }
    
    public required FloorData[] Floors { get; init; }
}