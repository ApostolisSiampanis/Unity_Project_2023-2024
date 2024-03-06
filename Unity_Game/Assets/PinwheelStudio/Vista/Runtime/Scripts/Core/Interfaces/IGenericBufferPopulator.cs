#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface IGenericBufferPopulator
    {
        void PopulateGenericBuffers(List<string> labels, List<ComputeBuffer> buffers);
    }
}
#endif
