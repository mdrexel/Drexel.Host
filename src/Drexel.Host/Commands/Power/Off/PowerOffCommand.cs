using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Power.Off
{
    /// <summary>
    /// Performs a power-off operation.
    /// </summary>
    internal sealed class PowerOffCommand : Command<PowerOffCommand.Options, PowerOffCommand.Handler>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PowerOffCommand"/> class.
        /// </summary>
        public PowerOffCommand()
            : base("off", "Performs a power-off operation.")
        {
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

                if (OperatingSystem.IsWindows())
                {
                    return await ShutdownWindowsAsync();
                }
                else if (OperatingSystem.IsLinux())
                {
                    return await ShutdownLinuxAsync();
                }
                else
                {
                    throw new PlatformNotSupportedException(
                        "The current platform does not have a shutdown implementation defined.");
                }
            }

            [SupportedOSPlatform("windows")]
            private static async Task<int> ShutdownWindowsAsync()
            {
                throw new NotImplementedException();
            }

            [SupportedOSPlatform("linux")]
            private static async Task<int> ShutdownLinuxAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}
