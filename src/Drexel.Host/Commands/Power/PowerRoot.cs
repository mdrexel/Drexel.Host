using System.CommandLine;
using Drexel.Host.Commands.Power.Off;

namespace Drexel.Host.Commands.Power
{
    /// <summary>
    /// The <c>power</c> command root.
    /// </summary>
    public sealed class PowerRoot : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PowerRoot"/> class.
        /// </summary>
        public PowerRoot()
            : base("power", "Power-related actions.")
        {
            this.Add(new PowerOffCommand());
        }
    }
}
