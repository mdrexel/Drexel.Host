using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Drexel.Host.Internals
{
    /// <inheritdoc cref="ICommandHandler"/>
    /// <typeparam name="T">
    /// The type of object containing the command options associated with this handler.
    /// </typeparam>
    internal interface ICommandHandler<in T>
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
        static abstract ICommandHandler<T> Create(ServiceProvider serviceProvider);

        /// <inheritdoc cref="ICommandHandler.InvokeAsync(InvocationContext)"/>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token to observe.
        /// </param>
        Task<int> HandleAsync(T options, CancellationToken cancellationToken);
    }
}
