#if VISTA
using Pinwheel.Vista.Graph;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(DotsNode))]
    public class DotsNodeEditor : ImageNodeEditorBase
    {
        public override void OnGUI(INode node)
        {
        }
    }
}
#endif
