#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class WarningBadge : Badge
    {
        public WarningBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/Warnings");
            tooltip = "";
        }
    }
}
#endif
