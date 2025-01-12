using System.CommandLine;
using Drexel.Host.Commands.Http.Send;

namespace Drexel.Host.Commands.Http
{
    /// <summary>
    /// The <c>http</c> command root.
    /// </summary>
    public sealed class HttpRoot : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRoot"/> class.
        /// </summary>
        public HttpRoot()
            : base("http", "HTTP-related actions.")
        {
            this.Add(new HttpSendCommand());
        }
    }
}
