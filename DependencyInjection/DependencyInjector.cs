using System;
using System.Collections.Generic;
using System.Reflection;
using NokLib.DependencyInjection.Exceptions;
using NokLib.DependencyInjection.Internal;

namespace NokLib.DependencyInjection;

public readonly record struct FieldInjectionInfo(Type DependencyType, FieldInfo Info);
public readonly record struct PropertyInjectionInfo(Type DependencyType, MethodInfo Setter);
public sealed record ObjectInjectionInfo(FieldInjectionInfo[] FieldInjectionInfo, PropertyInjectionInfo[] PropertyInjectionInfo);
public static class DependencyInjector
{
    private static readonly object[] argArr = new object[1];
    private static readonly Dictionary<Type, object> Dependencies = new();
    private static readonly Dictionary<Type, ObjectInjectionInfo> InjectionInfo = new();
    public static void RegisterDependency<T>(T instance) where T : class {
        if (IsDependencyRegistered<T>())
            throw new TypeAlreadyRegisteredException(typeof(T), nameof(instance));
        Dependencies.Add(typeof(T), instance);
        DelayedDependencyResolver.AddResolvedDependency(typeof(T));
    }

    public static void UnregisterDependency<T>() where T : class => Dependencies.Remove(typeof(T));

    public static bool IsDependencyRegistered<T>(T _) where T : class => IsDependencyRegistered<T>();
    public static bool IsDependencyRegistered<T>() where T : class => Dependencies.ContainsKey(typeof(T));
    public static bool ResolveDependencies<T>(T instance) => ResolveDependencies(instance, typeof(T));

    public static bool ResolveDependencies(object instance, Type type) {
        if (!type.IsClass) {
            throw new DITypeNotSupportedException(type);
        }

        if (!InjectionInfo.ContainsKey(type)) {
            var injInfo = DependencyReflectionUtils.GenerateInjectionInfoForType(type);
            InjectionInfo.Add(type, new(injInfo.Item1, injInfo.Item2));
        }
        var objectInjectionInfo = InjectionInfo[type];
        bool success = true;
        IDelayedDependencyInjection? delayedObject = instance as IDelayedDependencyInjection;
        foreach (var fieldInjInfo in objectInjectionInfo.FieldInjectionInfo) {
            if (!Dependencies.TryGetValue(fieldInjInfo.DependencyType, out var dependency)) {
                if (delayedObject is not null)
                    DelayedDependencyResolver.AddAwaitingTarget(delayedObject, type, fieldInjInfo.DependencyType);
                else
                    throw new DependencyNotRegisteredException(fieldInjInfo.DependencyType, type);
                success = false;
            }
            fieldInjInfo.Info.SetValue(instance, dependency);
        }
        foreach (var propInjInfo in objectInjectionInfo.PropertyInjectionInfo) {
            if (!Dependencies.TryGetValue(propInjInfo.DependencyType, out var dependency)) {
                if (delayedObject is not null)
                    DelayedDependencyResolver.AddAwaitingTarget(delayedObject, type, propInjInfo.DependencyType);
                else
                    throw new DependencyNotRegisteredException(propInjInfo.DependencyType, type);
                success = false;
            }
            argArr[0] = dependency;
            propInjInfo.Setter.Invoke(instance, argArr);
        }
        return success;
    }
}
