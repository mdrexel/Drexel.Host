using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Drexel.Host.Internals
{
    /// <inheritdoc cref="Command"/>
    /// <typeparam name="TOptions">
    /// The type of options the command callback receives.
    /// </typeparam>
    /// <typeparam name="THandler">
    /// The type that implements the command callback.
    /// </typeparam>
    internal abstract class Command<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler> : Command
        where THandler : ICommandHandler<TOptions, THandler>
    {
        /// <inheritdoc cref="Command(string, string?)"/>
        protected Command(string name, string description)
            : base(name, description)
        {
            this.Handler = CommandHandler.Create</*InvocationContext, */ TOptions, IServiceProvider, CancellationToken>(
                static async (options, serviceProvider, cancellationToken) =>
                {
                    THandler handler = THandler.Create(serviceProvider);
                    return await handler.HandleAsync(options, cancellationToken);
                });
        }
    }
}
