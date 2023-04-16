using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleDI;

internal abstract record InjectionInfo(Type DependencyType)
{
    public abstract void SetValue(object instance, object value);
}
internal record FieldInjectionInfo(Type DependencyType, FieldInfo FieldInfo) : InjectionInfo(DependencyType)
{
    public override void SetValue(object instance, object value)
    {
        FieldInfo.SetValue(instance, value);
    }
}

internal record PropertyInjectionInfo(Type DependencyType, PropertyInfo Setter) : InjectionInfo(DependencyType)
{
    public override void SetValue(object instance, object value)
    {
        Setter.SetValue(instance, value);
    }
}

internal record ConstructorInjectionInfo(Type DependencyType, ConstructorInfo ConstructorInfo)
{
    public Type[] ArgumentTypes { get; }
    private readonly object[]? _argumentArray;
    public ConstructorInjectionInfo(Type dependencyType, ConstructorInfo constructorInfo, bool preallocateArgumentArray)
        : this(dependencyType, constructorInfo)
    {
        ArgumentTypes = constructorInfo.GetParameters().Select(static p => p.ParameterType).ToArray();
        if (preallocateArgumentArray)
            _argumentArray = new object[ArgumentTypes.Length];
    }
    
    public object? CreateInstance(IReadOnlyList<object> arguments)
    {
        if (arguments.Count != ArgumentTypes.Length)
            throw new SimpleDependencyInjectionException($"Expected {ArgumentTypes.Length} arguments, but got {arguments.Count}");
        var argArray = _argumentArray ?? new object[ArgumentTypes.Length];
        for (var i = 0; i < arguments.Count; ++i)
        {
            argArray[i] = arguments[i];
        }

        return ConstructorInfo.Invoke(argArray);
    }
}

internal sealed record ObjectInjectionInfo(InjectionInfo[] InjectionInfo);
