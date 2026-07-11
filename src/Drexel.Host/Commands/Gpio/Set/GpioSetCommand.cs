using System.CommandLine;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Gpio.Set;

/// <summary>
/// Sets the value of a given pin.
/// </summary>
internal sealed class GpioSetCommand : Command<GpioSetCommand.Options, GpioSetCommand.Handler>
{
    private static Option<int> PinOption { get; } =
        new(["--pin", "-p"], "The numeric pin number to set the value of.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };

    private static Option<PinWriteValue> ValueOption { get; } =
        new(["--value", "-v"], "The value to write.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };

    private static Option<int> DurationOption { get; } =
        new(["--duration", "-d"], "The number of milliseconds for which the value should be set.")
        {
            IsRequired = false,
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="GpioSetCommand"/> class.
    /// </summary>
    public GpioSetCommand()
        : base("set", "Writes to a GPIO pin.")
    {
        Add(PinOption);
        Add(ValueOption);
        Add(DurationOption);
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
        /// Gets the value to write.
        /// </summary>
        public PinWriteValue Value { get; init; }

        /// <summary>
        /// Gets the number of milliseconds for which the value should be set.
        /// </summary>
        public int? Duration { get; init; }
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
            using GpioPin pin = controller.OpenPin(options.Pin, PinMode.Output);
            PinValue value = options.Value.ToPinValue();
            pin.Write(value);

            if (options.Duration is not null)
            {
                console.WriteLine($"Waiting for {options.Duration} milliseconds...");
                Thread.Sleep(options.Duration.Value);
                pin.Write(options.Value.Invert().ToPinValue());
            }

            return ExitCode.Success;
        }
    }
}