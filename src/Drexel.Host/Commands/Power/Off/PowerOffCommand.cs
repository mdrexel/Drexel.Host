using System;
using System.CommandLine;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;
using Windows.Win32;
using Windows.Win32.System.Shutdown;

namespace Drexel.Host.Commands.Power.Off
{
    /// <summary>
    /// Performs a power-off operation.
    /// </summary>
    internal sealed class PowerOffCommand : Command<PowerOffCommand.Options, PowerOffCommand.Handler>
    {
        private static Option<Reason> ReasonOption { get; } =
            new(["--reason", "-r"], () => Reason.None, "The reason for the power-off operation.")
            {
                Arity = ArgumentArity.ZeroOrOne,
            };

        private static Option<bool> ForceOption { get; } =
            new(["--force", "-f"], () => false, "Whether the operation should be forced.")
            {
                Arity = ArgumentArity.Zero,
            };

        private static Option<bool> WhatIf { get; } =
            new(["--what-if"], "Performs a simulation of the operation which has no side-effects.")
            {
                Arity = ArgumentArity.Zero,
                IsHidden = true,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="PowerOffCommand"/> class.
        /// </summary>
        public PowerOffCommand()
            : base("off", "Performs a power-off operation.")
        {
            Add(ReasonOption);
            Add(ForceOption);
            Add(WhatIf);
        }

        /// <summary>
        /// Represents the reason for the power-off operation.
        /// </summary>
        public enum Reason
        {
            /// <summary>
            /// Indicates that a reason was not provided.
            /// </summary>
            None,

            /// <summary>
            /// Indicates that the reason is a power failure.
            /// </summary>
            Power,

            /// <summary>
            /// Indicates that the reason is a software failure.
            /// </summary>
            Software,

            /// <summary>
            /// Indicates that the reason is a hardware failure.
            /// </summary>
            Hardware,
        }

        /// <summary>
        /// The options associated with performing the command.
        /// </summary>
        public new sealed class Options
        {
            /// <summary>
            /// Gets a value indicating the reason for the power-off operation.
            /// </summary>
            public Reason Reason { get; }

            /// <summary>
            /// Gets a value indicating whether the power-off operation should be forced.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if the operation should be forced; otherwise, <see langword="false"/>.
            /// </value>
            public bool Force { get; }

            /// <summary>
            /// Gets a value indicating whether a simulation of the operation which has no side-effects should be
            /// performed.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if a simulation of the operation should be performed; otherwise,
            /// <see langword="false"/>.
            /// </value>
            public bool WhatIf { get; }
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

                if (OperatingSystem.IsWindowsVersionAtLeast(5, 1, 2600))
                {
                    return await ShutdownWindowsAsync(options, cancellationToken);
                }
                else if (OperatingSystem.IsLinux())
                {
                    return await ShutdownLinuxAsync(options, cancellationToken);
                }
                else
                {
                    throw new PlatformNotSupportedException(
                        "A power-off implementation has not been defined for this platform.");
                }
            }

            [SupportedOSPlatform("windows5.1.2600")]
            private static async Task<int> ShutdownWindowsAsync(
                Options options,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                EXIT_WINDOWS_FLAGS mode = options.Force
                    ? EXIT_WINDOWS_FLAGS.EWX_POWEROFF | EXIT_WINDOWS_FLAGS.EWX_FORCE
                    : EXIT_WINDOWS_FLAGS.EWX_POWEROFF;
                SHUTDOWN_REASON reason = Convert(options.Reason);

                if (options.WhatIf)
                {
                    return 0;
                }
                else
                {
                    int result = PInvoke.ExitWindowsEx(mode, reason);
                    return result;
                }

                static SHUTDOWN_REASON Convert(Reason reason) =>
                    reason switch
                    {
                        Reason.None => SHUTDOWN_REASON.SHTDN_REASON_NONE,
                        Reason.Power => SHUTDOWN_REASON.SHTDN_REASON_MAJOR_POWER,
                        Reason.Software => SHUTDOWN_REASON.SHTDN_REASON_MAJOR_SOFTWARE,
                        Reason.Hardware => SHUTDOWN_REASON.SHTDN_REASON_MAJOR_HARDWARE,
                        _ => throw new ArgumentException("Unrecognized power-off reason.", nameof(options)),
                    };
            }

            [SupportedOSPlatform("linux")]
            private static async Task<int> ShutdownLinuxAsync(
                Options options,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                throw new NotImplementedException();
            }
        }
    }
}
