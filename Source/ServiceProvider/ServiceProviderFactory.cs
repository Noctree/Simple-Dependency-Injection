using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;

namespace SimpleDI.ServiceProvider;

public class ServiceProviderFactory : IAdvancedServiceProviderFactory
{
    /// <inheritdoc />
    public ServiceCollectionBase CreateBuilder(IServiceCollection services) =>
        new AdvancedServiceCollection(services);

    /// <inheritdoc />
    public IServiceProvider CreateServiceProvider(ServiceCollectionBase containerBuilder) =>
        CreateAdvancedServiceProvider(containerBuilder);

    public IAdvancedServiceProvider CreateAdvancedServiceProvider(ServiceCollectionBase containerBuilder) =>
        new AdvancedServiceProvider(containerBuilder);
}