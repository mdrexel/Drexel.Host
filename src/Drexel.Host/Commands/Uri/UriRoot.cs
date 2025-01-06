using System.CommandLine;
using Drexel.Host.Commands.Uri.Get;

namespace Drexel.Host.Commands.Uri
{
    /// <summary>
    /// The <c>uri</c> command root.
    /// </summary>
    public sealed class UriRoot : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriRoot"/> class.
        /// </summary>
        public UriRoot()
            : base("uri", "URI-related actions.")
        {
            this.Add(new UriGetCommand());
        }
    }
}
