using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleDI.Containers;

public enum ChangeType
{
    Added,
    Removed,
    Cleared,
    Updated
}

/// <summary>
/// Handler for <see cref="IAdvancedServiceCollection.ServiceCollectionChanged"/> changed events.
/// </summary>
/// <param name="changeType">The type of change that occured</param>
/// <param name="oldServiceDescriptor">The service descriptor that was updated, or removed.<br/> <b>Will be null on <see cref="ChangeType.Added"/></b></param>
/// <param name="newServiceDescriptor">The service descriptor that was added or that was updated to.<br/> <b>Will be null on <see cref="ChangeType.Removed"/></b></param>
public delegate void ServiceCollectionChangedEventHandler(ChangeType changeType,
    ServiceDescriptor? oldServiceDescriptor, ServiceDescriptor? newServiceDescriptor);

public interface IAdvancedServiceCollection : IServiceCollection, IAdvancedServiceProvider
{
    /// <summary>
    /// Removes a registered service by its type
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public bool RemoveByType(Type serviceType);

    public event ServiceCollectionChangedEventHandler ServiceCollectionChanged;

    /// <summary>
    /// Creates a copy of the current service collection
    /// </summary>
    /// <returns></returns>
    public IAdvancedServiceCollection Clone() => Clone(false);

    /// <summary>
    /// Creates a copy of the current service collection
    /// </summary>
    /// <param name="asReadOnly">Make the cloned instance read only</param>
    /// <returns></returns>
    public IAdvancedServiceCollection Clone(bool asReadOnly);
}