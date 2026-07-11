using System;
using System.Device.Gpio;

namespace Drexel.Host.Commands.Gpio;

/// <summary>
/// Represents the pin mode to read a GPIO value with.
/// </summary>
internal readonly record struct PinWriteValue
{
    private readonly UnderlyingValue _value;

    private PinWriteValue(UnderlyingValue value)
    {
        _value = value;
    }

    private enum UnderlyingValue
    {
        Low,
        High,
    }

    /// <summary>
    /// Gets a <see cref="PinWriteValue"/> instance that indicates the value should be set to Low.
    /// </summary>
    public static PinWriteValue Low { get; } = new(UnderlyingValue.Low);

    /// <summary>
    /// Gets a <see cref="PinReadMode"/> instance that indicates the value should be set to High.
    /// resistor mode.
    /// </summary>
    public static PinWriteValue High { get; } = new(UnderlyingValue.High);

    /// <summary>
    /// Converts this instance to the corresponding <see cref="PinValue"/>.
    /// </summary>
    /// <returns>
    /// The corresponding <see cref="PinValue"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when this instance cannot be mapped to <see cref="PinValue"/>.
    /// </exception>
    public PinValue ToPinValue() =>
        _value switch
        {
            UnderlyingValue.Low => PinValue.Low,
            UnderlyingValue.High => PinValue.High,
            _ => throw new NotSupportedException("The specified pin value is not supported."),
        };
}