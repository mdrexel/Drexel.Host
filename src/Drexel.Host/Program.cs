using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Drexel.Host.Commands.Gpio;
using Drexel.Host.Commands.Http;
using Drexel.Host.Commands.Power;
using Drexel.Host.Internals;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Drexel.Host
{
    /// <summary>
    /// The host application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point.
        /// </summary>
        /// <param name="args">
        /// The arguments supplied as part of the command invocation.
        /// </param>
        /// <returns>
        /// An exit code describing the state of the application.
        /// </returns>
        public static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand =
                new("Max's server utilities")
                {
                    new HttpRoot(),
                    new PowerRoot(),
                    new GpioRoot(),
                };

            Parser parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseExceptionHandler(
                    (exception, _) =>
                    {
                        // TODO: `WriteException` writes to stdout, but `UseParseErrorReporting` writes directly to
                        // `stderr` (and also twiddles the console colors). Maybe we can inject an `IConsole` that
                        // forwards all the output to `AnsiConsole`? Or just ignore the issue since if we die due to
                        // being improperly invoked, we never spun up an `AnsiConsole`, so the lifetimes never overlap?
                        AnsiConsole.Console.WriteException(exception);
                    },
                    ExitCode.UnspecifiedFailure)
                .UseParseErrorReporting(ExitCode.IncorrectInvocation)
                .UseDependencyInjection(
                    services =>
                    {
                        services.AddSingleton(AnsiConsole.Console);
                    })
                .Build();

            return await parser.InvokeAsync(args);
        }
    }
}
