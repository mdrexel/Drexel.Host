using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

            rootCommand.SetHandler(
                static async (context) =>
                {
                    string? urlOptionValue = context.ParseResult.GetValueForOption(UrlOption);
                    CancellationToken token = context.GetCancellationToken();

                    context.ExitCode = await DoRootCommand(urlOptionValue, token);
                });

            Parser parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build();
            return await parser.InvokeAsync(args);
        }

        public static async Task<int> DoRootCommand(
            string? urlOptionValue,
            CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient httpClient = new();

                HttpResponseMessage response = await httpClient.GetAsync(urlOptionValue, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                AnsiConsole.Console.WriteLine(content);

                return 0;
            }
            catch (OperationCanceledException e)
            {
                AnsiConsole.Console.WriteException(e, ExceptionFormats.NoStackTrace);
                return 1;
            }
            catch (Exception e)
            {
                AnsiConsole.Console.WriteException(e, ExceptionFormats.NoStackTrace);
                return 2;
            }
        }
    }
}
