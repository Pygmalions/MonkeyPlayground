using System.Text.Json.Serialization;

namespace MonkeyPlayground.Client.Models
{
    public class ActionResult
    {
        public required string Message { get; init; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required ActionStatus Status { get; init; }

        public static ActionResult Pending(string? message = null) => new()
        {
            Message = message ?? "This action is waiting to be processed.",
            Status = ActionStatus.Pending
        };
        
        public static ActionResult Running(string? message = null) => new()
        {
            Message = message ?? "This action is currently running.",
            Status = ActionStatus.Running
        };

        public static ActionResult Succeeded(string? message = null) => new()
        {
            Message = message ?? "This action has succeeded.",
            Status = ActionStatus.Succeeded
        };

        public static ActionResult Failed(string? message = null) => new()
        {
            Message = message ?? "This action has failed.",
            Status = ActionStatus.Failed
        };
    
        public static ActionResult Cancelled(string? message = null) => new()
        {
            Message = message ?? "This action is cancelled for some reasons.",
            Status = ActionStatus.Cancelled
        };
    }
}
