using System;
using System.Collections.Generic;
using SimpleDI.Internal;

namespace SimpleDI.Containers;

/// <summary>
/// Similar in function to scopes in dependency injection, except that containers can be assigned a specific ID and then shared between objects by that ID.
/// Essentially a scope that can be created by one object and then shared with a different one instead of both objects creating their own scope.
/// Each container can optionally also fall back onto the global container, in case the required dependency is not present.
/// </summary>
public class Container
{
    private readonly Container? _globalScope;
    private readonly Dictionary<Type, object> _dependencies = new();
    private readonly DelayedDependencyResolver _delayedDependencyResolver;

    /// <summary>
    /// Id of the scope
    /// </summary>
    public string Id { get; init; }
    /// <summary>
    /// If true, tries searching the global scope for dependencies as well, <br/> in case the dependency is not registered in the current scope
    /// </summary>
    public bool FallbackToGlobalScope => _globalScope is not null;
    
    public Container() : this(Guid.NewGuid().ToString(), null) { }

    public Container(string id, Container? globalScope)
    {
        Id = id;
        _globalScope = globalScope;
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Must not be an empty string, or whitespace", nameof(id));
        _delayedDependencyResolver = new(this);
    }

    public void RegisterDependency<T>(T instance) where T : class
    {
        if (IsDependencyRegistered<T>())
            throw new TypeAlreadyRegisteredException(typeof(T), nameof(instance));
        _dependencies.Add(typeof(T), instance);
        _delayedDependencyResolver.AddResolvedDependency(typeof(T));
    }

    public void UnregisterDependency<T>() where T : class => _dependencies.Remove(typeof(T));

    public bool IsDependencyRegistered<T>(T _) where T : class => IsDependencyRegistered<T>();
    public bool IsDependencyRegistered<T>() where T : class => _dependencies.ContainsKey(typeof(T));

    public bool ResolveDependencies(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var type = instance.GetType();
        if (!type.IsClass)
            throw new DITypeNotSupportedException(type);

        var missingDependencies = new List<InjectionInfo>();
        var objectInjectionInfo = InjectionInfoDatabase.GetInjectionInfo(type);
        bool success = true;
        IDelayedDependencyInjection? delayedObject = instance as IDelayedDependencyInjection;
        foreach (var injectionInfo in objectInjectionInfo.InjectionInfo)
        {
            if (_dependencies.TryGetValue(injectionInfo.DependencyType, out var dependency)
                || _globalScope?._dependencies.TryGetValue(injectionInfo.DependencyType, out dependency) == true)
            {
                injectionInfo.SetValue(instance, dependency);
            }
            else if (delayedObject is not null)
            {
                _delayedDependencyResolver.AddAwaitingTarget(delayedObject, type, injectionInfo.DependencyType);
                success = false;
            }
            else
                return false;
        }
        return success;
    }
}
