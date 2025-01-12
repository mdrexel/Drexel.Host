using System.Collections.Generic;
using System.CommandLine;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Spectre.Console;

namespace Drexel.Host.Commands.Http.Send
{
    /// <summary>
    /// Performs an HTTP <c>GET</c> request against a specified URI.
    /// </summary>
    internal sealed class HttpSendCommand : Command<HttpSendCommand.Options, HttpSendCommand.Handler>
    {
        private static Option<string> UriOption { get; } =
            new(["--uri", "-u"], "The URI the HTTP request should be sent to.")
            {
                Arity = ArgumentArity.ExactlyOne,
            };

        private static Option<string> MethodOption { get; } =
            new(["--method", "-m"], () => "GET", "The HTTP method to use.");

        private static Option<string[]> HeaderOption { get; } =
            new(["--header", "-h"], "A header name followed by its value.")
            {
                Arity = new ArgumentArity(2, 2),
            };

        private static Option<string> ContentOption { get; } =
            new(["--content", "-c"], "The content associated with the request.");

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSendCommand"/> class.
        /// </summary>
        public HttpSendCommand()
            : base("send", "Sends an outbound HTTP request.")
        {
            AddOption(UriOption);
            AddOption(MethodOption);
            AddOption(HeaderOption);
            AddOption(ContentOption);
        }

        /// <summary>
        /// The options associated with performing the command.
        /// </summary>
        public new sealed class Options
        {
            /// <summary>
            /// Gets the URI to use.
            /// </summary>
            public required string Uri { get; init; }

            /// <summary>
            /// Gets the method to use.
            /// </summary>
            public required string Method { get; init; }

            /// <summary>
            /// Gets the headers to include.
            /// </summary>
            public IReadOnlyDictionary<string, IReadOnlyList<string>>? Header { get; init; }

            /// <summary>
            /// Gets the content to include.
            /// </summary>
            public string? Content { get; init; }
        }

        /// <summary>
        /// The command implementation.
        /// </summary>
        /// <param name="console">
        /// The console to use.
        /// </param>
        public new sealed class Handler(IAnsiConsole console) : ICommandHandler<Options, Handler>
        {
            /// <inheritdoc/>
            public async Task<int> HandleAsync(Options options, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using HttpClient httpClient = new();

                HttpContent? content = options.Content is null ? null : new StringContent(options.Content);

                HttpRequestMessage request =
                    new(new(options.Method), options.Uri)
                    {
                        Content = content,
                    };

                HttpResponseMessage response = await httpClient.GetAsync(options.Uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                console.WriteLine(responseContent);

                return ExitCode.Success;
            }
        }
    }
}
