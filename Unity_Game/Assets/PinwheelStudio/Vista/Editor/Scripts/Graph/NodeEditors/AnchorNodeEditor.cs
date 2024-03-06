#if VISTA
using Pinwheel.Vista.Graph;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(AnchorNode))]
    public class AnchorNodeEditor : ExecutableNodeEditorBase
    {
        public override bool hasSpecificProperties => false;
    }
}
#endif
