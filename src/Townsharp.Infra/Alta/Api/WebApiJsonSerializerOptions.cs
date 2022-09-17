using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Townsharp.Infra.Alta.Json;

namespace Townsharp.Infra.Alta.Api
{
    internal class WebApiJsonSerializerOptions
    {
        public static JsonSerializerOptions Default = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNamingPolicy = new SnakeCaseNamingPolicy()};
    }
}
