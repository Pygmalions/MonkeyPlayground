namespace MonkeyPlayground.Client.Models.Actions;

public class MonkeyMoveAction : ActionData
{
    public override string Name => "Move";

    public override string Content => "Move this monkey to the goal position.";
}