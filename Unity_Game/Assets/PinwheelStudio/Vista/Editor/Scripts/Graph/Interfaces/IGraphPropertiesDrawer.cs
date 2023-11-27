#if VISTA
using Pinwheel.Vista.Graph;

namespace Pinwheel.VistaEditor.Graph
{
    public interface IGraphPropertiesDrawer
    {
        GraphEditorBase editor { get; set; }
        void OnDrawProperties(GraphAsset graph);
    }
}
#endif
