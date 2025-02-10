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

            string command = force
                ? "systemctl poweroff --force"
                : "systemctl poweroff";
            if (whatIf)
            {
                return 0;
            }

            return system(command);
        }

        public async Task<int> RebootAsync(
            PowerOffReason reason,
            bool force,
            bool whatIf,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string command = force
                ? "systemctl reboot --force"
                : "systemctl reboot";
            if (whatIf)
            {
                return 0;
            }

            return system(command);
        }

        [DllImport("libc.so.6", SetLastError = true)]
        public static extern int system(string command);
    }
}