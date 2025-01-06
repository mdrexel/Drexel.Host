using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Drexel.Host.Internals
{
    /// <inheritdoc cref="Command"/>
    /// <typeparam name="TOptions">
    /// The type of options the command callback receives.
    /// </typeparam>
    /// <typeparam name="THandler">
    /// The type that implements the command callback.
    /// </typeparam>
    internal abstract class Command<TOptions, THandler> : Command
        where THandler : ICommandHandler<TOptions>
    {
        /// <inheritdoc cref="Command(string, string?)"/>
        protected Command(string name, string description)
            : base(name, description)
        {
            this.Handler = CommandHandler.Create<TOptions, ServiceProvider, CancellationToken>(
                static async (options, serviceProvider, cancellationToken) =>
                {
                    ICommandHandler<TOptions> callback = THandler.Create(serviceProvider);
                    return await callback.HandleAsync(options, cancellationToken);
                });
        }
    }
}
