using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;

namespace SimpleDI;

public static class ContainerExtensions
{
    public static IContainer Unregister<TService>(this IContainer container)
        where TService : class
    {
        container.ServiceCollection.RemoveByType(typeof(TService));
        return container;
    }

    public static IContainer RegisterSingleton<TService>(
        this IContainer container,
        Func<IServiceProvider, TService> factory) where TService : class
    {
        container.ServiceCollection.Add(ServiceDescriptor.Singleton(factory));
        return container;
    }

    public static IContainer RegisterSingleton<TService, TImplementation>(
        this IContainer container,
        Func<IServiceProvider, TImplementation> factory) where TService : class where TImplementation : class, TService
    {
        container.ServiceCollection.Add(ServiceDescriptor.Singleton<TService, TImplementation>(factory));
        return container;
    }

    public static IContainer RegisterSingleton<TService, TImplementation>(
        this IContainer container)
        where TService : class where TImplementation : class, TService
    {
        container.ServiceCollection.Add(ServiceDescriptor.Singleton<TService, TImplementation>());
        return container;
    }

    public static IContainer RegisterSingleton<TService>(
        this IContainer container, TService implementation)
        where TService : class
    {
        container.ServiceCollection.Add(ServiceDescriptor.Singleton(implementation));
        return container;
    }

    public static IContainer RegisterTransient<TService, TImplementation>(
        this IContainer container)
        where TService : class where TImplementation : class, TService
    {
        container.ServiceCollection.Add(ServiceDescriptor.Transient<TService, TImplementation>());
        return container;
    }

    public static IContainer RegisterTransient<TService>(
        this IContainer container,
        Func<IServiceProvider, TService> factory) where TService : class
    {
        container.ServiceCollection.Add(ServiceDescriptor.Transient(typeof(TService), factory));
        return container;
    }

    public static IContainer RegisterTransient<TService, TImplementation>(
        this IContainer container,
        Func<IServiceProvider, TImplementation> factory) where TService : class where TImplementation : class, TService
    {
        container.ServiceCollection.Add(ServiceDescriptor.Transient<TService, TImplementation>(factory));
        return container;
    }

    public static IContainer RegisterScoped<TService>(this IContainer container,
        Func<IServiceProvider, TService> factory)
        where TService : class
    {
        container.ServiceCollection.Add(ServiceDescriptor.Scoped(factory));
        return container;
    }

    public static IContainer RegisterScoped<TService, TImplementation>(
        this IContainer container,
        Func<IServiceProvider, TImplementation> factory) where TService : class where TImplementation : class, TService
    {
        container.ServiceCollection.Add(ServiceDescriptor.Scoped<TService, TImplementation>(factory));
        return container;
    }

    public static bool IsRegistered<TService>(this IContainer container) where TService : class =>
        container.ServiceCollection.IsService(typeof(TService));
}