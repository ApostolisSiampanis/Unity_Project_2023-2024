#if VISTA
using UnityEditor.Searcher;

namespace Pinwheel.VistaEditor.Graph
{
    public class NodeSearcherItem : SearcherItem
    {
        public NodeSearcherItem(string name, string help="") : base(name, help)
        {
        }

        public SearcherItemData data { get; set; }

    }
}
#endif
