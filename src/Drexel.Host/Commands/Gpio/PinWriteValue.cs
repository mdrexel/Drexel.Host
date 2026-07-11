using System;
using System.Device.Gpio;

namespace Drexel.Host.Commands.Gpio;

internal enum PinWriteValue
{
    Low,
    High,
}

internal static class PinWriteValueExtensions
{
    extension(PinWriteValue value)
    {
        public PinWriteValue Invert() =>
            value switch
            {
                PinWriteValue.High => PinWriteValue.Low,
                PinWriteValue.Low => PinWriteValue.High,
                _ => throw new InvalidOperationException("Unrecognized pin write value.");
            };

        public PinValue ToPinValue() =>
            value switch
            {
                PinWriteValue.Low => PinValue.Low,
                PinWriteValue.High => PinValue.High,
                _ => throw new InvalidOperationException("Unrecognized pin write value."),
            };
    }
}