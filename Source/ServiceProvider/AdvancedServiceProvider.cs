using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;

namespace SimpleDI.ServiceProvider;

public sealed class AdvancedServiceProvider : ServiceCollectionBase
{
    private readonly Dictionary<Type, object> _scopedInstances = new();
    public AdvancedServiceProvider(ServiceCollectionBase original)
        : base(null!, null!)
    {
        IsReadOnly = true;
        ArgumentNullException.ThrowIfNull(original);
        CopyDataByRefFrom(original);
    }
        
    protected override void CheckAllowedCreationOfScopedServices(ServiceDescriptor descriptor) { }

    /// <inheritdoc />
    public override bool TryGetService(Type serviceType, [NotNullWhen(true)] out object? service)
    {
        service = null;
        if (!TryGetLifetimeOfService(serviceType, out var lifetime))
            return false;

        if (lifetime == ServiceLifetime.Scoped
            && _scopedInstances.TryGetValue(serviceType, out service))
            return true;

        var result = InternalTryGetService(serviceType);
        if (!result.Success)
            return false;

        _scopedInstances.Add(serviceType, result.Instance!);
        service = result.Instance!;
        return true;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        foreach (var instance in _scopedInstances.Values)
            if (instance is IDisposable disposable)
                disposable.Dispose();
        _scopedInstances.Clear();
        base.Dispose(disposing);
    }
}