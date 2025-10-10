namespace MonkeyPlayground.Client.Models
{
    public record struct MonkeyData
    {
        /// <summary>
        /// Name of this monkey.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Current position of this monkey.
        /// </summary>
        public required PositionData Position { get; init; }
        
        /// <summary>
        /// Size of this monkey.
        /// </summary>
        public required SizeData Size { get; init; }
        
        /// <summary>
        /// Item that this monkey is currently holding, null if none.
        /// </summary>
        public ItemData? HoldingItem { get; init; }
    }
}