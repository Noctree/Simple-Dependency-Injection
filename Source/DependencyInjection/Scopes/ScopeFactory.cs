using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;
using SimpleDI.ServiceProvider;

namespace SimpleDI.Scopes;

public class ScopeFactory : IServiceScopeFactory
{
    private readonly IAdvancedServiceProviderFactory _factory;
    private readonly ServiceCollectionBase _source;

    public ScopeFactory(IAdvancedServiceProviderFactory factory, ServiceCollectionBase source)
    {
        _factory = factory;
        _source = source;
    }

    /// <inheritdoc />
    public IServiceScope CreateScope() =>
        new Scope(_factory.CreateAdvancedServiceProvider(_source));
}