using System.CommandLine;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Gpio.Get
{
    /// <summary>
    /// Performs a power-off operation.
    /// </summary>
    internal sealed class GpioGetCommand : Command<GpioGetCommand.Options, GpioGetCommand.Handler>
    {
        ////private static Option<Reason> ReasonOption { get; } =
        ////    new(["--reason", "-r"], () => Reason.None, "The reason for the power-off operation.")
        ////    {
        ////        Arity = ArgumentArity.ZeroOrOne,
        ////    };

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioGetCommand"/> class.
        /// </summary>
        public GpioGetCommand()
            : base("get", "Reads from a GPIO pin.")
        {
            ////Add(WhatIf);
        }

        /// <summary>
        /// The options associated with performing the command.
        /// </summary>
        public new sealed class Options
        {
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

                GpioController controller = new();
                PinValue value = controller.Read(12);
                console.WriteLine(((int)value).ToString());

                return ExitCode.Success;
            }
        }
    }
}
