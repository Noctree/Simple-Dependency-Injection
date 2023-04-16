using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;

namespace SimpleDI.Internal;

internal sealed class DelayedDependencyResolver
{
    private readonly IContainer _owner;
    
    //Stores references to all instances of a type that are awaiting injection, Type = Type of instance
    private readonly Dictionary<Type, DelayedTargetInfo> _waitingTargets = new();
    
    //Stores sets of types that are waiting for injection
    //Type = Type of service that is yet to be registered, Hashset<Type> = set of instances that are missing said type
    private readonly Dictionary<Type, HashSet<Type>> _waitingDependencies = new();

    public DelayedDependencyResolver(IContainer owner)
    {
        _owner = owner;
        _owner.ServiceCollection.ServiceCollectionChanged += ServiceCollectionOnServiceCollectionChanged;
    }

    private void ServiceCollectionOnServiceCollectionChanged(
        ChangeType changetype,
        ServiceDescriptor? oldservicedescriptor,
        ServiceDescriptor? newservicedescriptor)
    {
        switch (changetype)
        {
            case ChangeType.Added:
                AddResolvedDependency(newservicedescriptor!.ServiceType);
                return;
            case ChangeType.Removed:
                RemoveResolvedDependency(oldservicedescriptor!.ServiceType);
                return;
            case ChangeType.Updated:
                if (oldservicedescriptor!.ServiceType == newservicedescriptor!.ServiceType)
                    return;
                AddResolvedDependency(newservicedescriptor.ServiceType);
                RemoveResolvedDependency(oldservicedescriptor.ServiceType);
                return;
            case ChangeType.Cleared:
                InvalidateAllResolvedDependencies();
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(changetype), changetype, string.Empty);
        }
    }

    public void AddAwaitingTarget(object instance, Type instanceType, Type awaitingDependencyType) {
        if (instance is not IDelayedDependencyInjection instanceDelayedDi) {
            throw new TypeNotSupportedException(instanceType);
        }
        if (!_waitingTargets.TryGetValue(instanceType, out var awaitingTargets)) {
            awaitingTargets = new() { Dependencies = new(), Targets = new() };
            _waitingTargets.Add(instanceType, awaitingTargets);
        }
        awaitingTargets.Targets.Add(instanceDelayedDi);
        
        //If the dependency is already being awaited, set it to false, as this means it's still missing
        if (!awaitingTargets.Dependencies.TryAdd(awaitingDependencyType, false))
            awaitingTargets.Dependencies[awaitingDependencyType] = false;
        if (!_waitingDependencies.TryGetValue(awaitingDependencyType, out var dependencies)) {
            dependencies = new();
            _waitingDependencies.Add(awaitingDependencyType, dependencies);
        }
        dependencies.Add(instanceType);
    }

    private void AddResolvedDependency(Type resolvedType) {
        if (!_waitingDependencies.TryGetValue(resolvedType, out var awaitingTargetTypes))
            return;
        foreach (var awaitingType in awaitingTargetTypes) {
            if (_waitingTargets.TryGetValue(awaitingType, out var targets)
                && TryResolveAwaitingInstances(resolvedType, targets))
            {
                _waitingTargets.Remove(awaitingType);
            }
        }
        _waitingDependencies.Remove(resolvedType);
    }

    private void RemoveResolvedDependency(Type unresolvedType)
    {
        if (!_waitingDependencies.TryGetValue(unresolvedType, out var awaitingTargets))
            return;
        
        foreach (var awaitingType in awaitingTargets)
        {
            if (!_waitingTargets.TryGetValue(awaitingType, out var targets))
                continue;
            foreach (var dependencyType in targets.Dependencies.Keys)
                targets.Dependencies[dependencyType] = false;
        }
    }

    private bool TryResolveAwaitingInstances(Type resolvedType, DelayedTargetInfo targets)
    {
        //Only resolve the dependencies if all services are registered
        targets.Dependencies[resolvedType] = true;
        if (targets.Dependencies.Values.All(static v => v)) {
            foreach (var target in targets.Targets) {
                _owner.ResolveDependencies(target);
                target.OnDependenciesInjected();
            }
            return true;
        }
        return false;
    }

    private void InvalidateAllResolvedDependencies()
    {
        foreach (var target in _waitingTargets.Values) {
            foreach (var dependency in target.Dependencies.Keys) {
                target.Dependencies[dependency] = false;
            }
        }
    }

    public void Clear()
    {
        _waitingDependencies.Clear();
        _waitingTargets.Clear();
    }
}
