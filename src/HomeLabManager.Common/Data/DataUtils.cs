using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace HomeLabManager.Common.Data;

/// <summary>
/// Data related utility methods.
/// </summary>
public static class DataUtils
{
    /// <summary>
    /// Create the default YAML serializer.
    /// </summary>
    public static ISerializer CreateBasicYamlSerializer() =>
        new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

    /// <summary>
    /// Create the default YAML deserializer.
    /// </summary>
    public static IDeserializer CreateBasicYamlDeserializer() =>
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
}
