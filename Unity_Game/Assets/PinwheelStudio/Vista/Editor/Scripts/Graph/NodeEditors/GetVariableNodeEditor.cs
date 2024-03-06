#if VISTA
using Pinwheel.Vista.Graph;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(GetVariableNode))]
    public class GetVariableNodeEditor : ExecutableNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent VAR_NAME = new GUIContent("Variable Name", "Name of the variable");
        private static readonly GUIContent SLOT_TYPE = new GUIContent("Slot Type", "Data type of the slot");

        public override void OnGUI(INode node)
        {
            GetVariableNode n = node as GetVariableNode;
            List<string> varNames = new List<string>();
            List<Type> varTypes = new List<Type>();
            m_graphEditor.clonedGraph.GetRegisteredVars(varNames, varTypes);
            int selectedIndex = varNames.IndexOf(n.varName);

            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(VAR_NAME, selectedIndex, varNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                if (selectedIndex >= 0 && selectedIndex < varNames.Count)
                {
                    n.varName = varNames[selectedIndex];
                    n.SetSlotType(varTypes[selectedIndex]);
                }
            }
        }

        public void UpdateVisual(INode node, NodeView nv)
        {
            GetVariableNode n = node as GetVariableNode;
            PortView pv = nv.Q<PortView>();
            if (pv != null)
            {
                pv.portName = !string.IsNullOrEmpty(n.varName) ? n.varName : "(not set)";
            }
        }
    }
}
#endif
