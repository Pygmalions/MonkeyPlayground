namespace MonkeyPlayground.Data.Actions;

public class MonkeyMovingAction : ActionData
{
    public required int GoalPosition { get; init; }

    public override string Name => "Move Monkey";

    public override string Content => "Move this monkey to the goal position.";
}