﻿using CSharpFunctionalExtensions;
using System.Text;
using System.Text.Json;

namespace Townsharp.Api.Json
{
    internal static class JsonUtils
    {
        private enum SeparatedCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }

        public static JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { PropertyNamingPolicy = new SnakeCaseNamingPolicy() };

        public static Maybe<T> Deserialize<T>(string content, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            var result = JsonSerializer.Deserialize<T>(content, jsonSerializerOptions ?? JsonSerializerOptions.Default );
            return result != null ?
                Maybe.From(result):
                Maybe.None;
        }

        public static string Serialize<T>(T content)
        {
            return JsonSerializer.Serialize(content, DefaultSerializerOptions);
        }

        public static string ToSnakeCase(string s) => ToSeparatedCase(s, '_');

        private static string ToSeparatedCase(string s, char separator)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            StringBuilder sb = new StringBuilder();
            SeparatedCaseState state = SeparatedCaseState.Start;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                {
                    if (state != SeparatedCaseState.Start)
                    {
                        state = SeparatedCaseState.NewWord;
                    }
                }
                else if (char.IsUpper(s[i]))
                {
                    switch (state)
                    {
                        case SeparatedCaseState.Upper:
                            bool hasNext = i + 1 < s.Length;
                            if (i > 0 && hasNext)
                            {
                                char nextChar = s[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != separator)
                                {
                                    sb.Append(separator);
                                }
                            }
                            break;
                        case SeparatedCaseState.Lower:
                        case SeparatedCaseState.NewWord:
                            sb.Append(separator);
                            break;
                    }

                    char c;
                    c = char.ToLowerInvariant(s[i]);
                    sb.Append(c);

                    state = SeparatedCaseState.Upper;
                }
                else if (s[i] == separator)
                {
                    sb.Append(separator);
                    state = SeparatedCaseState.Start;
                }
                else
                {
                    if (state == SeparatedCaseState.NewWord)
                    {
                        sb.Append(separator);
                    }

                    sb.Append(s[i]);
                    state = SeparatedCaseState.Lower;
                }
            }

            return sb.ToString();
        }
    }
}