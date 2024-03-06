#if VISTA
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    public class GroupView : Group
    {
        public string groupId { get; set; }

        public GroupView() : base()
        {
            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/GroupView");
            styleSheets.Add(uss);

            capabilities |= Capabilities.Snappable;
        }
    }
}
#endif
