using System;

namespace SimpleDI;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class InjectAttribute : Attribute
{
}
