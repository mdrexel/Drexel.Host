using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Drexel.Host.Internals
{
    /// <inheritdoc cref="ICommandHandler"/>
    /// <typeparam name="TOptions">
    /// The type of options the command callback receives.
    /// </typeparam>
    /// <typeparam name="THandler">
    /// The type that implements the command callback.
    /// </typeparam>
    internal interface ICommandHandler<in TOptions, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] out THandler>
        where THandler : ICommandHandler<TOptions, THandler>
    {
        /// <summary>
        /// Creates an instance of the handler.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider to use when creating the handler instance.
        /// </param>
        /// <returns>
        /// An instance of the handler.
        /// </returns>
        static virtual THandler Create(ServiceProvider serviceProvider)
        {
            return ActivatorUtilities.CreateInstance<THandler>(serviceProvider);
        }

        /// <inheritdoc cref="ICommandHandler.InvokeAsync(InvocationContext)"/>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token to observe.
        /// </param>
        Task<int> HandleAsync(TOptions options, CancellationToken cancellationToken);
    }
}
