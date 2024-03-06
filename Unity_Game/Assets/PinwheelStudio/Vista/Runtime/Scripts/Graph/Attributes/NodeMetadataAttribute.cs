#if VISTA
using System;

namespace Pinwheel.Vista.Graph
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeMetadataAttribute : Attribute
    {
        public delegate string ParseNodeDescriptionHandler(string description);
        public static ParseNodeDescriptionHandler parseDescriptionCallback;

        public string title { get; set; }
        public string path { get; set; }
        public string icon { get; set; }
        public string documentation { get; set; }
        public string keywords { get; set; }
        public string description { get; set; }
        public bool hideFromDoc { get; set; }

        public string GetCategory()
        {
            string category = string.Empty;
            if (!string.IsNullOrEmpty(path))
            {
                int separatorIndex = path.IndexOf('/');
                if (separatorIndex > 0)
                {
                    category = path.Substring(0, separatorIndex);
                }
            }
            return category;
        }
    }
}
#endif
