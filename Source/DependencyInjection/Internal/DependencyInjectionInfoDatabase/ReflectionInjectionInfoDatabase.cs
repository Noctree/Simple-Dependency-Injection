using SimpleDI.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleDI.Internal.Utilities;

namespace SimpleDI;
internal static class ReflectionInjectionInfoDatabase
{
    private static readonly Dictionary<Type, ObjectInjectionInfo> PropertyDatabase = new();
    private static readonly Dictionary<Type, ConstructorInjectionInfo> ConstructorDatabase = new();

    public static ObjectInjectionInfo GetPropertyInjectionInfo(Type type)
    {
        if (PropertyDatabase.TryGetValue(type, out var injectionInfo))
            return injectionInfo;
        injectionInfo = DependencyReflectionUtils.GenerateInjectionInfoForType(type);
        PropertyDatabase.Add(type, injectionInfo);
        return injectionInfo;
    }

    public static ConstructorInjectionInfo GetConstructorInjectionInfo(Type type, bool preallocateArgArray = false)
    {
        if (ConstructorDatabase.TryGetValue(type, out var constructorInjectionInfo))
            return constructorInjectionInfo;
        constructorInjectionInfo = DependencyReflectionUtils.GetConstructorInjectionInfo(type, preallocateArgArray);
        ConstructorDatabase.Add(type, constructorInjectionInfo);
        return constructorInjectionInfo;
    }
}
