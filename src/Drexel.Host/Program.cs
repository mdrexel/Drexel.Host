using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
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
        private static Option<string> UrlOption { get; } = new("--url", "A URL.");

        public static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand =
                new("Max experimenting")
                {
                    UrlOption,
                };

            rootCommand.Handler = CommandHandler.Create<InvocationContext, IAnsiConsole>(
                static async (context, console) =>
                {
                    string? urlOptionValue = context.ParseResult.GetValueForOption(UrlOption);
                    CancellationToken token = context.GetCancellationToken();

                    context.ExitCode = await DoRootCommand(console, urlOptionValue, token);
                });

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

        public static async Task<int> DoRootCommand(
            IAnsiConsole console,
            string? urlOptionValue,
            CancellationToken cancellationToken)
        {
            using HttpClient httpClient = new();

            HttpResponseMessage response = await httpClient.GetAsync(urlOptionValue, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            console.WriteLine(content);

            return ExitCode.Success;
        }
    }
}
