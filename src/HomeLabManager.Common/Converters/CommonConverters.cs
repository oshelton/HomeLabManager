using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Common.Utils;

namespace HomeLabManager.Common.Converters;

public static class CommonConverters
{
    /// <summary>
	/// Returns true if the passed in objects are equal.
	/// </summary>
    /// <remarks>Null values will always return false.</remarks>
	public static GenericConverter<object, object> ObjectsAreEqual { get; } =
        new GenericConverter<object, object>((value, param) => value!.Equals(param), false, false);

    /// <summary>
	/// Returns the icon corresponding to the passed in server kind.
	/// </summary>
    /// <remarks>Null values will return a safe default.</remarks>
    public static GenericConverter<ServerKind?, object> ServerKindToIconConverter { get; } =
        new GenericConverter<ServerKind?, object>((value, param) => ServerKindMapper.GetIconKind(value), false, false);

    /// <summary>
	/// Returns the label corresponding to the passed in server kind.
	/// </summary>
    /// <remarks>Null values will return a safe default.</remarks>
    public static GenericConverter<ServerKind?, object> ServerKindToLabelConverter { get; } =
        new GenericConverter<ServerKind?, object>((value, param) => ServerKindMapper.GetLabel(value), false, false);
}
