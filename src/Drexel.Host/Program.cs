using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
                    context.ExitCode = await DoRootCommand(context.Console, urlOptionValue, token);
                });

            return await rootCommand.InvokeAsync(args);
        }

        public static async Task<int> DoRootCommand(
            IConsole console,
            string? urlOptionValue,
            CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient httpClient = new();

                HttpResponseMessage response = await httpClient.GetAsync(urlOptionValue, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                console.WriteLine(content);

                return 0;
            }
            catch (OperationCanceledException e)
            {
                console.Error.WriteLine(e.Message);
                return 1;
            }
        }
    }
}
