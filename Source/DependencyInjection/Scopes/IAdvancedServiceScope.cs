using Microsoft.Extensions.DependencyInjection;
using SimpleDI.Containers;

namespace SimpleDI.Scopes;

public interface IAdvancedServiceScope : IServiceScope
{
    public IAdvancedServiceProvider AdvancedServiceProvider { get; }
}