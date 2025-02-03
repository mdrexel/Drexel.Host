using System;
using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
                    return await new Windows(console).ShutdownAsync(options, cancellationToken);
                }
                else if (OperatingSystem.IsLinux())
                {
                    return await new Linux(console).ShutdownAsync(options, cancellationToken);
                }
                else
                {
                    throw new PlatformNotSupportedException(
                        "A power-off implementation has not been defined for this platform.");
                }
            }

            private sealed class Windows(IAnsiConsole console)
            {
                [SupportedOSPlatform("windows5.1.2600")]
                public async Task<int> ShutdownAsync(
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

                    if (!options.WhatIf)
                    {
                        if (!PInvoke.ExitWindowsEx(mode, reason))
                        {
                            console.WriteException(
                                new Exception(Marshal.GetLastPInvokeErrorMessage()),
                                ExceptionFormats.NoStackTrace);
                            return Marshal.GetLastPInvokeError();
                        }
                    }

                    return 0;

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

                [DllImport("advapi32.dll")]
                private static extern int OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

                [DllImport("advapi32.dll", SetLastError = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                private static extern bool AdjustTokenPrivileges(
                    IntPtr TokenHandle,
                    [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
                    ref TOKEN_PRIVILEGES NewState,
                    UInt32 BufferLength,
                    IntPtr PreviousState,
                    IntPtr ReturnLength);

                [DllImport("advapi32.dll")]
                private static extern int LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

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

            private sealed class Linux(IAnsiConsole console)
            {
                [SupportedOSPlatform("linux")]
                public async Task<int> ShutdownAsync(
                    Options options,
                    CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int result = reboot(LINUX_REBOOT_CMD_POWER_OFF, IntPtr.Zero);
                    if (result != -1)
                    {
                        console.WriteException(
                            new Exception("Unexpected return code."),
                            ExceptionFormats.NoStackTrace);
                        return result;
                    }

                    result = Marshal.GetLastWin32Error();
                    switch (result)
                    {
                        case EPERM:
                            throw new UnauthorizedAccessException("Don't have permissions to reboot.");
                        case EINVAL:
                            throw new InvalidOperationException("Bad input values.");
                        case EFAULT:
                        default:
                            throw new InvalidOperationException("Could not call reboot:" + result.ToString());
                    }
                }

                [DllImport("libc.so", SetLastError = true)]
                public static extern Int32 reboot(Int32 cmd, IntPtr arg);

                public const Int32 LINUX_REBOOT_CMD_RESTART = 0x01234567;
                public const Int32 LINUX_REBOOT_CMD_HALT = unchecked((int)0xCDEF0123);
                public const Int32 LINUX_REBOOT_CMD_CAD_ON = unchecked((int)0x89ABCDEF);
                public const Int32 LINUX_REBOOT_CMD_CAD_OFF = 0x00000000;
                public const Int32 LINUX_REBOOT_CMD_POWER_OFF = 0x4321FEDC;
                public const Int32 LINUX_REBOOT_CMD_RESTART2 = unchecked((int)0xA1B2C3D4);
                public const Int32 LINUX_REBOOT_CMD_SW_SUSPEND = unchecked((int)0xD000FCE2);
                public const Int32 LINUX_REBOOT_CMD_KEXEC = 0x45584543;

                public const Int32 EPERM = 1;
                public const Int32 EFAULT = 14;
                public const Int32 EINVAL = 22;
            }
        }
    }
}
