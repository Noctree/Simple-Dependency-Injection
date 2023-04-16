using System;
using System.Diagnostics.CodeAnalysis;

namespace SimpleDI.Internal;

public readonly struct DependencyResolutionResult
{
    public DependencyResolutionResult(bool Success, Type? MissingDependency = null)
    {
        this.Success = Success;
        this.MissingDependency = MissingDependency;
    }

    public bool Success { get; init; }

    [MemberNotNullWhen(false, nameof(Success))]
    public Type? MissingDependency { get; init; }

    public void Deconstruct(out bool Success, out Type? MissingDependency)
    {
        Success = this.Success;
        MissingDependency = this.MissingDependency;
    }
}