using Microsoft.Extensions.DependencyInjection;

namespace SimpleDI;
public static class DIExtensions
{
    public static void ResolveDependencies<T>(this T instance) where T : class => DependencyInjector.ResolveDependencies(instance);

    public static void RegisterAsSingleton<T>(this T instance) where T : class => DependencyInjector.RegisterDependency(ServiceDescriptor.Singleton(instance));

    public static ServiceDescriptor WithInstance(this ServiceDescriptor serviceDescriptor, object instance)
    {
        return new(serviceDescriptor.ServiceType, instance);
    }
}
