using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Internal.Utilities;

namespace SimpleDI;

internal static class DelegateInjectionInfoDatabase
{
    private static readonly Dictionary<Type, ObjectFactory> ConstructorFactories = new();
    private static readonly Dictionary<Type, DelegateObjectInjectionInfo> SetterDelegates = new();

    public static ObjectFactory GetConstructorObjectFactory(Type type)
    {
        if (ConstructorFactories.TryGetValue(type, out var factory))
            return factory;
        var constructorInjectionInfo = ReflectionInjectionInfoDatabase.GetConstructorInjectionInfo(type, false);
        factory = ActivatorUtilities.CreateFactory(type, constructorInjectionInfo.ArgumentTypes);
        ConstructorFactories.Add(type, factory);
        return factory;
    }

    public static DelegateObjectInjectionInfo GetDelegateInjectionInfo(Type type)
    {
        if (SetterDelegates.TryGetValue(type, out var injectionInfo))
            return injectionInfo;
        injectionInfo = DependencyReflectionUtils.GenerateDelegateObjectInjectionInfoForType(type);
        SetterDelegates.Add(type, injectionInfo);
        return injectionInfo;
    }
}