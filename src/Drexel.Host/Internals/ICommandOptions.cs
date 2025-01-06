using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Drexel.Host.Internals
{
    /// <summary>
    /// Represents the options associated with a command.
    /// </summary>
    /// <typeparam name="TOptions">
    /// The type of object representing the options.
    /// </typeparam>
    public interface ICommandOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] out TOptions>
        where TOptions : ICommandOptions<TOptions>
    {
        /////// <summary>
        /////// Creates an instance of the options.
        /////// </summary>
        /////// <param name="context">
        /////// The invocation context associated with the invocation of the application.
        /////// </param>
        /////// <returns>
        /////// An instance of the options.
        /////// </returns>
        /////// <exception cref="NotSupportedException">
        /////// Thrown when <typeparamref name="TOptions"/> does not have exactly one constructor.
        /////// </exception>
        ////static virtual TOptions Create(InvocationContext context)
        ////{
        ////    ConstructorInfo constructor = GetConstructor();

        ////    IReadOnlyList<ParameterInfo> parameters = constructor.GetParameters();
        ////    context.ParseResult.

        ////    return (TOptions)constructor.Invoke()

        ////    static ConstructorInfo GetConstructor()
        ////    {
        ////        IReadOnlyList<ConstructorInfo> constructors = typeof(TOptions).GetConstructors();
        ////        if (constructors.Count != 1)
        ////        {
        ////            throw new NotSupportedException("Exactly one constructor which receives all parameters is required.");
        ////        }

        ////        return constructors[0];
        ////    }
        ////}
    }
}
