using System.CommandLine;
using Drexel.Host.Commands.Uri.Get;

namespace Drexel.Host.Commands.Uri
{
    public sealed class UriRoot : Command
    {
        public UriRoot()
            : base("uri", "URI-related actions.")
        {
            this.Add(new UriGetCommand());
        }
    }
}
