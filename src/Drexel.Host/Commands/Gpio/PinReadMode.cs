using System;
using System.Device.Gpio;

namespace Drexel.Host.Commands.Gpio
{
    /// <summary>
    /// Represents the pin mode to read a GPIO value with.
    /// </summary>
    internal readonly record struct PinReadMode
    {
        private readonly UnderlyingMode _mode;

        private PinReadMode(UnderlyingMode mode)
        {
            _mode = mode;
        }

        private enum UnderlyingMode
        {
            Default,
            PullDown,
            PullUp,
        }

        /// <summary>
        /// Gets a <see cref="PinReadMode"/> instance that indicates the value should be read using the default mode.
        /// </summary>
        public static PinReadMode Default { get; } = new(UnderlyingMode.Default);

        /// <summary>
        /// Gets a <see cref="PinReadMode"/> instance that indicates the value should be read using the pull-down
        /// resistor mode.
        /// </summary>
        public static PinReadMode PullDown { get; } = new(UnderlyingMode.PullDown);

        /// <summary>
        /// Gets a <see cref="PinReadMode"/> instance that indicates the value should be read using the pull-up
        /// resistor mode.
        /// </summary>
        public static PinReadMode PullUp { get; } = new(UnderlyingMode.PullUp);

        /// <summary>
        /// Converts this instance to the corresponding <see cref="PinMode"/>.
        /// </summary>
        /// <returns>
        /// The corresponding <see cref="PinMode"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when this instance cannot be mapped to <see cref="PinMode"/>.
        /// </exception>
        public PinMode ToPinMode() =>
            _mode switch
            {
                UnderlyingMode.Default => PinMode.Input,
                UnderlyingMode.PullDown => PinMode.InputPullDown,
                UnderlyingMode.PullUp => PinMode.InputPullUp,
                _ => throw new NotSupportedException("The specified pin mode is not supported."),
            };
    }
}
