#if VISTA
using Pinwheel.Vista.Graph;
using System.Collections.Generic;

namespace Pinwheel.VistaEditor.Graph
{
    public class BiomeMaskGraphEditor : TerrainGraphEditor
    {
        protected override void SetupAdapter()
        {
            BiomeMaskGraphAdapter adapter = new BiomeMaskGraphAdapter();
            adapter.Init(this);
            m_adapter = adapter;
        }
    }
}
#endif
