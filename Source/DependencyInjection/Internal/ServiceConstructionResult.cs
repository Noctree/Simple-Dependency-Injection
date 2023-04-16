using System;
using System.Diagnostics.CodeAnalysis;

namespace SimpleDI.Internal;

public readonly struct ServiceConstructionResult
{
    public ServiceConstructionResult(bool Success, object? Instance = null, Type? MissingDependency = null)
    {
        this.Success = Success;
        this.Instance = Instance;
        this.MissingDependency = MissingDependency;
    }

    public bool Success { get; init; }

    [MemberNotNullWhen(true, nameof(Success))]
    public object? Instance { get; init; }

    [MemberNotNullWhen(false, nameof(Success))]
    public Type? MissingDependency { get; init; }

    public void Deconstruct(out bool Success, out object? Instance, out Type? MissingDependency)
    {
        Success = this.Success;
        Instance = this.Instance;
        MissingDependency = this.MissingDependency;
    }
}