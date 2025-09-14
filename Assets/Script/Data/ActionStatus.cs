namespace MonkeyPlayground.Data;

public enum ActionStatus
{
    /// <summary>
    /// This action is waiting to be executed.
    /// </summary>
    Pending,
    /// <summary>
    /// This action is currently being executed.
    /// </summary>
    Running,
    /// <summary>
    /// This action has been executed successfully.
    /// </summary>
    Succeeded,
    /// <summary>
    /// This action has failed during execution.
    /// </summary>
    Failed,
    /// <summary>
    /// This action has been cancelled and will not be executed.
    /// </summary>
    Cancelled
}
