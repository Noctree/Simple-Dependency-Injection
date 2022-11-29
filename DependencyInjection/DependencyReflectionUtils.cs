using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleDI.Internal;
internal static class DependencyReflectionUtils
{
    internal static List<FieldInfo> GetInjectedFieldsOf(Type type) {
        var fields = new List<FieldInfo>();
        foreach (var field in type.GetRuntimeFields()) {
            if (field.IsStatic || field.IsInitOnly || field.IsLiteral)
                continue;
            var injectAttr = field.GetCustomAttribute<InjectAttribute>();
            if (injectAttr is not null)
                fields.Add(field);
        }
        return fields;
    }

    internal static List<PropertyInfo> GetInjectedPropertiesOf(Type type) {
        var props = new List<PropertyInfo>();
        foreach (var prop in type.GetRuntimeProperties()) {
            var injectAttr = prop.GetCustomAttribute<InjectAttribute>();
            if (injectAttr is not null && prop.CanWrite)
                props.Add(prop);
        }
        return props;
    }

    internal static ObjectInjectionInfo GenerateInjectionInfoForType(Type type) {
        var fieldInfo = new List<FieldInjectionInfo>();
        var propInfo = new List<PropertyInjectionInfo>();
        var injectedFields = GetInjectedFieldsOf(type);
        var injectedProps = GetInjectedPropertiesOf(type);
        foreach (var field in injectedFields)
            fieldInfo.Add(new FieldInjectionInfo(field.FieldType, field));
        foreach (var prop in injectedProps)
            propInfo.Add(new PropertyInjectionInfo(prop.PropertyType, prop.GetSetMethod(true) ?? throw new MissingMethodException(type.FullName, prop.Name + ".get()")));
        
        var injectionInfo = new InjectionInfo[fieldInfo.Count + propInfo.Count];
        fieldInfo.CopyTo((FieldInjectionInfo[])injectionInfo, 0);
        propInfo.CopyTo((PropertyInjectionInfo[])injectionInfo, fieldInfo.Count);
        return new ObjectInjectionInfo(injectionInfo);
    }
}
