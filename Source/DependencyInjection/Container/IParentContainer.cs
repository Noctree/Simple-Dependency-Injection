using System.Collections.Generic;

namespace SimpleDI.Containers;

public interface IParentContainer
{
    public IContainer CreateSubContainer(string id);
    public IContainer GetSubContainer(string id);
    public bool ContainsSubContainer(string id);
    public bool DeleteSubContainer(string id);
    public ICollection<IContainer> GetSubContainers();
}