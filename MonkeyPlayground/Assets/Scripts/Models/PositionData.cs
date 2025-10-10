using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using RestServer.Runtime.Helper;
using UnityEngine;

namespace MonkeyPlayground.Models
{
    public record struct PositionData()
    {
        [JsonConverter(typeof(TwoDecimalFloatConverter))]
        public required float X { get; init; }

        [JsonConverter(typeof(TwoDecimalFloatConverter))]
        public required float Y { get; init; }

        [SetsRequiredMembers]
        public PositionData(Transform transform) : this()
        {
            X = transform.position.x;
            Y = transform.position.y;
        }
    }
}