using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Gpio.Serve
{
    internal sealed class GpioServeCommand : Command<GpioServeCommand.Options, GpioServeCommand.Handler>
    {
        // MAX TODO: What's the right abstraction for hosting the pin state? Because we want debounce logic and
        // ignore-if-says-no-power-during-bringup stuff; does that get baked into the consumer rather than the host? We
        // just serve raw GPIO pin values, and it's up to the logic on the client-side to decide how to handle them?
        // Reason that could matter is for BSD, since I dont know if we'll be able to run C# there yet (and so I'd have
        // to do crappy bash stuff)

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioServeCommand"/> class.
        /// </summary>
        public GpioServeCommand()
            : base("serve", "Starts an HTTP server for interacting with GPIO pins.")
        {
            ////Add(PinOption);
            ////Add(ModeOption);
        }

        /// <summary>
        /// The options associated with performing the command.
        /// </summary>
        public new sealed class Options
        {
            /// <summary>
            /// Gets the numeric identifier of the pin.
            /// </summary>
            public int Pin { get; init; }

            /// <summary>
            /// Gets the mode to use when reading the value.
            /// </summary>
            public PinReadMode Mode { get; init; }
        }

        /// <summary>
        /// The command implementation.
        /// </summary>
        /// <param name="console">
        /// The console to use.
        /// </param>
        public new sealed class Handler(IAnsiConsole console) : ICommandHandler<Options, Handler>
        {
            /// <inheritdoc/>
            public async Task<int> HandleAsync(Options options, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using GpioController controller = new();
                GpioPin pin = controller.OpenPin(options.Pin, options.Mode.ToPinMode());
                PinValue value = pin.Read();
                console.WriteLine(((int)value).ToString());

                return ExitCode.Success;
            }
        }
    }
}
