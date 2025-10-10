namespace MonkeyPlayground.Client.Models;

public record struct SizeData()
{
    public required float Width { get; init; }

    public required float Height { get; init; }
}