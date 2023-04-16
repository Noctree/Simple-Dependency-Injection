using System;

namespace SimpleDI;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InjectTroughConstructorAttribute : Attribute
{
}