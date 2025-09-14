namespace MonkeyPlayground.Data.Actions;


public class MonkeyDropAction : ActionData
{
    public override string Name => "Drop Item";
    
    public override string Content => "Drop the current item at the same location of the monkey.";
}

