using System;

namespace SimpleDI;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
public sealed class InjectAttribute : Attribute
{
}
