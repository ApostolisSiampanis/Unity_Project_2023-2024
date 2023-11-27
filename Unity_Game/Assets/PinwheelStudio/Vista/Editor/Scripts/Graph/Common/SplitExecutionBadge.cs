#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class SplitExecutionBadge : Badge
    {
        public SplitExecutionBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/SplitExecution");
            tooltip = "This node splits its execution to several frames";
        }
    }
}
#endif
