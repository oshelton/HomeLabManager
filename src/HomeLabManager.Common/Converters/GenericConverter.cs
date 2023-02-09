using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace HomeLabManager.Common.Converters;

/// <summary>
/// Class for simple converters that can be implemented via a function passed in.
/// </summary>
/// <typeparam name="TValue">Desired type of the value to convert.</typeparam>
/// <typeparam name="TParameter">Desired typ of the conversion parameter.</typeparam>
public class GenericConverter<TValue, TParameter> : IValueConverter
{
	public GenericConverter(Func<TValue?, TParameter?, object?> converterFunction, bool passThroughNull = false, object? defaultValue = null)
	{
		_converterFunction = converterFunction;

		_passThroughNull = passThroughNull;
		if (defaultValue is null)
			_defaultValue = AvaloniaProperty.UnsetValue;
		else
			_defaultValue = defaultValue;
	}

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if ((value is TValue || (value is null && _passThroughNull)) && (parameter is null || parameter is TParameter))
            return _converterFunction((TValue?) value, (TParameter?) parameter);
		else
			return _defaultValue;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

	private readonly Func<TValue?, TParameter?, object?> _converterFunction;
	private readonly bool _passThroughNull;
	private readonly object _defaultValue;
}
