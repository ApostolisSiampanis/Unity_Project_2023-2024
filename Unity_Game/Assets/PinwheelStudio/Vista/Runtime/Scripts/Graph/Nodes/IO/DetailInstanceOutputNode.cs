#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Detail Instance Output",
        path = "IO/Detail Instance Output",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.esrm5jho6el",
        keywords = "",
        description = "Output grass/detail instances to the terrain. Unity terrains don't use detail instances, use Detail Density Output Node instead.")]
    public class DetailInstanceOutputNode : InstanceOutputNodeBase
    {
        [SerializeAsset]
        private DetailTemplate m_detailTemplate;
        public DetailTemplate detailTemplate
        {
            get
            {
                return m_detailTemplate;
            }
            set
            {
                m_detailTemplate = value;
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

        public DetailInstanceOutputNode() : base()
        {
            m_detailTemplate = null;
        }        
    }
}
#endif
