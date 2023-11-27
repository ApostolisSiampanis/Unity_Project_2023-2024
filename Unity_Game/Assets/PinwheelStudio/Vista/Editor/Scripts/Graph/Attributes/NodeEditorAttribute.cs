#if VISTA
using System;

namespace Pinwheel.VistaEditor.Graph
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeEditorAttribute : Attribute
    {
        public Type nodeType { get; set; }

        public NodeEditorAttribute(Type nodeType)
        {
            this.nodeType = nodeType;
        }
    }
}
#endif
