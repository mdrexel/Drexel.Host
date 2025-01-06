using System.CommandLine;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Uri.Get
{
    /// <summary>
    /// Performs an HTTP <c>GET</c> request against a specified URI.
    /// </summary>
    internal sealed class UriGetCommand : Command<UriGetCommand.GetUriOptions, UriGetCommand.GetUriHandler>
    {
        private static Option<string> UriOption { get; } = new("--uri", "A URI.");

        /// <summary>
        /// Initializes a new instance of the <see cref="UriGetCommand"/> class.
        /// </summary>
        public UriGetCommand()
            : base("get", "Gets the URI")
        {
            AddOption(UriOption);
        }

        /// <summary>
        /// The options associated with performing the command.
        /// </summary>
        public sealed class GetUriOptions
        {
            /// <summary>
            /// Gets the URI to use.
            /// </summary>
            public required string Uri { get; init; }
        }

        /// <summary>
        /// The command implementation.
        /// </summary>
        /// <param name="console"></param>
        public sealed class GetUriHandler(IAnsiConsole console) : ICommandHandler<GetUriOptions, GetUriHandler>
        {
            /// <inheritdoc/>
            public async Task<int> HandleAsync(GetUriOptions options, CancellationToken cancellationToken)
            {
                using HttpClient httpClient = new();

                HttpResponseMessage response = await httpClient.GetAsync(options.Uri, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                console.WriteLine(content);

                return ExitCode.Success;
            }
        }
    }
}
