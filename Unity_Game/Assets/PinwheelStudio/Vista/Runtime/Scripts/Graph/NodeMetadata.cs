#if VISTA
using System;
using System.Reflection;

namespace Pinwheel.Vista.Graph
{
    public class NodeMetadata
    {
        public static NodeMetadataAttribute Get(Type nodeType)
        {
            NodeMetadataAttribute att = nodeType.GetCustomAttribute<NodeMetadataAttribute>(false);
            return att;
        }

        public static NodeMetadataAttribute Get<T>()
        {
            return Get(typeof(T));
        }
    }
}
#endif
