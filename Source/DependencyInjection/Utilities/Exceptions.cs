using System;
using System.Runtime.Serialization;

namespace SimpleDI;

/// <summary>
/// Base exception for dependency injection.
/// </summary>
[Serializable]
public class SimpleDependencyInjectionException : Exception
{
    public SimpleDependencyInjectionException() { }

    public SimpleDependencyInjectionException(string? message) : base(message) { }

    public SimpleDependencyInjectionException(string? message, Exception? innerException) : base(message, innerException) { }


    /// <inheritdoc />
    protected SimpleDependencyInjectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

/// <summary>
/// Thrown when a type is already registered
/// </summary>
[Serializable]
public sealed class TypeAlreadyRegisteredException : SimpleDependencyInjectionException
{
    public static string UnknownParameterName { get; } = "_Unknown_";

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
        base.GetObjectData(info, context);
    }
}


/// <summary>
/// This exception is thrown when a dependency is not registered in the <see cref="DependencyInjector"/>
/// </summary>
[Serializable]
public sealed class ServiceNotFoundException : SimpleDependencyInjectionException
{
    public Type DependencyType { get; }
    public Type? InstanceType { get; }

    public ServiceNotFoundException(Type dependency, Type? instance = null)
        : this(dependency, instance, GetMessage(dependency, instance)) {}

    public ServiceNotFoundException(Type dependency, Type? instance, string? message, Exception? innerException = null) : base(message, innerException)
    {
        DependencyType = dependency;
        InstanceType = instance;
    }

    public ServiceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        DependencyType = (Type)(info?.GetValue("DependencyType", typeof(Type)) ?? typeof(nuint));
        InstanceType = (info?.GetValue("InstanceType", typeof(Type))) as Type;
    }
    
    private static string GetMessage(Type dependency, Type? instance) =>
        instance is null
            ? $"Service of type {dependency.FullName} was not found"
            : $"Object of type {instance.FullName} requested service {dependency.FullName} as its dependency, but {dependency.FullName} was not found";

    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(DependencyType), DependencyType);
        info.AddValue(nameof(InstanceType), InstanceType);
        base.GetObjectData(info, context);
    }
}

/// <summary>
/// Thrown when the supplied type is is not supported for dependency injection
/// </summary>
[Serializable]
public sealed class TypeNotSupportedException : SimpleDependencyInjectionException
{
    public Type InstanceType { get; }

    public TypeNotSupportedException(Type instanceType)
        : this(instanceType, $"{nameof(DependencyInjector)} only supports non-generic class types", null) {
    }

    public TypeNotSupportedException(Type instanceType,
        string? message,
        Exception? exception = null) : base(message, exception) {
        InstanceType = instanceType;
    }
    
    public TypeNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) {
        InstanceType = (Type)(info?.GetValue("InstanceType", typeof(Type)) ?? typeof(nuint));
    }
    
    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(InstanceType), InstanceType);
        base.GetObjectData(info, context);
    }
}

[Serializable]
public sealed class MemberNotSupportedException : SimpleDependencyInjectionException
{
    /// <summary>
    /// Name of the member that is not supported
    /// </summary>
    public string MemberName { get; }
    
    public Type InstanceType { get; }
    
    public MemberNotSupportedException(Type instanceType, string memberName)
        : this(instanceType, memberName, null, null)
    { }
    
    public MemberNotSupportedException(Type instanceType, string memberName, string? message, Exception? innerException = null)
        : base(message, innerException)
    {
        InstanceType = instanceType;
        MemberName = memberName;
    }
    
    public MemberNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        InstanceType = (Type)(info?.GetValue("InstanceType", typeof(Type)) ?? typeof(nuint));
        MemberName = info?.GetString("MemberName") ?? string.Empty;
    }
    
    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(InstanceType), InstanceType);
        info.AddValue(nameof(MemberName), MemberName);
        base.GetObjectData(info, context);
    }
}

/// <summary>
/// Thrown when an object with the same ID already exists
/// </summary>
[Serializable]
public sealed class DuplicitIdException : SimpleDependencyInjectionException
{
    public string Id { get; }
    public DuplicitIdException(string id)
        : this(id, $"A scope with ID {id} already exists", null) { }

    public DuplicitIdException(string id, string? message, Exception? innerException) : base(message, innerException)
    {
        Id = id;
    }

    public DuplicitIdException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Id = info?.GetString("Id") ?? string.Empty;
    }
    
    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Id), Id);
        base.GetObjectData(info, context);
    }
}

/// <summary>
/// Thrown when there is an issue with dependency injection trough constructor
/// </summary>
[Serializable]
public sealed class ConstructorException : SimpleDependencyInjectionException
{
    /// <summary>
    /// Type of the instance to be created.
    /// </summary>
    public Type InstanceType { get; }

    public ConstructorException(Type instanceType, string message, Exception? innerException = null) : base(
        message, innerException)
    {
        InstanceType = instanceType;
    }
    
    public ConstructorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        InstanceType = (Type)(info?.GetValue("InstanceType", typeof(Type)) ?? typeof(nuint));
    }
    
    /// <inheritdoc />
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(InstanceType), InstanceType);
        base.GetObjectData(info, context);
    }
}