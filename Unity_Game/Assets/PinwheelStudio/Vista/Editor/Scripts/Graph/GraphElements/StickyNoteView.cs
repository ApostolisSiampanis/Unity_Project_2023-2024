#if VISTA
using UnityEditor.Experimental.GraphView;

namespace Pinwheel.VistaEditor.Graph
{
    public class StickyNoteView : StickyNote
    {
        public string noteId { get; set; }

        public StickyNoteView() : base()
        {
            capabilities |= Capabilities.Groupable | Capabilities.Snappable;
        }
    }
}
#endif
