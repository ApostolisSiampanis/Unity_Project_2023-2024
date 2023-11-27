#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface IDetailInstancePopulator : ITile
    {
        void PopulateDetailInstance(List<DetailTemplate> templates, List<ComputeBuffer> samples);
    }
}
#endif
