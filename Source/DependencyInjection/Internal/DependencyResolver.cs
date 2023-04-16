using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;
using SimpleDI.Internal.Utilities;

namespace SimpleDI.Internal;

/// <summary>
///Responsible for injecting dependencies into instances, or creating them trough constructors
///Also responsible for picking the injection strategy
/// </summary>
internal static class DependencyResolver
{
    [ThreadStatic] private static List<object>? argumentList;
    private static readonly Dictionary<Type, bool> ConstructedTypes = new();

    public static DependencyResolutionResult ResolveDependencies(
            object instance,
            IServiceProvider serviceProvider)
    {
        return DependencyInjectionSettings.UseCompatibilityMethodForProperties
            ? ResolveDependenciesTroughPropertiesWithCompatibility(instance, instance.GetType(), serviceProvider)
            : ResolveDependenciesTroughProperties(instance, instance.GetType(), serviceProvider);
    }

    internal static ServiceConstructionResult TryConstructService(
        ServiceDescriptor descriptor,
        IAdvancedServiceProvider serviceProvider)
    {
        var implementationType = descriptor.GetImplementationType();
        if (!ConstructedTypes.TryGetValue(implementationType, out var requiresConstruction))
        {
            ConstructedTypes.TryAdd(implementationType,
                DependencyReflectionUtils.HasCustomAttribute<InjectTroughConstructorAttribute>(implementationType));
        }

        if (requiresConstruction)
        {
            return DependencyInjectionSettings.UseCompatibilityMethodForConstructors
                ? ResolveDependenciesTroughConstructorWithCompatibility(implementationType, serviceProvider, descriptor.Lifetime)
                : ResolveDependenciesTroughConstructor(implementationType, serviceProvider);
        }
        else
        {
            var instance = (descriptor.ImplementationFactory is null
                ? Activator.CreateInstance(implementationType)
                : descriptor.ImplementationFactory(serviceProvider))
                ?? throw new ConstructorException(implementationType, "Failed to create an instance of service.");
            
            var result = DependencyInjectionSettings.UseCompatibilityMethodForProperties
                ? ResolveDependenciesTroughPropertiesWithCompatibility(instance, implementationType, serviceProvider)
                : ResolveDependenciesTroughProperties(instance, implementationType, serviceProvider);
            return new(result.Success, result.Success? instance : null, result.MissingDependency);
        }
    }

    private static ServiceConstructionResult ResolveDependenciesTroughConstructor(Type type, IAdvancedServiceProvider serviceProvider)
    {
        var checkResult = CheckServiceAvailability(type, serviceProvider);
        if (!checkResult.Success)
            return new ServiceConstructionResult(false, null, checkResult.MissingDependency);
        var factory = DelegateInjectionInfoDatabase.GetConstructorObjectFactory(type);
        var instance = factory(serviceProvider, null);
        return new ServiceConstructionResult(true, instance, null);
    }

    private static DependencyResolutionResult CheckServiceAvailability(Type instanceType, IServiceProviderIsService isService)
    {
        var constructor = ReflectionInjectionInfoDatabase.GetConstructorInjectionInfo(instanceType);
        foreach (var type in constructor.ArgumentTypes)
            if (!isService.IsService(type))
                return new DependencyResolutionResult(false, type);
        return new DependencyResolutionResult(true, null);
    }

    private static ServiceConstructionResult ResolveDependenciesTroughConstructorWithCompatibility(Type type,
        IServiceProvider serviceProvider,
        ServiceLifetime lifetime)
    {
        if (!type.IsClass)
            throw new TypeNotSupportedException(type);
        
        var constructorInjectionInfo = ReflectionInjectionInfoDatabase.GetConstructorInjectionInfo(type,
            lifetime != ServiceLifetime.Singleton);
        argumentList ??= new();

        foreach (var argumentType in constructorInjectionInfo.ArgumentTypes)
        {
            if (serviceProvider.TryGetService(argumentType, out var service))
            {
                argumentList.Add(service);
            }
            else
                return new(false, null, argumentType);
        }

        var instance = constructorInjectionInfo.CreateInstance(argumentList)
            ?? throw new SimpleDependencyInjectionException($"Failed to create an instance of type {type.FullName}");
        argumentList.Clear();
        return new ServiceConstructionResult(true, instance, null);
    }

    private static DependencyResolutionResult ResolveDependenciesTroughProperties(
        object instance,
        Type instanceType,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (!instanceType.IsClass)
            throw new TypeNotSupportedException(instanceType);

        var injectionSetters = DelegateInjectionInfoDatabase.GetDelegateInjectionInfo(instanceType);
        var success = true;
        IDelayedDependencyInjection? delayedObject = instance as IDelayedDependencyInjection;
        foreach (var (injectorDelegate, serviceType) in injectionSetters.Setters)
        {
            if (serviceProvider.TryGetService(serviceType, out var dependency))
            {
                injectorDelegate(instance, dependency);
            }
            else
                return new DependencyResolutionResult(false, serviceType);
        }
        if (!success && delayedObject is not null)
            delayedObject.MarkForDelayedInjection();
        return new DependencyResolutionResult(true, null);
    }
    
    private static DependencyResolutionResult ResolveDependenciesTroughPropertiesWithCompatibility(object instance,
        Type instanceType,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (!instanceType.IsClass)
            throw new TypeNotSupportedException(instanceType);

        var objectInjectionInfo = ReflectionInjectionInfoDatabase.GetPropertyInjectionInfo(instanceType);
        foreach (var injectionInfo in objectInjectionInfo.InjectionInfo)
        {
            if (serviceProvider.TryGetService(injectionInfo.DependencyType, out var dependency))
            {
                injectionInfo.SetValue(instance, dependency);
            }
            else
                return new DependencyResolutionResult(false, injectionInfo.DependencyType);
        }
        return new DependencyResolutionResult(true, null);
    }
}