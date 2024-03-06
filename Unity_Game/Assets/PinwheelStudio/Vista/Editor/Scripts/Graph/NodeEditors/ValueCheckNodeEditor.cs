#if VISTA
using Pinwheel.Vista.Graph;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(ValueCheckNode))]
    public class ValueCheckNodeEditor : ImageNodeEditorBase
    {
        public override void OnGUI(INode node)
        {

        }
    }
}
#endif
