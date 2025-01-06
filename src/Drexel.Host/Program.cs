using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Drexel.Host.Internals;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Drexel.Host
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand =
                new("Max experimenting")
                {
                    new Command("uri", "More experimenting")
                    {
                        new GetUriCommand(),
                    },
                };

            Parser parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseExceptionHandler(
                    (exception, context) =>
                    {
                        // TODO: `WriteException` writes to stdout, but `UseParseErrorReporting` writes directly to
                        // `stderr` (and also twiddles the console colors). Maybe we can inject an `IConsole` that
                        // forwards all the output to `AnsiConsole`? Or just ignore the issue since if we die due to
                        // being improperly invoked, we never spun up an `AnsiConsole`, so the lifetimes never overlap?
                        AnsiConsole.Console.WriteException(exception);
                        context.ExitCode = ExitCode.UnspecifiedFailure;
                    })
                .UseParseErrorReporting(ExitCode.IncorrectInvocation)
                .UseDependencyInjection(
                    services =>
                    {
                        services.AddSingleton(AnsiConsole.Console);
                    })
                .Build();

            return await parser.InvokeAsync(args);
        }

        private sealed class GetUriCommand : Command<GetUriCommand.GetUriOptions, GetUriCommand.GetUriHandler>
        {
            private static Option<string> UriOption { get; } = new("--uri", "A URI.");

            public GetUriCommand()
                : base("get", "Gets the URI")
            {
                AddOption(UriOption);
            }

            public sealed class GetUriOptions(string uri) : ICommandOptions<GetUriOptions>
            {
                public string Uri { get; } = uri;
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
}
