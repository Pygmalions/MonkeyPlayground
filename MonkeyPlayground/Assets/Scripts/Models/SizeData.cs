using System.Text.Json.Serialization;
using RestServer.Runtime.Helper;

namespace MonkeyPlayground.Models
{
    public record struct SizeData
    {
        [JsonConverter(typeof(TwoDecimalFloatConverter))]
        public required float Width { get; init; }
        
        [JsonConverter(typeof(TwoDecimalFloatConverter))]
		public required float Height { get; init; }
    }
}