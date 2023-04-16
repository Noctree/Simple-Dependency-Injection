namespace SimpleDI;
public interface IDelayedDependencyInjection
{
    public void OnDependenciesInjected();
    public void MarkForDelayedInjection();
}
