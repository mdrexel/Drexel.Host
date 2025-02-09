using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Drexel.Host.Commands.Power
{
    internal sealed class Linux(IAnsiConsole console)
    {
        [SupportedOSPlatform("linux")]
        public async Task<int> ShutdownAsync(
            PowerOffReason reason,
            bool force,
            bool whatIf,
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

        [DllImport("libc.so.6", SetLastError = true)]
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
