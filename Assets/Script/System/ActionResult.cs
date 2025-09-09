using JetBrains.Annotations;

public readonly struct ActionResult
{
    public bool Succeeded { get; }
    
    [CanBeNull] 
    public string Message { get; }

    private ActionResult(bool succeeded, [CanBeNull] string message)
    {
        Succeeded = succeeded;
        Message = message;
    }
    
    public static ActionResult Success([CanBeNull] string message = null)
    {
        return new ActionResult(true, message);
    }
    
    public static ActionResult Failure([CanBeNull] string message = null)
    {
        return new ActionResult(false, message);
    }
}
