using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;

namespace SimpleDI.Scopes;

public sealed class Scope : IAdvancedServiceScope
{
    public Scope(IAdvancedServiceProvider provider)
    {
        AdvancedServiceProvider = provider;
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        AdvancedServiceProvider.Dispose();
    }
    
    public IAdvancedServiceProvider AdvancedServiceProvider { get; }

    public IServiceProvider ServiceProvider => AdvancedServiceProvider;
}