using System.Collections.Generic;

namespace MonkeyPlayground.Data
{
    public record struct ItemData
    {
        /// <summary>
        /// Name of this item.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Unique identifier of this item.
        /// </summary>
        public required int Id { get; init; }
        
        /// <summary>
        /// Description of this item.
        /// </summary>
        public required string Description { get; init; }
        
        /// <summary>
        /// Current position of this item.
        /// </summary>
        public required PositionData Position { get; init; }

        /// <summary>
        /// Size of this item.
        /// </summary>
        public required SizeData Size { get; init; }

        /// <summary>
        /// Functions that this item can be performed with.
        /// </summary>
        public required IReadOnlyCollection<string> Functions { get; init; }
    }
}