using SimpleDI.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDI;
internal static class InjectionInfoDatabase
{
    private static readonly Dictionary<Type, ObjectInjectionInfo> _database = new();

    public static ObjectInjectionInfo GetInjectionInfo(Type type)
    {
        if (!_database.TryGetValue(type, out var injectionInfo))
        {
            injectionInfo = DependencyReflectionUtils.GenerateInjectionInfoForType(type);
            _database.Add(type, injectionInfo);
        }
        return injectionInfo;
    }
}
