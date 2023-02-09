namespace HomeLabManager.Common.Converters;

public static class CommonConverters
{
    /// <summary>
	/// Returns true if the passed in objects are equal.
	/// </summary>
    /// <remarks>Null values will always return false.</remarks>
	public static GenericConverter<object, object> ObjectsAreEqual { get; } =
        new GenericConverter<object, object>((value, param) => value!.Equals(param), false, false);
}
