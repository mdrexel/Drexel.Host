using System;
using System.Device.Gpio;

namespace Drexel.Host.Commands.Gpio;

internal enum PinReadMode
{
    Default,
    PullDown,
    PullUp,
}

internal static class PinReadModeExtensions
{
    extension(PinReadMode mode)
    {
        public PinMode ToPinMode() =>
            mode switch
            {
                PinReadMode.Default => PinMode.Input,
                PinReadMode.PullDown => PinMode.InputPullDown,
                PinReadMode.PullUp => PinMode.InputPullUp,
                _ => throw new InvalidOperationException("The specified pin read mode is not recognized."),
            };
    }
}