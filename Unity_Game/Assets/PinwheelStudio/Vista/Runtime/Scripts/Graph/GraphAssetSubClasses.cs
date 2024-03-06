#if VISTA
using System.Collections.Generic;

namespace Pinwheel.Vista.Graph
{
    public partial class GraphAsset
    {
        public struct RemovedElements
        {
            public INode node;
            public List<IEdge> edges;
        }
    }
}
#endif
