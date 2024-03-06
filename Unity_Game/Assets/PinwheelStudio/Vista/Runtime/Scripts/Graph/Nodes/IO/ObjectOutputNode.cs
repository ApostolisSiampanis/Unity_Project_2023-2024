#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Object Output",
        path = "IO/Object Output",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.qbafbe7y594g",
        keywords = "",
        description = "Output instances (game object) of a specific prefab (and its variants) to the terrain.")]
    public class ObjectOutputNode : InstanceOutputNodeBase
    {
        [SerializeAsset]
        private ObjectTemplate m_objectTemplate;
        public ObjectTemplate objectTemplate
        {
            get
            {
                return m_objectTemplate;
            }
            set
            {
                m_objectTemplate = value;
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

        public ObjectOutputNode() : base()
        {
            m_objectTemplate = null;
        }
    }
}
#endif
