using System;
using System.Collections.Generic;
using SimpleDI.Containers;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleDI;

public static class DependencyInjector
{
    private static string GlobalContainerId => "global-container";
    private static readonly IContainer GlobalContainer = new Container(GlobalContainerId, null);
    private static readonly Dictionary<string, IContainer> Containers = new();

    /// <summary>
    /// Register an instance of an object as the dependency instance for it's type. <br/> <b>This method applies for the global container</b>
    /// </summary>
    /// <param name="descriptor">Instance to resolve any dependency for this type to</param>
    public static void RegisterDependency(ServiceDescriptor descriptor) =>
        GlobalContainer.RegisterService(descriptor);

    /// <summary>
    /// Unregisters any instance of this type from the container <br/> <b>This method applies for the global container</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    public static void UnregisterDependency<T>(T _) where T : class =>
        UnregisterDependency<T>();

    /// <summary>
    /// Unregisters any instance of this type from the container <br/> <b>This method applies for the global container</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void UnregisterDependency<T>() where T : class =>
        GlobalContainer.Unregister<T>();

    /// <summary>
    /// Check if a dependency instance for type <typeparamref name="T"/> is registered <br/> <b>This method applies for the global container</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <returns>True if an instance for type <typeparamref name="T"/> is registered</returns>
    public static bool IsDependencyRegistered<T>(T _) where T : class => IsDependencyRegistered<T>();

    /// <summary>
    /// Check if a dependency instance for type <typeparamref name="T"/> is registered <br/> <b>This method applies for the global container</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>True if an instance for type <typeparamref name="T"/> is registered</returns>
    public static bool IsDependencyRegistered<T>() where T : class => GlobalContainer.IsRegistered<T>();

    /// <summary>
    /// Attempts to resolve all dependencies of <paramref name="instance"/> <br/> <b>This method applies for the global container</b>
    /// </summary>
    /// <param name="instance">The instance which dependecies should be resolved</param>
    /// <returns>True if all dependencies were resolved successfuly</returns>
    public static void ResolveDependencies(object instance) =>
        GlobalContainer.ResolveDependencies(instance);

    /// <summary>
    /// Creates a new empty dependency injection container
    /// </summary>
    /// <returns>The created container</returns>
    /// <exception cref="DuplicitIdException"></exception>
    public static IContainer CreateContainer() => CreateContainer(Guid.NewGuid().ToString());

    /// <summary>
    /// Creates a new empty dependency injection container
    /// </summary>
    /// <param name="id">Container Id</param>
    /// <returns>The created container</returns>
    /// <exception cref="DuplicitIdException"></exception>
    public static IContainer CreateContainer(string id) => CreateContainer(id, true);

    /// <summary>
    /// Creates a new empty dependency injection container
    /// </summary>
    /// <param name="id">Container Id</param>
    /// <param name="fallbackToGlobalcontainer">Whenever to allow the created container to also search the global container for missing dependencies</param>
    /// <returns>The created container</returns>
    /// <exception cref="DuplicitIdException"></exception>
    public static IContainer CreateContainer(string id, bool fallbackToGlobalcontainer = true)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null, empty, or whitespace", nameof(id));
        if (Containers.ContainsKey(id))
            throw new DuplicitIdException(id);
        var container = new Container(id, fallbackToGlobalcontainer? GlobalContainer : null);
        Containers.Add(id, container);
        return container;
    }

    /// <summary>
    /// Get a container by its ID
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public static IContainer GetContainer(string id)
    {
        if (!Containers.ContainsKey(id))
            throw new KeyNotFoundException($"container with id {id} does not exist");
        return Containers[id];
    }

    /// <summary>
    /// Try to get a container by it's id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="container"></param>
    /// <returns>True if the container exists</returns>
    public static bool TryGetContainer(string id,[NotNullWhen(true)] out IContainer? container) => Containers.TryGetValue(id, out container);

    public static void DeleteContainer(IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);
        if (container.IsDisposed)
            return;
        container.Dispose();
        Containers.Remove(container.Id);
    }
}
