using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleDI.Internal.Utilities;

/// <summary>
/// Utility class for runtime code generation
/// </summary>
internal static class CodeGen
{
    public static ServiceInjectionSetter CreateSetterDelegate(PropertyInfo property)
    {
        var instanceParameter = Expression.Parameter(typeof(object), "instance");
        var valueParameter = Expression.Parameter(typeof(object), "service");
        var castInstance = 
            Expression.Convert(
                instanceParameter,
                property.DeclaringType
                    ?? throw new MissingMemberException(
                    $"Property {property.Name} is missing a declaring type.")
                );
        var castValue = Expression.Convert(valueParameter, property.PropertyType);
        var setMethod = property.GetSetMethod(true)
                        ?? throw new MissingMemberException(
                            $"Property {property.Name} is missing a set method.");
        var callMethod = Expression.Call(castInstance, setMethod, castValue);
        var lambda = Expression.Lambda<ServiceInjectionSetter>(callMethod, instanceParameter, valueParameter);
        return lambda.Compile();
    }

    public static ServiceInjectionSetter CreateSetterDelegate(FieldInfo field)
    {
        var instanceParameter = Expression.Parameter(typeof(object), "instance");
        var valueParameter = Expression.Parameter(typeof(object), "value");
        var castInstance = Expression.Convert(
            instanceParameter,
            field.DeclaringType
                ?? throw new MissingMemberException($"Field {field.Name} is missing a declaring type."));
        var castValue = Expression.Convert(valueParameter, field.FieldType);
        var fieldAccess = Expression.Field(castInstance, field);
        var assign = Expression.Assign(fieldAccess, castValue);
        var lambda = Expression.Lambda<ServiceInjectionSetter>(assign, instanceParameter, valueParameter);
        return lambda.Compile();
    }
}