using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace Drexel.Host.Commands.Power
{
    [SupportedOSPlatform("linux")]
    internal sealed class Linux(IAnsiConsole console)
    {
        public async Task<int> ShutdownAsync(
            PowerOffReason reason,
            bool force,
            bool whatIf,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int cmd = Convert(RebootCommand.PowerOff);
            if (whatIf)
            {
                return 0;
            }

            return RebootImpl(cmd);
        }

        public async Task<int> RebootAsync(
            PowerOffReason reason,
            bool force,
            bool whatIf,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int cmd = Convert(RebootCommand.Restart);
            if (whatIf)
            {
                return 0;
            }

            return RebootImpl(cmd);
        }

        public enum RebootCommand
        {
            Restart,
            Halt,
            ControlAltDeleteEnable,
            ControlAltDeleteDisable,
            PowerOff,
            RestartWithCommand,
            Suspend,
            ExecuteKernel
        }

        private int RebootImpl(int cmd)
        {
            int result = reboot(cmd, IntPtr.Zero);
            if (result != -1)
            {
                console.WriteException(
                    new Exception("Unexpected return code."),
                    ExceptionFormats.NoStackTrace);
                return result;
            }

            const int EPERM = 1;
            const int EFAULT = 14;
            const int EINVAL = 22;
            result = Marshal.GetLastWin32Error();
            switch (result)
            {
                case EPERM:
                    throw new UnauthorizedAccessException("Don't have permissions to invoke `reboot` syscall wrapper.");
                case EINVAL:
                    throw new InvalidOperationException("Bad input values.");
                case EFAULT:
                default:
                    throw new InvalidOperationException("Could not invoke `reboot` syscall wrapper:" + result.ToString());
            }
        }

        private static int Convert(RebootCommand mode)
        {
            const int LINUX_REBOOT_CMD_RESTART = 0x01234567;
            const int LINUX_REBOOT_CMD_HALT = unchecked((int)0xCDEF0123);
            const int LINUX_REBOOT_CMD_CAD_ON = unchecked((int)0x89ABCDEF);
            const int LINUX_REBOOT_CMD_CAD_OFF = 0x00000000;
            const int LINUX_REBOOT_CMD_POWER_OFF = 0x4321FEDC;
            const int LINUX_REBOOT_CMD_RESTART2 = unchecked((int)0xA1B2C3D4);
            const int LINUX_REBOOT_CMD_SW_SUSPEND = unchecked((int)0xD000FCE2);
            const int LINUX_REBOOT_CMD_KEXEC = 0x45584543;

            return
                mode switch
                {
                    RebootCommand.ControlAltDeleteDisable => LINUX_REBOOT_CMD_CAD_OFF,
                    RebootCommand.ControlAltDeleteEnable => LINUX_REBOOT_CMD_CAD_ON,
                    RebootCommand.ExecuteKernel => LINUX_REBOOT_CMD_KEXEC,
                    RebootCommand.Halt => LINUX_REBOOT_CMD_HALT,
                    RebootCommand.PowerOff => LINUX_REBOOT_CMD_POWER_OFF,
                    RebootCommand.Restart => LINUX_REBOOT_CMD_RESTART,
                    RebootCommand.RestartWithCommand => LINUX_REBOOT_CMD_RESTART2,
                    RebootCommand.Suspend => LINUX_REBOOT_CMD_SW_SUSPEND,
                    _ => throw new InvalidOperationException("Unrecognized mode."),
                };
        }

        [DllImport("libc.so.6", SetLastError = true)]
        public static extern int reboot(int cmd, nint arg);

        [DllImport("libc.so.6", SetLastError = true)]
        public static extern int system(string command);
    }
}