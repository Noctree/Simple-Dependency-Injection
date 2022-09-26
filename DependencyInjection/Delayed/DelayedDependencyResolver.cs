using System;
using System.Collections.Generic;
using NokLib.DependencyInjection.Exceptions;

namespace NokLib.DependencyInjection.Internal;

public static class DelayedDependencyResolver
{
    private record struct DelayedTargetInfo(HashSet<IDelayedDependencyInjection> Targets, HashSet<Type> Dependencies);
    private static Dictionary<Type, DelayedTargetInfo> WaitingTargets = new();
    private static Dictionary<Type, HashSet<Type>> WaitingDependencies = new();

    public static void AddAwaitingTarget(object instance, Type instanceType, Type awaitingDependencyType) {
        if (instance is not IDelayedDependencyInjection instanceDelayedDI) {
            throw new DITypeNotSupportedException(instanceType);
        }
        if (!WaitingTargets.TryGetValue(instanceType, out var awaitingTargets)) {
            awaitingTargets = new() { Dependencies = new(), Targets = new() };
            WaitingTargets.Add(instanceType, awaitingTargets);
        }
        awaitingTargets.Targets.Add(instanceDelayedDI);
        awaitingTargets.Dependencies.Add(awaitingDependencyType);
        if (!WaitingDependencies.TryGetValue(awaitingDependencyType, out var dependencies)) {
            dependencies = new();
            WaitingDependencies.Add(awaitingDependencyType, dependencies);
        }
        dependencies.Add(instanceType);
    }

    public static void AddResolvedDependency(Type resolvedType) {
        if (!WaitingDependencies.TryGetValue(resolvedType, out var awaitingTargetTypes))
            return;
        foreach (var awaitingType in awaitingTargetTypes) {
            if (WaitingTargets.TryGetValue(awaitingType, out var targets)) {
                targets.Dependencies.Remove(resolvedType);
                if (targets.Dependencies.Count == 0) {
                    foreach (var target in targets.Targets) {
                        DependencyInjector.ResolveDependencies(target, awaitingType);
                        target.OnDependenciesInjected();
                    }
                    WaitingTargets.Remove(awaitingType);
                }
            }
        }
        WaitingDependencies.Remove(resolvedType);
    }
}
