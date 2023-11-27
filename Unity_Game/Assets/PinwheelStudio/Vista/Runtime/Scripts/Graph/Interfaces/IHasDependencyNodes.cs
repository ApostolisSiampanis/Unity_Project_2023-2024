#if VISTA
using System.Collections.Generic;

namespace Pinwheel.Vista.Graph
{
    public interface IHasDependencyNodes : INode
    {
        IEnumerable<INode> GetDependencies(IEnumerable<INode> nodes);
    }
}
#endif
