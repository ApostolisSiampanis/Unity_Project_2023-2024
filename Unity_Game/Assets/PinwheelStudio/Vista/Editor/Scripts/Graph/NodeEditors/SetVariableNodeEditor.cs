#if VISTA
using Pinwheel.Vista.Graph;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(SetVariableNode))]
    public class SetVariableNodeEditor : ExecutableNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent VAR_NAME = new GUIContent("Variable Name", "Name of the variable"); 
        private static readonly GUIContent SLOT_TYPE = new GUIContent("Slot Type", "Data type of the slot");

        public override void OnGUI(INode node)
        {
            SetVariableNode n = node as SetVariableNode;
            EditorGUI.BeginChangeCheck();
            string varName = EditorGUILayout.DelayedTextField(VAR_NAME, n.varName);

            List<Type> slotTypes = SlotProvider.GetAllSlotTypes();
            int selectedTypeIndex = slotTypes.IndexOf(n.slotType);
            string[] slotTypeLabels = new string[slotTypes.Count];
            for (int i = 0; i < slotTypes.Count; ++i)
            {
                slotTypeLabels[i] = ObjectNames.NicifyVariableName(slotTypes[i].Name);
            }
            selectedTypeIndex = EditorGUILayout.Popup(SLOT_TYPE, selectedTypeIndex, slotTypeLabels);

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.varName = varName;
                if (selectedTypeIndex >= 0 && selectedTypeIndex < slotTypes.Count)
                {
                    n.SetSlotType(slotTypes[selectedTypeIndex]);
                }
                else
                {
                    n.SetSlotType(slotTypes[0]);
                }
            }
        }

        public void UpdateVisual(INode node, NodeView nv)
        {
            SetVariableNode n = node as SetVariableNode;
            PortView pv = nv.Q<PortView>();
            if (pv != null)
            {
                pv.portName = !string.IsNullOrEmpty(n.varName) ? n.varName : "(not set)";
            }
        }
    }
}
#endif
