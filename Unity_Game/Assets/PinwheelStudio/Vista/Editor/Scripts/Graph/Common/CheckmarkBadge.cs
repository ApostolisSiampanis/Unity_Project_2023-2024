#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor.UIElements
{
    public class CheckmarkBadge : Badge
    {
        public CheckmarkBadge() : base()
        {
            icon = Resources.Load<Texture>("Vista/Textures/Checkmark");
            tooltip = null;
        }
    }
}
#endif
