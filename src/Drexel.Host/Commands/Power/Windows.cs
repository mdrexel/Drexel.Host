using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Windows.Win32;
using Windows.Win32.System.Shutdown;

namespace Drexel.Host.Commands.Power
{
    [SupportedOSPlatform("windows")]
    internal sealed class Windows(IAnsiConsole console)
    {
        [SupportedOSPlatform("windows5.1.2600")]
        public async Task<int> ShutdownAsync(
            PowerOffReason reason,
            bool force,
            bool whatIf,
            CancellationToken cancellationToken)
        {
            EXIT_WINDOWS_FLAGS mode = force
                ? EXIT_WINDOWS_FLAGS.EWX_POWEROFF | EXIT_WINDOWS_FLAGS.EWX_FORCE
                : EXIT_WINDOWS_FLAGS.EWX_POWEROFF;

            return await ExitWindowsExImpl(mode, reason, force, whatIf, cancellationToken);
        }

        [SupportedOSPlatform("windows5.1.2600")]
        public async Task<int> RebootAsync(
            PowerOffReason reason,
            bool force,
            bool whatIf,
            CancellationToken cancellationToken)
        {
            EXIT_WINDOWS_FLAGS mode = force
                ? EXIT_WINDOWS_FLAGS.EWX_REBOOT | EXIT_WINDOWS_FLAGS.EWX_FORCE
                : EXIT_WINDOWS_FLAGS.EWX_REBOOT;

            return await ExitWindowsExImpl(mode, reason, force, whatIf, cancellationToken);
        }

        [SupportedOSPlatform("windows5.1.2600")]
        private async Task<int> ExitWindowsExImpl(
            EXIT_WINDOWS_FLAGS mode,
            PowerOffReason reason,
            bool force,
            bool whatIf,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SHUTDOWN_REASON win32Reason = Convert(reason);

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

            if (!whatIf)
            {
                if (!PInvoke.ExitWindowsEx(mode, win32Reason))
                {
                    console.WriteException(
                        new Exception(Marshal.GetLastPInvokeErrorMessage()),
                        ExceptionFormats.NoStackTrace);
                    return Marshal.GetLastPInvokeError();
                }
            }

            return 0;

            static SHUTDOWN_REASON Convert(PowerOffReason reason) =>
                reason switch
                {
                    PowerOffReason.None => SHUTDOWN_REASON.SHTDN_REASON_NONE,
                    PowerOffReason.Power => SHUTDOWN_REASON.SHTDN_REASON_MAJOR_POWER,
                    PowerOffReason.Software => SHUTDOWN_REASON.SHTDN_REASON_MAJOR_SOFTWARE,
                    PowerOffReason.Hardware => SHUTDOWN_REASON.SHTDN_REASON_MAJOR_HARDWARE,
                    _ => throw new ArgumentException("Unrecognized power-off reason.", nameof(reason)),
                };
        }

        [DllImport("advapi32.dll")]
        private static extern int OpenProcessToken(nint ProcessHandle, int DesiredAccess, out nint TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges(
            nint TokenHandle,
            [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            uint BufferLength,
            nint PreviousState,
            nint ReturnLength);

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
}
