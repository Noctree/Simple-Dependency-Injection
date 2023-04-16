using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Internal;

namespace SimpleDI.Containers;

/// <summary>
/// Similar in function to scopes in dependency injection, except that containers can be assigned a specific ID and then shared between objects by that ID.
/// Essentially a scope that can be created by one object and then shared with a different one instead of both objects creating their own scope.
/// Each container can optionally also fall back onto the global container, in case the required dependency is not present.
/// </summary>
public sealed class Container : IContainer
{
    private readonly IContainer? _fallback;
    private readonly IAdvancedServiceCollection _serviceCollection;
    private readonly DelayedDependencyResolver _delayedDependencyResolver;
    private readonly Dictionary<string, IContainer> _subContainers;
    public bool IsDisposed { get; private set; }

    public IAdvancedServiceCollection ServiceCollection => _serviceCollection;
    
    /// <summary>
    /// Id of the container
    /// </summary>
    public string Id { get; }
    
    public bool HasFallbackContainer => _fallback is not null;
    
    internal Container() : this(Guid.NewGuid().ToString()) { }

    internal Container(string id, IContainer? fallback = null, IAdvancedServiceCollection? serviceCollection = null)
    {
        Id = id;
        _fallback = fallback;
        _serviceCollection = serviceCollection ?? new AdvancedServiceCollection();
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Must not be an empty string, or whitespace", nameof(id));
        _delayedDependencyResolver = new(this);
        _subContainers = new();
    }

    public void RegisterService(ServiceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (IsRegistered(descriptor.ServiceType))
            throw new TypeAlreadyRegisteredException(descriptor.ServiceType, nameof(descriptor));
        _serviceCollection.Add(descriptor);
    }

    public void UnregisterService(Type serviceType) => _serviceCollection.RemoveByType(serviceType);
    public bool IsRegistered(Type serviceType) => _serviceCollection.ContainsServiceOfType(serviceType);

    public void ResolveDependencies(object instance)
    {
        var result = DependencyResolver.ResolveDependencies(instance, _serviceCollection);
        if (result.Success)
            return;

        if (instance is IDelayedDependencyInjection delayedInstance)
        {
            delayedInstance.MarkForDelayedInjection();
            _delayedDependencyResolver.AddAwaitingTarget(delayedInstance, instance.GetType(), result.MissingDependency!);
        }
        else
            throw new ServiceNotFoundException(result.MissingDependency!, instance.GetType());
    }

    public IContainer CreateSubContainer() =>
        CreateSubContainer(Guid.NewGuid().ToString());

    public IContainer CreateSubContainer(string id)
    {
        var container = new Container(id, this);
        if (_subContainers.TryAdd(id, container))
            return container;
        throw new DuplicitIdException(id);
    }

    public bool TryGetSubContainer(string id, [NotNullWhen(true)] out IContainer container) =>
        _subContainers.TryGetValue(id, out container);

    public bool DeleteSubContainer(string id)
    {
        if (_subContainers.TryGetValue(id, out var container))
        {
            container.Dispose();
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        DependencyInjector.DeleteContainer(this);
        foreach (var container in _subContainers.Values)
            container.Dispose();
        foreach (var instance in _serviceCollection)
            if (instance.ImplementationInstance is IDisposable disposable)
                disposable.Dispose();
        _serviceCollection.Clear();
    }

    /// <inheritdoc />
    public bool TryGetService(Type serviceType, [NotNullWhen(true)] out object? service)
    {
        if (_serviceCollection.TryGetService(serviceType, out service))
            return true;

        return _fallback is not null
               && _fallback.TryGetService(serviceType, out service);
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        TryGetService(serviceType, out var service);
        return service;
    }

    /// <inheritdoc />
    public bool IsService(Type serviceType) =>
        _serviceCollection.IsService(serviceType) || (_fallback?.IsService(serviceType) ?? false);

    /// <inheritdoc />
    public object GetRequiredService(Type serviceType) =>
        _serviceCollection.GetRequiredService(serviceType);

    /// <summary>
    /// Creates a new Container with a copy of the specified service collection
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IContainer Wrap(IAdvancedServiceCollection serviceCollection)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        return new Container(Guid.NewGuid().ToString(), null, serviceCollection.Clone());
    }
}
