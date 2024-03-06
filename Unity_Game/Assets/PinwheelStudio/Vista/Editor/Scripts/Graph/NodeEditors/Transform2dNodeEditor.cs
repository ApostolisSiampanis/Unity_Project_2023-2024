#if VISTA
using Pinwheel.Vista.Graph;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(Transform2dNode))]
    public class Transform2dNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent SLOT_TYPE = new GUIContent("Slot Type", "Data format of the input");
        private static readonly GUIContent TILING_MODE = new GUIContent("Tiling Mode", "How to tile the image");
        private static readonly GUIContent OFFSET = new GUIContent("Offset", "Offset of the image in %");
        private static readonly GUIContent ROTATION = new GUIContent("Rotation", "Rotation of the image in degree");
        private static readonly GUIContent SCALE = new GUIContent("Scale", "Scale of the image in %");
        private static readonly GUIContent BACKGROUND_COLOR = new GUIContent("Background Color", "The background color when there is no tiling");

        public override void OnGUI(INode node)
        {
            Transform2dNode n = node as Transform2dNode;
            EditorGUI.BeginChangeCheck();

            List<Type> slotTypes = SlotProvider.GetTextureSlotTypes();
            int selectedTypeIndex = slotTypes.IndexOf(n.slotType);
            string[] slotTypeLabels = new string[slotTypes.Count];
            for (int i = 0; i < slotTypes.Count; ++i)
            {
                slotTypeLabels[i] = ObjectNames.NicifyVariableName(slotTypes[i].Name);
            }
            selectedTypeIndex = EditorGUILayout.Popup(SLOT_TYPE, selectedTypeIndex, slotTypeLabels);

            Transform2dNode.TilingMode tilingMode = (Transform2dNode.TilingMode)EditorGUILayout.EnumPopup(TILING_MODE, n.tilingMode);
            Vector2 offset = EditorCommon.InlineVector2Field(OFFSET, n.offset);
            float rotation = EditorGUILayout.FloatField(ROTATION, n.rotation);
            Vector2 scale = EditorCommon.InlineVector2Field(SCALE, n.scale);
            Color bgColor = EditorGUILayout.ColorField(BACKGROUND_COLOR, n.backgroundColor);

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                if (selectedTypeIndex >= 0 && selectedTypeIndex < slotTypes.Count)
                {
                    n.SetSlotType(slotTypes[selectedTypeIndex]);
                }
                else
                {
                    n.SetSlotType(slotTypes[0]);
                }

                n.tilingMode = tilingMode;
                n.offset = offset;
                n.rotation = rotation;
                n.scale = scale;
                n.backgroundColor = bgColor;
            }
        }
    }
}
#endif
