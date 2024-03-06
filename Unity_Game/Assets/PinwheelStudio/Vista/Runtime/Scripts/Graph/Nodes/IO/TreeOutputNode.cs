#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Tree Output",
        path = "IO/Tree Output",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.6xa0ivkuvpke",
        keywords = "",
        description = "Output tree instances of a prefab (and its variants) to the terrain.")]
    public class TreeOutputNode : InstanceOutputNodeBase
    {
        [SerializeAsset]
        private TreeTemplate m_treeTemplate;
        public TreeTemplate treeTemplate
        {
            get
            {
                return m_treeTemplate;
            }
            set
            {
                m_treeTemplate = value;
            }
        }

        public override bool isBypassed
        {
            get
            {
                return false;
            }
            set
            {
                m_isBypassed = false;
            }
        }

        public TreeOutputNode() : base()
        {
            m_treeTemplate = null;
        }
    }
}
#endif
