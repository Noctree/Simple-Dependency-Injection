using System;
using System.Collections.Generic;
using SimpleDI.Scopes;

namespace SimpleDI.Internal;

internal sealed class DelayedDependencyResolver
{
    private readonly Scope _owner;
    private readonly Dictionary<Type, DelayedTargetInfo> _waitingTargets = new();
    private readonly Dictionary<Type, HashSet<Type>> _waitingDependencies = new();

    public DelayedDependencyResolver(Scope owner)
    {
        _owner = owner;
    }

    public void AddAwaitingTarget(object instance, Type instanceType, Type awaitingDependencyType) {
        if (instance is not IDelayedDependencyInjection instanceDelayedDI) {
            throw new DITypeNotSupportedException(instanceType);
        }
        if (!_waitingTargets.TryGetValue(instanceType, out var awaitingTargets)) {
            awaitingTargets = new() { Dependencies = new(), Targets = new() };
            _waitingTargets.Add(instanceType, awaitingTargets);
        }
        awaitingTargets.Targets.Add(instanceDelayedDI);
        awaitingTargets.Dependencies.Add(awaitingDependencyType);
        if (!_waitingDependencies.TryGetValue(awaitingDependencyType, out var dependencies)) {
            dependencies = new();
            _waitingDependencies.Add(awaitingDependencyType, dependencies);
        }
        dependencies.Add(instanceType);
    }

    public void AddResolvedDependency(Type resolvedType) {
        if (!_waitingDependencies.TryGetValue(resolvedType, out var awaitingTargetTypes))
            return;
        foreach (var awaitingType in awaitingTargetTypes) {
            if (_waitingTargets.TryGetValue(awaitingType, out var targets)) {
                targets.Dependencies.Remove(resolvedType);
                if (targets.Dependencies.Count == 0) {
                    foreach (var target in targets.Targets) {
                        _owner.ResolveDependencies(target);
                        target.OnDependenciesInjected();
                    }
                    _waitingTargets.Remove(awaitingType);
                }
            }
        }
        _waitingDependencies.Remove(resolvedType);
    }
}
