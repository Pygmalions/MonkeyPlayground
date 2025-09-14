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
    /// Current status of this action.
    /// </summary>
    public ActionResult Result { get; set; } = ActionResult.Pending();
}

public static class ActionDataExtensions
{
    /// <summary>
    /// Check whether this action is completed (succeeded, failed, or cancelled).
    /// </summary>
    /// <returns>
    /// True if this action is completed (succeeded, failed, or cancelled); otherwise, false.
    /// </returns>
    public static bool IsCompleted(this ActionData data)
        => data.Result.Status != ActionStatus.Pending && data.Result.Status != ActionStatus.Running;
}