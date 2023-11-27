#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface IObjectPopulator
    {
        ProgressiveTask PopulateObject(List<ObjectTemplate> templates, List<ComputeBuffer> sampleBuffers, VistaManager.ObjectPopulateArgs objectPopulateArgs);
    }
}
#endif
