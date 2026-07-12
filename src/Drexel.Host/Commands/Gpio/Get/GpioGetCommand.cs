using System.CommandLine;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Gpio.Get;

/// <summary>
/// Gets the value of a given pin.
/// </summary>
internal sealed class GpioGetCommand : Command<GpioGetCommand.Options, GpioGetCommand.Handler>
{
    private static Option<int> PinOption { get; } =
        new(["--pin", "-p"], "The numeric pin number to get the value of.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true,
        };

    private static Option<PinReadMode> ModeOption { get; } =
        new(["--mode", "-m"], () => PinReadMode.Default, "The mode to use when reading the value of the pin.")
        {
            Arity = ArgumentArity.ExactlyOne,
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="GpioGetCommand"/> class.
    /// </summary>
    public GpioGetCommand()
        : base("get", "Reads from a GPIO pin.")
    {
        Add(PinOption);
        Add(ModeOption);
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
            using GpioPin pin = controller.OpenPin(options.Pin, options.Mode.ToPinMode());
            PinValue value = pin.Read();
            console.WriteLine(((int)value).ToString());

            return ExitCode.Success;
        }
    }
}