using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Drexel.Host.Internals
{
    internal static class DependencyInjectionMiddleware
    {
        public static CommandLineBuilder UseDependencyInjection(
            this CommandLineBuilder builder,
            Action<ServiceCollection> configureServices)
        {
            return builder.UseDependencyInjection((_, services) => configureServices.Invoke(services));
        }

        public static CommandLineBuilder UseDependencyInjection(
            this CommandLineBuilder builder,
            Action<InvocationContext, ServiceCollection> configureServices)
        {
            return builder.AddMiddleware(
                async (context, next) =>
                {
                    ServiceCollection services = [];
                    configureServices.Invoke(context, services);

                    // TODO: this `ServiceProvider` is disposable, but the `CommandLineBuilder` doesn't expose a hook
                    // indicating when the builder goes out of scope, so we have no way to know when to dispose the
                    // `ServiceProvider`. Since we don't expect someone to call `UseDependencyInjection` multiple times
                    // or to re-use the `CommandLineBuilder` instance, we're probably only going to leak at most one
                    // `ServiceProvider` for the lifetime of the application, so it's probably fine.
                    ServiceProvider provider = services.BuildServiceProvider();
                    context.BindingContext.AddService<IServiceProvider>(_ => provider);
                    foreach (Type serviceType in services.Select(x => x.ServiceType).Distinct())
                    {
                        // Instance of type
                        context.BindingContext.AddService(
                            serviceType,
                            _ => provider.GetRequiredService(serviceType));

                        // Collection of type
                        context.BindingContext.AddService(
                            typeof(IEnumerable<>).MakeGenericType(serviceType),
                            _ => provider.GetServices(serviceType));
                    }

                    await next.Invoke(context);
                });
        }
    }
}