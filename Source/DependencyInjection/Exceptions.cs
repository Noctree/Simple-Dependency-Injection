using System;
using System.Runtime.Serialization;

namespace SimpleDI;

[Serializable]
public class SimpleDependencyInjectionException : Exception
{
    public SimpleDependencyInjectionException() { }

    public SimpleDependencyInjectionException(string? message) : base(message) { }

    public SimpleDependencyInjectionException(string? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    protected SimpleDependencyInjectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public sealed class TypeAlreadyRegisteredException : SimpleDependencyInjectionException
{
    public const string UnknownParameterName = "_Unknown_";


    public Type InstanceType { get; }
    public string ParameterName { get; } = UnknownParameterName;

    public TypeAlreadyRegisteredException(Type instanceType) : base($"Instance of type {instanceType.FullName} is already registered") {
        InstanceType = instanceType;
    }

    public TypeAlreadyRegisteredException(Type instanceType, string paramName) : base($"Instance of type {instanceType.FullName} from parameter {paramName} is already registered", null) {
        InstanceType = instanceType;
        ParameterName = paramName;
    }
    public TypeAlreadyRegisteredException(Type instanceType, string paramName, string? message, Exception? innerException) : base(message, innerException)
    {
        InstanceType = instanceType;
        ParameterName = paramName;
    }

    /// <inheritdoc />
    public TypeAlreadyRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        InstanceType = (Type)(info?.GetValue("InstanceType", typeof(Type)) ?? typeof(nuint));
        ParameterName = info?.GetString("ParameterName") ?? UnknownParameterName;
    }

    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(InstanceType), InstanceType);
        info.AddValue(nameof(ParameterName), ParameterName);
    }
}

[Serializable]
public sealed class DependencyNotRegisteredException : SimpleDependencyInjectionException
{
    public Type DependencyType { get; }
    public Type InstanceType { get; }

    public DependencyNotRegisteredException(Type dependency, Type instance)
        : this(dependency, instance, $"Object of type {instance.FullName} requested {dependency.FullName} as its dependency, but {dependency.FullName} has not been registered in {nameof(DependencyInjector)}") {}

    public DependencyNotRegisteredException(Type dependency, Type instance, string? message, Exception? innerException = null) : base(message, innerException)
    {
        DependencyType = dependency;
        InstanceType = instance;
    }

    public DependencyNotRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        DependencyType = (Type)(info?.GetValue("DependencyType", typeof(Type)) ?? typeof(nuint));
        InstanceType = (Type)(info?.GetValue("InstanceType", typeof(Type)) ?? typeof(nuint));
    }
    
    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(DependencyType), DependencyType);
        info.AddValue(nameof(InstanceType), InstanceType);
    }
}

[Serializable]
public sealed class DITypeNotSupportedException : SimpleDependencyInjectionException
{
    public Type InstanceType { get; }

    public DITypeNotSupportedException(Type instanceType) : 
        this(instanceType, $"{nameof(DependencyInjector)} only supports class types, {instanceType.FullName} is not a class", null) { }

    public DITypeNotSupportedException(Type instanceType, string? message, Exception? exception) : base(message, exception) {
        InstanceType = instanceType;
    }
    
    public DITypeNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) {
        InstanceType = (Type)(info?.GetValue("InstanceType", typeof(Type)) ?? typeof(nuint));
    }
    
    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(InstanceType), InstanceType);
    }
}

[Serializable]
public sealed class DuplicitScopeIdException : SimpleDependencyInjectionException
{
    public string Id { get; }
    public DuplicitScopeIdException(string id)
        : this(id, $"A scope with ID {id} already exists", null) { }

    public DuplicitScopeIdException(string id, string? message, Exception? innerException) : base(message, innerException)
    {
        Id = id;
    }

    public DuplicitScopeIdException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Id = info?.GetString("Id") ?? string.Empty;
    }
    
    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Id), Id);
    }
}