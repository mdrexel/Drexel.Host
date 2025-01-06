using System.CommandLine;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Uri.Get
{
    internal sealed class UriGetCommand : Command<UriGetCommand.GetUriOptions, UriGetCommand.GetUriHandler>
    {
        private static Option<string> UriOption { get; } = new("--uri", "A URI.");

        public UriGetCommand()
            : base("get", "Gets the URI")
        {
            AddOption(UriOption);
        }

        public sealed class GetUriOptions
        {
            public required string Uri { get; init; }
        }

        public sealed class GetUriHandler(IAnsiConsole console) : ICommandHandler<GetUriOptions, GetUriHandler>
        {
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
