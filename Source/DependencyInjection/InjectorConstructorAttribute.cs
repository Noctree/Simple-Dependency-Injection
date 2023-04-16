using System;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleDI;

[AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
public class InjectorConstructorAttribute : ActivatorUtilitiesConstructorAttribute {}