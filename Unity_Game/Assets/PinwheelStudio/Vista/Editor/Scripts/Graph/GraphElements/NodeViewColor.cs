#if VISTA
using UnityEngine;

namespace Pinwheel.VistaEditor
{
    internal struct NodeViewColor
    {
        internal Color mainColor { get; set; }
        internal Color secondaryColor { get; set; }

        public NodeViewColor(Color main, Color secondary)
        {
            mainColor = main;
            secondaryColor = secondary;
        }
    }
}
#endif
