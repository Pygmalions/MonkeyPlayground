using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestServer.Runtime.Helper
{
    public class TwoDecimalFloatConverter : JsonConverter<float>
    {
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetSingle();
        }

        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(value.ToString("F2"));
        }
    }
}