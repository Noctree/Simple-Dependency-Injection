using System;
using System.Collections.Generic;
using SimpleDI.Containers;
using SimpleDI.Internal;
using System.Diagnostics.CodeAnalysis;

namespace SimpleDI;

public static class DependencyInjector
{
    internal const string GlobalScopeId = "global-scope";
    private static readonly Container GlobalContainer = new(GlobalScopeId, null);
    private static readonly Dictionary<string, Container> _scopes = new();

    /// <summary>
    /// Register an instance of an object as the dependency instance for it's type. <br/> <b>This method applies for the global scope</b>
    /// </summary>
    /// <typeparam name="T">Type of the dependency</typeparam>
    /// <param name="instance">Instance to resolve any dependency for this type to</param>
    public static void RegisterDependency<T>(T instance) where T : class =>
        GlobalContainer.RegisterDependency(instance);

    /// <summary>
    /// Unregisters any instance of this type from the scope <br/> <b>This method applies for the global scope</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    public static void UnregisterDependency<T>(T _) where T : class =>
        UnregisterDependency<T>();

    /// <summary>
    /// Unregisters any instance of this type from the scope <br/> <b>This method applies for the global scope</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void UnregisterDependency<T>() where T : class =>
        GlobalContainer.UnregisterDependency<T>();

    /// <summary>
    /// Check if a dependency instance for type <typeparamref name="T"/> is registered <br/> <b>This method applies for the global scope</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <returns>True if an instance for type <typeparamref name="T"/> is registered</returns>
    public static bool IsDependencyRegistered<T>(T _) where T : class => IsDependencyRegistered<T>();

    /// <summary>
    /// Check if a dependency instance for type <typeparamref name="T"/> is registered <br/> <b>This method applies for the global scope</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>True if an instance for type <typeparamref name="T"/> is registered</returns>
    public static bool IsDependencyRegistered<T>() where T : class => GlobalContainer.IsDependencyRegistered(typeof(T));

    /// <summary>
    /// Attempts to resolve all dependencies of <paramref name="instance"/> <br/> <b>This method applies for the global scope</b>
    /// </summary>
    /// <param name="instance">The instance which dependecies should be resolved</param>
    /// <returns>True if all dependencies were resolved successfuly</returns>
    public static bool ResolveDependencies(object instance) =>
        GlobalContainer.ResolveDependencies(instance);

    /// <summary>
    /// Creates a new empty dependency injection scope
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fallbackToGlobalScope">Whenever to allow the created scope to also search the global scope for missing dependencies</param>
    /// <returns>The created scope</returns>
    /// <exception cref="DuplicitScopeIdException"></exception>
    public static Container CreateScope(string? id = null, bool fallbackToGlobalScope = true)
    {
        if (id is null)
            id = Guid.NewGuid().ToString();
        if (_scopes.ContainsKey(id))
            throw new DuplicitScopeIdException(id);
        var scope = new Container(id, fallbackToGlobalScope? GlobalContainer : null);
        _scopes.Add(id, scope);
        return scope;
    }

    /// <summary>
    /// Get a scope by it's ID
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Container GetScope(string id)
    {
        if (!_scopes.ContainsKey(id))
            throw new KeyNotFoundException($"Scope with id {id} does not exist");
        return _scopes[id];
    }

    /// <summary>
    /// Try to get a scope by it's id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="scope"></param>
    /// <returns>True if the scope exists</returns>
    public static bool TryGetScope(string id,[NotNullWhen(true)] out Container? scope) => _scopes.TryGetValue(id, out scope);

    public static void DeleteScope(Container container) => _scopes.Remove(container.Id);
}
