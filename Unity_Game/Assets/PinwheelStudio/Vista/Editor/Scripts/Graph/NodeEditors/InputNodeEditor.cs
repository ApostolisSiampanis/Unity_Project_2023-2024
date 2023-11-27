#if VISTA
using Pinwheel.Vista.Graph;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(InputNode))]
    public class InputNodeEditor : ExecutableNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent NAME = new GUIContent("Name", "Name of the input");
        private static readonly GUIContent SLOT_TYPE = new GUIContent("Slot Type", "Data type of the slot");

        private static List<NameSelectorEntry> s_nameSelector;
        public static List<NameSelectorEntry> nameSelector
        {
            get
            {
                if (s_nameSelector == null)
                {
                    s_nameSelector = new List<NameSelectorEntry>();
                }
                return s_nameSelector;
            }
        }

        public void UpdateVisual(INode node, NodeView nv)
        {
            InputNode n = node as InputNode;
            PortView pv = nv.Q<PortView>();
            if (pv != null)
            {
                pv.portName = !string.IsNullOrEmpty(n.inputName) ? n.inputName : "(not set)";
            }
        }

        public override void OnGUI(INode node)
        {
            InputNode n = node as InputNode;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            string inputName = EditorGUILayout.DelayedTextField(NAME, n.inputName);
            if (nameSelector.Count > 0)
            {
                Rect dropDownRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, EditorStyles.popup, GUILayout.Width(20));
                if (GUI.Button(dropDownRect, "", EditorStyles.popup))
                {
                    GenericMenu menu = new GenericMenu();
                    foreach (NameSelectorEntry s in nameSelector)
                    {
                        menu.AddItem(
                            new GUIContent(s.name),
                            false,
                            () =>
                            {
                                m_graphEditor.RegisterUndo(n);
                                n.inputName = s.name;
                                n.SetSlotType(s.slotType);
                                m_graphEditor.UpdateNodesVisual();
                            });
                    }
                    menu.DropDown(dropDownRect);
                }
            }
            EditorGUILayout.EndHorizontal();

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
                n.inputName = inputName;
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
    }
}
#endif
