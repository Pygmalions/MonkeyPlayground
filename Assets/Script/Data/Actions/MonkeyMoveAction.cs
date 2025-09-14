namespace MonkeyPlayground.Data.Actions;

public class MonkeyMoveAction : ActionData
{
    public required int GoalPosition { get; init; }

    public override string Name => "Move";

    public override string Content => "Move this monkey to the goal position.";
}