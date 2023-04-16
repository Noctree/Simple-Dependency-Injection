using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Internal;

namespace SimpleDI.Containers;

public class AdvancedServiceCollection : ServiceCollectionBase, IAdvancedServiceCollection
{
    /// <inheritdoc />
    public event ServiceCollectionChangedEventHandler? ServiceCollectionChanged;

    public AdvancedServiceCollection()
        : base(new Dictionary<Type, IndexedDescriptor>(), new Dictionary<int, ServiceDescriptor>()) {}

    public AdvancedServiceCollection(IServiceCollection original)
        : base(new Dictionary<Type, IndexedDescriptor>(), new Dictionary<int, ServiceDescriptor>())
    {
        ArgumentNullException.ThrowIfNull(original);
        foreach (var descriptor in original)
            InternalTryAdd(descriptor);
    }

    public AdvancedServiceCollection(AdvancedServiceCollection original)
        : base(original) {}

    protected override void CheckAllowedCreationOfScopedServices(ServiceDescriptor descriptor) =>
        throw new InvalidOperationException(
            $"Cannot instantiate a scoped service of type {descriptor.ServiceType} outside of a {nameof(IServiceScope)}");

    /// <inheritdoc />
    public override void Add(ServiceDescriptor item)
    {
        base.Add(item);
        ServiceCollectionChanged?.Invoke(ChangeType.Added, null, item);
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        ServiceCollectionChanged?.Invoke(ChangeType.Cleared, null, null);
    }

    public override bool Remove(ServiceDescriptor item)
    {
        var result = base.Remove(item);
        if (result)
            ServiceCollectionChanged?.Invoke(ChangeType.Removed, item, null);
        return result;
    }

    public bool RemoveByType(Type serviceType)
    {
        CheckReadOnly();
        ArgumentNullException.ThrowIfNull(serviceType);
        if (!Services.TryGetValue(serviceType, out var descriptor))
            return false;

        Services.Remove(descriptor.ServiceDescriptor.ServiceType);
        ServicesIndexLookup.Remove(descriptor.Index);
        ServiceCollectionChanged?.Invoke(ChangeType.Removed, descriptor.ServiceDescriptor, null);
        return true;
    }

    /// <inheritdoc />
    public int IndexOf(ServiceDescriptor item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return Services.TryGetValue(item.ServiceType, out var result)
            ? result.Index
            : -1;
    }

    /// <inheritdoc />
    public void Insert(int index, ServiceDescriptor item)
    { 
        CheckReadOnly();
        ServicesIndexLookup.Add(index, item);
        if (!Services.TryAdd(item.ServiceType, new(index, item)))
            Services[item.ServiceType] = new(index, item);
        ServiceCollectionChanged?.Invoke(ChangeType.Added, null, item);
    }
    
    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        CheckReadOnly();
        ServicesIndexLookup.Remove(index);
        var service = Services.FirstOrDefault(id => id.Value.Index == index);
        Services.Remove(service.Key);
        ServiceCollectionChanged?.Invoke(ChangeType.Removed, service.Value.ServiceDescriptor, null);
    }

    /// <inheritdoc />
    public ServiceDescriptor this[int index]
    {
        get => ServicesIndexLookup[index];
        set
        {
            CheckReadOnly();
            ArgumentNullException.ThrowIfNull(value);
            var old = ServicesIndexLookup[index];
            ServicesIndexLookup[index] = value;
            if (!Services.TryAdd(value.ServiceType, new(index, value)))
            {
                Services[value.ServiceType] = new(index, value);
            }
            ServiceCollectionChanged?.Invoke(ChangeType.Updated, old, value);
        }
    }

    /// <inheritdoc />
    public IAdvancedServiceCollection Clone(bool asReadOnly)
    {
        var clone = new AdvancedServiceCollection(this);
        clone.IsReadOnly = asReadOnly;
        return clone;
    }
}