#if VISTA
using Pinwheel.Vista.Graph;
using System;

namespace Pinwheel.VistaEditor.Graph
{
    public class SearcherItemData
    {
        public Type nodeType { get; set; }
        public NodeMetadataAttribute meta { get; set; }
        public int slotId { get; set; }
        public string slotName { get; set; }
        public int slotIndex { get; set; }
        public string hint { get; set; }
    }
}
#endif
