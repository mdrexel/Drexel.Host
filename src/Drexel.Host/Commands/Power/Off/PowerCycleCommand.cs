using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Power.Off
{
    /// <summary>
    /// Performs a power-cycle operation.
    /// </summary>
    internal sealed class PowerCycleCommand : Command<PowerCycleCommand.Options, PowerCycleCommand.Handler>
    {
        private static Option<PowerOffReason> ReasonOption { get; } =
            new(["--reason", "-r"], () => PowerOffReason.None, "The reason for the power-cycle operation.")
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
        /// Initializes a new instance of the <see cref="PowerCycleCommand"/> class.
        /// </summary>
        public PowerCycleCommand()
            : base("cycle", "Performs a power-cycle operation.")
        {
            Add(ReasonOption);
            Add(ForceOption);
            Add(WhatIf);
        }

        /// <summary>
        /// The options associated with performing the command.
        /// </summary>
        public new sealed class Options
        {
            /// <summary>
            /// Gets a value indicating the reason for the power-cycle operation.
            /// </summary>
            public PowerOffReason PowerOffReason { get; init; }

            /// <summary>
            /// Gets a value indicating whether the power-cycle operation should be forced.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if the operation should be forced; otherwise, <see langword="false"/>.
            /// </value>
            public bool Force { get; init; }

            /// <summary>
            /// Gets a value indicating whether a simulation of the operation which has no side-effects should be
            /// performed.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if a simulation of the operation should be performed; otherwise,
            /// <see langword="false"/>.
            /// </value>
            public bool WhatIf { get; init; }
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
                    return await new Windows(console).RebootAsync(
                        options.PowerOffReason,
                        options.Force,
                        options.WhatIf,
                        cancellationToken);
                }
                else if (OperatingSystem.IsLinux())
                {
                    return await new Linux(console).RebootAsync(
                        options.PowerOffReason,
                        options.Force,
                        options.WhatIf,
                        cancellationToken);
                }
                else
                {
                    throw new PlatformNotSupportedException(
                        "A power-cycle implementation has not been defined for this platform.");
                }
            }
        }
    }
}
