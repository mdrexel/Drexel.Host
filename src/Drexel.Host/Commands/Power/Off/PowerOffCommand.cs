﻿using System;
using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Microsoft.Win32.SafeHandles;
using Spectre.Console;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
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
            public Reason Reason { get; init; }

            /// <summary>
            /// Gets a value indicating whether the power-off operation should be forced.
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
            private async Task<int> ShutdownWindowsAsync(
                Options options,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                EXIT_WINDOWS_FLAGS mode = options.Force
                    ? EXIT_WINDOWS_FLAGS.EWX_POWEROFF | EXIT_WINDOWS_FLAGS.EWX_FORCE
                    : EXIT_WINDOWS_FLAGS.EWX_POWEROFF;
                SHUTDOWN_REASON reason = Convert(options.Reason);

                const short TOKEN_ADJUST_PRIVILEGES = 32;
                const short TOKEN_QUERY = 8;
                if (0 == OpenProcessToken(
                    Process.GetCurrentProcess().Handle,
                    TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY,
                    out IntPtr hToken))
                {
                    console.WriteException(
                        new Exception(Marshal.GetLastPInvokeErrorMessage()),
                        ExceptionFormats.NoStackTrace);
                    return Marshal.GetLastPInvokeError();
                }

                if (0 == LookupPrivilegeValue("", "SeShutdownPrivilege", out LUID lpLuid))
                {
                    console.WriteException(
                        new Exception(Marshal.GetLastPInvokeErrorMessage()),
                        ExceptionFormats.NoStackTrace);
                    return Marshal.GetLastPInvokeError();
                }

                const short SE_PRIVILEGE_ENABLED = 2;
                TOKEN_PRIVILEGES tkp =
                    new()
                    {
                        PrivilegeCount = 1,
                        Privileges =
                            {
                                pLuid = lpLuid,
                                Attributes = SE_PRIVILEGE_ENABLED,
                            },
                    };

                if (!AdjustTokenPrivileges(
                    hToken,
                    false,
                    ref tkp,
                    0U,
                    IntPtr.Zero,
                    IntPtr.Zero))
                {
                    console.WriteException(
                        new Exception(Marshal.GetLastPInvokeErrorMessage()),
                        ExceptionFormats.NoStackTrace);
                    return Marshal.GetLastPInvokeError();
                }

                if (options.WhatIf)
                {
                    return 0;
                }

                // For whatever reason, Microsoft decided that zero means failed and non-zero means succeeded. We
                // need to check what the actual error was.
                return PInvoke.ExitWindowsEx(mode, reason) == 0
                    ? Marshal.GetLastWin32Error()
                    : 0;

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
            private async Task<int> ShutdownLinuxAsync(
                Options options,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                throw new NotImplementedException();
            }

            [DllImport("advapi32.dll")]
            static extern int OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool AdjustTokenPrivileges(
                IntPtr TokenHandle,
                [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
                ref TOKEN_PRIVILEGES NewState,
                UInt32 BufferLength,
                IntPtr PreviousState,
                IntPtr ReturnLength);

            [DllImport("advapi32.dll")]
            static extern int LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

            private struct LUID
            {
                public int LowPart;
                public int HighPart;
            }

            private struct LUID_AND_ATTRIBUTES
            {
                public LUID pLuid;
                public int Attributes;
            }

            private struct TOKEN_PRIVILEGES
            {
                public int PrivilegeCount;
                public LUID_AND_ATTRIBUTES Privileges;
            }
        }
    }
}
