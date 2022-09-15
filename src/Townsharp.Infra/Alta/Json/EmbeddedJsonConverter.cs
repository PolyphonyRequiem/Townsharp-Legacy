using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Townsharp.Infra.Alta.Json
{
    public class EmbeddedJsonConverter<TJson> : JsonConverter<TJson>
    {
        public override TJson Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string jsonData = reader.GetString()!;
            var jsonObject = System.Text.Json.JsonSerializer.Deserialize<TJson>(jsonData, options);
            return jsonObject!;
        }

        public override void Write(Utf8JsonWriter writer, TJson value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
