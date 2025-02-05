using System.CommandLine;
using Drexel.Host.Commands.Gpio.Get;

namespace Drexel.Host.Commands.Gpio
{
    /// <summary>
    /// The <c>gpio</c> command root.
    /// </summary>
    public sealed class GpioRoot : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GpioRoot"/> class.
        /// </summary>
        public GpioRoot()
            : base("gpio", "GPIO-related actions.")
        {
            this.Add(new GpioGetCommand());
        }
    }
}
