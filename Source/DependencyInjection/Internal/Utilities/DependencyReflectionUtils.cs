using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleDI.Internal.Utilities;

internal static class DependencyReflectionUtils
{
    private static BindingFlags SearchBindingFlags => BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    internal static bool HasCustomAttribute<TAttribute>(ICustomAttributeProvider info, bool inherit = false)
        where TAttribute : Attribute =>
        info.IsDefined(typeof(TAttribute), inherit);

    internal static ConstructorInjectionInfo GetConstructorInjectionInfo(Type type, bool preallocateArgArray)
    {
        if (type.IsAbstract)
            throw new ConstructorException(type, "Cannot create an instance of an abstract type.");
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (constructors.Length == 0)
            throw new ConstructorException(type, "No constructor found");

        var bestMatch = FindBestConstructor(type, constructors);

        if (bestMatch is null)
            throw new ConstructorException(type, "No suitable constructor found");

        return new(type, bestMatch, preallocateArgArray);
    }

    private static ConstructorInfo? FindBestConstructor(Type type, ConstructorInfo[] constructors)
    {
        ConstructorInfo? bestMatch = null;
        var bestLength = 0;
        var seenPrefered = false;

        foreach (var constructor in constructors)
        {
            var isPreferred = HasCustomAttribute<ActivatorUtilitiesConstructorAttribute>(constructor)
                              || HasCustomAttribute<InjectorConstructorAttribute>(constructor);

            if (isPreferred && seenPrefered)
            {
                throw new ConstructorException(type, "Multiple constructors tagged as preferred found");
            }

            var parameters = constructor.GetParameters();
            if (!AreValidParameters(parameters))
            {
                if (isPreferred)
                    throw new ConstructorException(type,
                        "Parameters of an explicitly tagged constructor must be only classes or must be parameterless");
                continue;
            }

            if (bestMatch is null || (parameters.Length > bestLength && !seenPrefered) || isPreferred)
            {
                bestLength = parameters.Length;
                bestMatch = constructor;
            }

            seenPrefered |= isPreferred;
        }

        return bestMatch;
    }

    internal static bool AreValidParameters(ParameterInfo[] parameters)
    {
        foreach (var parameter in parameters)
        {
            if (!parameter.ParameterType.IsClass)
                return false;
        }

        return true;
    }

    internal static bool ValidateMemberStorageTypeOrThrow(Type type)
    {
        if (type.IsArray || !type.IsClass || type.IsGenericType)
            throw new TypeNotSupportedException(type);
        return true;
    }

    internal static bool ValidateMemberInfoOrThrow(MemberInfo info) =>
        info switch
        {
            FieldInfo fieldInfo => fieldInfo.IsStatic || fieldInfo.IsLiteral || fieldInfo.IsInitOnly
                ? throw new MemberNotSupportedException(info.DeclaringType!, info.Name,
                    "Field cannot be static, const, or readonly")
                : ValidateMemberStorageTypeOrThrow(fieldInfo.FieldType),
            PropertyInfo propertyInfo => ValidateMemberStorageTypeOrThrow(propertyInfo.PropertyType),
            _ => throw new MemberNotSupportedException(info.DeclaringType!, info.Name,
                "Member cannot be static, const, or readonly"),
        };

    internal static IEnumerable<FieldInfo> EnumerateInjectedFieldsOf(Type type) =>
        type.GetFields(SearchBindingFlags)
            .Where(static field => HasCustomAttribute<InjectAttribute>(field))
            .Where(static field => ValidateMemberInfoOrThrow(field));

    internal static IEnumerable<PropertyInfo> EnumerateInjectedPropertiesOf(Type type) =>
        type.GetProperties(SearchBindingFlags)
            .Where(static prop => HasCustomAttribute<InjectAttribute>(prop))
            .Where(static prop => ValidateMemberInfoOrThrow(prop));

    internal static ObjectInjectionInfo GenerateInjectionInfoForType(Type type)
    {
        var fieldInfo =
            EnumerateInjectedFieldsOf(type)
                .Select(static field => new FieldInjectionInfo(field.FieldType, field));
        var propInfo =
            EnumerateInjectedPropertiesOf(type)
                .Select(static prop => new PropertyInjectionInfo(prop.PropertyType, prop));

        return new ObjectInjectionInfo(fieldInfo.Cast<InjectionInfo>().Concat(propInfo).ToArray());
    }

    internal static DelegateObjectInjectionInfo GenerateDelegateObjectInjectionInfoForType(Type type)
    {
        var fieldSetters = EnumerateInjectedFieldsOf(type)
            .Select(static field => new SetterDelegateInfo(CodeGen.CreateSetterDelegate(field), field.FieldType));
        var propSetters = EnumerateInjectedPropertiesOf(type)
            .Select(static prop => new SetterDelegateInfo(CodeGen.CreateSetterDelegate(prop), prop.PropertyType));
        return new DelegateObjectInjectionInfo(fieldSetters.Concat(propSetters).ToArray(), type);
    }
}