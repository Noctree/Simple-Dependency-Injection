using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleDI.Containers;

public static class ServiceDescriptorExtensions
{
    /// <summary>
    /// Returns the service implementation type
    /// Method extracted from <see cref="ServiceDescriptor"/>, as it was marked as internal
    /// </summary>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public static Type GetImplementationType(this ServiceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (descriptor.ImplementationType != null)
        {
            return descriptor.ImplementationType;
        }
        if (descriptor.ImplementationInstance != null)
        {
            return descriptor.ImplementationInstance.GetType();
        }
        if (descriptor.ImplementationFactory != null)
        {
            Type[]? typeArguments = descriptor.ImplementationFactory.GetType().GenericTypeArguments;

            Debug.Assert(typeArguments.Length == 2);

            return typeArguments[1];
        }
        
        throw new SimpleDependencyInjectionException("ImplementationType, ImplementationInstance or ImplementationFactory must be non null");
    }
}