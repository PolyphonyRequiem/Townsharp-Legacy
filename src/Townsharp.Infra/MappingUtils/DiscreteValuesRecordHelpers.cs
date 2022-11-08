using System.Reflection;

namespace Townsharp.Infra.MappingUtils
{
    internal class DiscreteValuesRecordHelpers
    {
        internal static Dictionary<string, T> GetStaticMappings<T>(Func<T, string> keyProvider)
        {
            var mappings = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public).Where(_ => _.FieldType == typeof(T));
            return mappings.ToDictionary(_ => GetKey(_, keyProvider), GetValue<T>);
        }

        private static string GetKey<T>(FieldInfo field, Func<T, string> keyProvider)
        {
            string value = keyProvider((T)field.GetValue(null)!);
            return value;
        }

        private static T GetValue<T>(FieldInfo field)
        {
            T value = (T)field.GetValue(null)!;
            return value;
        }

    }
}
