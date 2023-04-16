using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Internal;

namespace SimpleDI.Containers;

public abstract class ServiceCollectionBase : IAdvancedServiceProvider, ICollection<ServiceDescriptor>
{
    protected Dictionary<Type, IndexedDescriptor> Services { get; private set; }
    protected Dictionary<int, ServiceDescriptor> ServicesIndexLookup { get; private set; }

    protected record struct IndexedDescriptor(int Index, ServiceDescriptor ServiceDescriptor);

    /// <inheritdoc />
    public int Count => Services.Count;

    /// <inheritdoc />
    public bool IsReadOnly { get; protected set; }
    
    protected ServiceCollectionBase(Dictionary<Type, IndexedDescriptor> services,
        Dictionary<int, ServiceDescriptor> servicesIndexLookup)
    {
        Services = services;
        ServicesIndexLookup = servicesIndexLookup;
    }

    protected ServiceCollectionBase(ServiceCollectionBase original)
    {
        ArgumentNullException.ThrowIfNull(original);
        Services = new Dictionary<Type, IndexedDescriptor>(original.Services);
        ServicesIndexLookup = new Dictionary<int, ServiceDescriptor>(original.ServicesIndexLookup);
    }
    
    protected bool InternalTryAdd(ServiceDescriptor item)
    {
        var index = Services.Count;
        if (!Services.TryAdd(item.ServiceType, new IndexedDescriptor(index, item)))
            return false;
        ServicesIndexLookup.Add(index, item);
        return true;
    }
    
    /// <summary>
    /// Checks the <see cref="IsReadOnly"/> property
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="IsReadOnly"/> is true</exception>
    protected void CheckReadOnly()
    {
        if (IsReadOnly)
            throw new InvalidOperationException("Cannot modify a read-only service collection.");
    }
    
    protected abstract void CheckAllowedCreationOfScopedServices(ServiceDescriptor descriptor);

    protected void CopyDataByRefFrom(ServiceCollectionBase source)
    {
        Services = source.Services;
        ServicesIndexLookup = source.ServicesIndexLookup;
    }
    
    protected ServiceConstructionResult InternalTryGetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        object? service = null;
        if (Services.TryGetValue(serviceType, out var descriptor))
        {
            switch (descriptor.ServiceDescriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    service = descriptor.ServiceDescriptor.ImplementationInstance;
                    if (service is not null)
                        return new ServiceConstructionResult(true, service);
                    var result = DependencyResolver.TryConstructService(descriptor.ServiceDescriptor, this);
                    if (result.Success)
                        descriptor.ServiceDescriptor = descriptor.ServiceDescriptor.WithInstance(result.Instance!);
                    return result;
                case ServiceLifetime.Scoped:
                    CheckAllowedCreationOfScopedServices(descriptor.ServiceDescriptor);
                    return DependencyResolver.TryConstructService(descriptor.ServiceDescriptor, this);
                case ServiceLifetime.Transient:
                    return DependencyResolver.TryConstructService(descriptor.ServiceDescriptor, this);
                default:
                    throw new InvalidOperationException("Should never throw");
            }
        }
        return new ServiceConstructionResult(false, null, serviceType);
    }

    protected bool TryGetLifetimeOfService(Type serviceType, [NotNullWhen(true)] out ServiceLifetime? lifetime)
    {
        lifetime = null;
        if (!Services.TryGetValue(serviceType, out var descriptor))
            return false;
        
        lifetime = descriptor.ServiceDescriptor.Lifetime;
        return true;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">If the service type is already registered</exception>
    public virtual void Add(ServiceDescriptor item)
    {
        ArgumentNullException.ThrowIfNull(item);
        CheckReadOnly();
        if (!InternalTryAdd(item))
            throw new ArgumentException($"Service {item.ServiceType} already exists", nameof(item));
    }


    /// <summary>
    /// Disposes of any Singleton instances that were created and clean up the collection
    /// </summary>
    public virtual void Clear()
    {
        CheckReadOnly();
        Services.Clear();
        foreach (var descriptor in ServicesIndexLookup.Values)
            if (descriptor.ImplementationInstance is IDisposable disposable)
                disposable.Dispose();
        ServicesIndexLookup.Clear();
    }

    /// <summary>
    /// Determines whether the collection contains the specified <paramref name="item"/><br/>
    /// Using <see cref="IsService"/> is recommended over this method, as it's faster
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual bool Contains(ServiceDescriptor item) => ServicesIndexLookup.ContainsValue(item);

    /// <inheritdoc />
    public virtual void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ServicesIndexLookup.Values.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public virtual bool Remove(ServiceDescriptor item)
    {
        CheckReadOnly();
        ArgumentNullException.ThrowIfNull(item);
        var index = Services.Values
            .FirstOrDefault(d => d.ServiceDescriptor == item, new IndexedDescriptor(-1, null!));
        if (index.Index == -1)
            return false;
        Services.Remove(item.ServiceType);
        ServicesIndexLookup.Remove(index.Index);
        return true;
    }
    
    public virtual IEnumerator<ServiceDescriptor> GetEnumerator() => ServicesIndexLookup.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public virtual bool IsService(Type serviceType) => Services.ContainsKey(serviceType);

    public virtual object? GetService(Type serviceType) => 
        TryGetService(serviceType, out var instance) ? instance : null;

    public virtual object GetRequiredService(Type serviceType)
    {
        if (!TryGetService(serviceType, out var service))
            throw new ServiceNotFoundException(serviceType);
        return service;
    }

    public virtual bool TryGetService(Type serviceType, [NotNullWhen(true)] out object? service)
    {
        var result = InternalTryGetService(serviceType);
        service = result.Instance;
        return result.Success;
    }

    /// <summary>
    /// Iterates over all services and disposes of any Singleton instances that implement <see cref="IDisposable"/>
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        foreach (var instance in ServicesIndexLookup.Values
                     .Select(static d => d.ImplementationInstance))
            if (instance is IDisposable disposable)
                disposable.Dispose();
        Services.Clear();
        ServicesIndexLookup.Clear();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}