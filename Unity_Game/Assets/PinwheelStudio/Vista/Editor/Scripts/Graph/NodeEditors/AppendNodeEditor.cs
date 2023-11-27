#if VISTA
using Pinwheel.Vista.Graph;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(AppendNode))]
    public class AppendNodeEditor : ExecutableNodeEditorBase
    {
        public override void OnGUI(INode node)
        {

        }
    }
}
#endif
