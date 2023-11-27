#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [HelpURL("https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.kbbk63lwm6i")]
    public class BiomeMaskGraph : TerrainGraph
    {
        public override bool AcceptNodeType(Type t)
        {
            NodeMetadataAttribute meta = NodeMetadata.Get(t);
            if (meta != null)
            {
                string category = meta.GetCategory();
                if (category.Equals("Base Shape") || category.Equals("Adjustments"))
                {
                    return true;
                }
            }

            if (t.Equals(typeof(InputNode)) ||
                t.Equals(typeof(OutputNode)) ||
                t.Equals(typeof(SetVariableNode)) ||
                t.Equals(typeof(GetVariableNode)) ||
                t.Equals(typeof(CombineNode)) ||
                t.Equals(typeof(MathNode)) ||
                t.Equals(typeof(ValueCheckNode)) ||
                t.Equals(typeof(LoadTextureNode)) ||
                t.Equals(typeof(AnchorNode)))
            {
                return true;
            }

            return false;
        }

    }
}
#endif
