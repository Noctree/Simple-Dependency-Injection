namespace NokLib.DependencyInjection;
public static class DIExtensions
{
    public static bool ResolveDependencies<T>(this T instance) where T : class => DependencyInjector.ResolveDependencies(instance, typeof(T));

    public static void RegisterAsDependency<T>(this T instance) where T : class => DependencyInjector.RegisterDependency(instance);
}
