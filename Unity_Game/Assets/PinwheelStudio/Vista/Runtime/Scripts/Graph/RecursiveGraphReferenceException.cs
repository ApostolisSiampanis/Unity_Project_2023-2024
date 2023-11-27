#if VISTA
using System;

namespace Pinwheel.Vista.Graph
{
    public class RecursiveGraphReferenceException : Exception
    {
        public RecursiveGraphReferenceException() : base()
        {

        }

        public RecursiveGraphReferenceException(string message) : base(message)
        {

        }
    }
}
#endif
