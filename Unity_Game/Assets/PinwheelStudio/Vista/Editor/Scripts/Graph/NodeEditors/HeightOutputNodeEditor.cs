#if VISTA
using Pinwheel.Vista.Graph;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(HeightOutputNode))]
    public class HeightOutputNodeEditor : ExecutableNodeEditorBase
    {
        public override void OnGUI(INode node)
        {
        }
    }
}
#endif
