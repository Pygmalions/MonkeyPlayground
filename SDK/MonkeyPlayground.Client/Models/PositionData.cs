namespace MonkeyPlayground.Client.Models;

public record struct PositionData()
{
    public required float X { get; init; }

    public required float Y { get; init; }
}