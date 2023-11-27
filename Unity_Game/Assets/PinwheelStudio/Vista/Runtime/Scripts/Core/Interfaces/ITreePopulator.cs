#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public interface ITreePopulator : ITile
    {
        void PopulateTrees(List<TreeTemplate> templates, List<ComputeBuffer> samples);
    }
}
#endif
