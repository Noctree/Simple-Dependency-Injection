using System;
using System.Reflection;

namespace SimpleDI;

internal abstract record InjectionInfo(Type DependencyType, MemberInfo MemberInfo)
{
    public abstract void SetValue(object instance, object value);
}
internal record FieldInjectionInfo(Type DependencyType, FieldInfo FieldInfo) : InjectionInfo(DependencyType, FieldInfo)
{
    public override void SetValue(object instance, object value)
    {
        FieldInfo.SetValue(instance, value);
    }
}

internal record PropertyInjectionInfo(Type DependencyType, MethodInfo Setter) : InjectionInfo(DependencyType, Setter)
{
    [ThreadStatic] private static readonly object[] argArray = new object[1];
    public override void SetValue(object instance, object value)
    {
        argArray[0] = value;
        Setter.Invoke(instance, argArray);
    }
}

internal sealed record ObjectInjectionInfo(InjectionInfo[] InjectionInfo);
