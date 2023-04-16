using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;

namespace SimpleDI;

public static class AdvancedServiceCollectionExtensions
{
    public static IAdvancedServiceCollection Unregister<TService>(this IAdvancedServiceCollection serviceCollection)
        where TService : class
    {
        serviceCollection.RemoveByType(typeof(TService));
        return serviceCollection;
    }

    public static IAdvancedServiceCollection RegisterSingleton<TService>(
        this IAdvancedServiceCollection serviceCollection,
        Func<IServiceProvider, TService> factory) where TService : class
    {
        serviceCollection.Add(ServiceDescriptor.Singleton(factory));
        return serviceCollection;
    }

    public static IAdvancedServiceCollection RegisterSingleton<TService, TImplementation>(
        this IAdvancedServiceCollection serviceCollection,
        Func<IServiceProvider, TImplementation> factory) where TService : class where TImplementation : class, TService
    {
        serviceCollection.Add(ServiceDescriptor.Singleton<TService, TImplementation>(factory));
        return serviceCollection;
    }

    public static IAdvancedServiceCollection RegisterSingleton<TService, TImplementation>(
        this IAdvancedServiceCollection serviceCollection)
        where TService : class where TImplementation : class, TService
    {
        serviceCollection.Add(ServiceDescriptor.Singleton<TService, TImplementation>());
        return serviceCollection;
    }

    public static IAdvancedServiceCollection RegisterSingleton<TService>(
        this IAdvancedServiceCollection serviceCollection, TService implementation)
        where TService : class
    {
        serviceCollection.Add(ServiceDescriptor.Singleton(implementation));
        return serviceCollection;
    }

    public static IAdvancedServiceCollection RegisterTransient<TService, TImplementation>(
        this IAdvancedServiceCollection serviceCollection)
        where TService : class where TImplementation : class, TService
    {
        serviceCollection.Add(ServiceDescriptor.Transient<TService, TImplementation>());
        return serviceCollection;
    }

    public static IAdvancedServiceCollection RegisterTransient<TService>(
        this IAdvancedServiceCollection serviceCollection,
        Func<IServiceProvider, TService> factory) where TService : class
    {
        serviceCollection.Add(ServiceDescriptor.Transient(typeof(TService), factory));
        return serviceCollection;
    }

    public static IAdvancedServiceCollection RegisterTransient<TService, TImplementation>(
        this IAdvancedServiceCollection serviceCollection,
        Func<IServiceProvider, TImplementation> factory) where TService : class where TImplementation : class, TService
    {
        serviceCollection.Add(ServiceDescriptor.Transient<TService, TImplementation>(factory));
        return serviceCollection;
    }

    public static IAdvancedServiceCollection RegisterScoped<TService>(this IAdvancedServiceCollection serviceCollection,
        Func<IServiceProvider, TService> factory)
        where TService : class
    {
        serviceCollection.Add(ServiceDescriptor.Scoped(factory));
        return serviceCollection;
    }

    public static IAdvancedServiceCollection RegisterScoped<TService, TImplementation>(
        this IAdvancedServiceCollection serviceCollection,
        Func<IServiceProvider, TImplementation> factory) where TService : class where TImplementation : class, TService
    {
        serviceCollection.Add(ServiceDescriptor.Scoped<TService, TImplementation>(factory));
        return serviceCollection;
    }

    public static bool IsRegistered<TService>(this IAdvancedServiceCollection serviceCollection) where TService : class =>
        serviceCollection.IsService(typeof(TService));
}