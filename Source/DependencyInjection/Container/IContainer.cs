using System;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleDI.Containers;

public interface IContainer : IAdvancedServiceProvider
{
    public string Id { get; }
    public bool IsDisposed { get; }
    public IAdvancedServiceCollection ServiceCollection { get; }
    public void RegisterService(ServiceDescriptor descriptor);
    public void UnregisterService(Type serviceType);
    public bool IsRegistered(Type serviceType);
    public void ResolveDependencies(object instance);
    public IContainer CreateSubContainer();
    public IContainer CreateSubContainer(string id);
    public bool TryGetSubContainer(string id, out IContainer container);
    public bool DeleteSubContainer(string id);
}