using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;
using SimpleDI.Scopes;

namespace SimpleDI.ServiceProvider;

public interface IAdvancedServiceProviderFactory : IServiceProviderFactory<ServiceCollectionBase>
{
    public IAdvancedServiceProvider CreateAdvancedServiceProvider(ServiceCollectionBase containerBuilder); 
}