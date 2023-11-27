#if VISTA
using System;
using System.Collections.Generic;

namespace Pinwheel.Vista.Graph
{
    public static class GraphAssetExtensions
    {
        public static void GetRegisteredVars(this GraphAsset graph, List<string> varNames, List<Type> varTypes)
        {
            varNames.Clear();
            varTypes.Clear();

            List<SetVariableNode> setVarNodes = graph.GetNodesOfType<SetVariableNode>();
            foreach (SetVariableNode n in setVarNodes)
            {
                if (string.IsNullOrEmpty(n.varName))
                    continue;
                if (!typeof(ISlot).IsAssignableFrom(n.slotType))
                    continue;
                varNames.Add(n.varName);
                varTypes.Add(n.slotType);
            }
        }
    }
}
#endif
