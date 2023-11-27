#if VISTA
using Pinwheel.Vista.Graph;

namespace Pinwheel.VistaEditor.Graph
{
    public interface INeedUpdateNodeVisual
    {
        void UpdateVisual(INode node, NodeView nv);
    }
}
#endif
