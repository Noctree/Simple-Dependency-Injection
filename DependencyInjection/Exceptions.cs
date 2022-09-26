using System;

namespace NokLib.DependencyInjection.Exceptions;

[Serializable]
public sealed class TypeAlreadyRegisteredException : ArgumentException
{
    public Type InstanceType { get; }
    public TypeAlreadyRegisteredException() : base() {
        InstanceType = typeof(object);
    }

    public TypeAlreadyRegisteredException(Type instanceType) : base($"Instance of type {instanceType.FullName} is already registered") {
        InstanceType = instanceType;
    }

    public TypeAlreadyRegisteredException(Type instanceType, string paramName) : base($"Instance of type {instanceType.FullName} from parameter {paramName} is already registered", paramName) {
        InstanceType = instanceType;
    }

    public TypeAlreadyRegisteredException(Type instanceType, string message, Exception innerException) : base(message, innerException) {
        InstanceType = instanceType;
    }

    public TypeAlreadyRegisteredException(Type instanceType, string message, string paramName) : base(message, paramName) {
        InstanceType = instanceType;
    }

    public TypeAlreadyRegisteredException(Type instanceType, string message, string paramName, Exception innerException) : base(message, paramName, innerException) {
        InstanceType = instanceType;
    }
}

[Serializable]
public sealed class DependencyNotRegisteredException : Exception
{
    public Type DependencyType { get; }
    public Type InstanceType { get; }

    public DependencyNotRegisteredException(Type dependency, Type instance) : base($"Object of type {instance.FullName} requested {dependency.FullName} as its dependency, but {dependency.FullName} has not been registered in {nameof(DependencyInjector)}") {
        DependencyType = dependency;
        InstanceType = instance;
    }
}

[Serializable]
public sealed class DITypeNotSupportedException : NotSupportedException
{
    public Type InstanceType { get; }

    public DITypeNotSupportedException(Type instanceType) : base($"{nameof(DependencyInjector)} only supports class types, {instanceType.FullName} is not a class") {
        InstanceType = instanceType;
    }
}