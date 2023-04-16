using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleDI.Containers;

public interface IAdvancedServiceProvider : IServiceProviderIsService, IServiceProvider, ISupportRequiredService, IDisposable
{
    public bool TryGetService(Type serviceType, [NotNullWhen(true)] out object? service);
}