using System.Text.Json;

namespace Townsharp.Infra.Alta.Json
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => JsonUtils.ToSnakeCase(name);
    }
}
