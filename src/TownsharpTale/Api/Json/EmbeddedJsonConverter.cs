using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection.PortableExecutable;

namespace Townsharp.Api.Json
{
    public class EmbeddedJsonConverter<TJson> : JsonConverter<TJson>
    {
        public override TJson Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string jsonData = reader.GetString()!;
            var jsonObject = JsonSerializer.Deserialize<TJson>(jsonData, options);
            return jsonObject!;
        }

        public override void Write(Utf8JsonWriter writer, TJson value, JsonSerializerOptions options)
        {
            var jsonValue = JsonSerializer.Serialize(value, options);
            writer.WriteStringValue(jsonValue);
        }
    }
}
