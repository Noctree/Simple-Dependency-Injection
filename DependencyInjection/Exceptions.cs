using System;
using System.Runtime.Serialization;

namespace SimpleDI;

[Serializable]
public class SimpleDependencyInjectionException : Exception
{
    public SimpleDependencyInjectionException() { }

    public SimpleDependencyInjectionException(string? message) : base(message) { }

    public SimpleDependencyInjectionException(string? message, Exception? innerException) : base(message, innerException) { }

    protected SimpleDependencyInjectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class TypeAlreadyRegisteredException : SimpleDependencyInjectionException
{
    public const string UnknownParameterName = "_Unknown_";
    public Type InstanceType { get; }
    public string ParameterName { get; } = UnknownParameterName;
    public TypeAlreadyRegisteredException() : base() {
        InstanceType = typeof(object);
    }

    public TypeAlreadyRegisteredException(Type instanceType) : base($"Instance of type {instanceType.FullName} is already registered") {
        InstanceType = instanceType;
    }

    public TypeAlreadyRegisteredException(Type instanceType, string paramName) : base($"Instance of type {instanceType.FullName} from parameter {paramName} is already registered") {
        InstanceType = instanceType;
    }
}

[Serializable]
public sealed class DependencyNotRegisteredException : SimpleDependencyInjectionException
{
    public Type DependencyType { get; }
    public Type InstanceType { get; }

    public DependencyNotRegisteredException(Type dependency, Type instance) : base($"Object of type {instance.FullName} requested {dependency.FullName} as its dependency, but {dependency.FullName} has not been registered in {nameof(DependencyInjector)}") {
        DependencyType = dependency;
        InstanceType = instance;
    }
}

[Serializable]
public sealed class DITypeNotSupportedException : SimpleDependencyInjectionException
{
    public Type InstanceType { get; }

    public DITypeNotSupportedException(Type instanceType) : base($"{nameof(DependencyInjector)} only supports class types, {instanceType.FullName} is not a class") {
        InstanceType = instanceType;
    }
}

[Serializable]
public sealed class DuplicitScopeIdException : SimpleDependencyInjectionException
{
    public string Id { get; }
    public DuplicitScopeIdException(string id) : base($"A scope with ID {id} already exists")
    {
        Id = id;
    }
}