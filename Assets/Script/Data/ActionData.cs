namespace MonkeyPlayground.Data;

public abstract class ActionData
{
    /// <summary>
    /// Id of this action.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Name of this action.
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// The content of this action, describing what this action does in detail.
    /// </summary>
    public abstract string Content { get; }
    
    /// <summary>
    /// Message of this action, describing what this action does.
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// Current status of this action.
    /// </summary>
    public ActionResult Result { get; set; } = ActionResult.Running();
}