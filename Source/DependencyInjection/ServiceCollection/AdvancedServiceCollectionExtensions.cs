using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleDI.Containers;

public static class AdvancedServiceCollectionExtensions
{
    public static bool ContainsServiceOfType(this IServiceProviderIsService services, Type serviceType) =>
        services?.IsService(serviceType) ?? false;

    public static bool TryGetService(this IServiceProvider services, Type serviceType,
        [NotNullWhen(true)] out object? service)
    {
        service = services.GetService(serviceType);
        return service is not null;
    }
}