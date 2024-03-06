#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(MathNode))]
    public class MathNodeEditor : ImageNodeEditorBase
    {
        public override void OnGUI(INode node)
        {
            MathNode n = node as MathNode;
            foreach (MathNode.MathConfig c in n.maths)
            {
                DrawConfig(n, c);
            }

            GUI.enabled = n.maths.Count < MathNode.MAX_CONFIG;
            if (GUILayout.Button("Add Operator"))
            {
                n.maths.Add(new MathNode.MathConfig());
            }
            GUI.enabled = true;
        }

        private static readonly GUIContent ENABLED = new GUIContent("Enabled", "Is the math config enable?");
        private static readonly GUIContent ADD_NUMBER = new GUIContent("Number", "The number to add");
        private static readonly GUIContent OPS = new GUIContent("Operator", "The math operator to apply on each texture pixel");

        private void DrawConfig(MathNode node, MathNode.MathConfig c)
        {
            MathNode.Operator ops = c.ops;
            bool enabled = c.enabled;
            float number = c.number;
            EditorGUILayout.BeginFoldoutHeaderGroup(true, ops.ToString(), null, (r) => { ShowConfigContextMenu(node, r, c); });
            EditorGUI.BeginChangeCheck();
            enabled = EditorGUILayout.Toggle(ENABLED, c.enabled);
            if (c.ops != MathNode.Operator.Abs &&
                c.ops != MathNode.Operator.OneMinus &&
                c.ops != MathNode.Operator.Sin &&
                c.ops != MathNode.Operator.Cos)
            {
                number = EditorGUILayout.FloatField(ADD_NUMBER, c.number);
            }
            ops = (MathNode.Operator)EditorGUILayout.EnumPopup(OPS, c.ops);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(node);
                c.ops = ops;
                c.enabled = enabled;
                c.number = number;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void ShowConfigContextMenu(MathNode node, Rect r, MathNode.MathConfig c)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Remove"),
                false,
                () => { node.maths.Remove(c); });
            menu.DropDown(r);
            GUI.changed = true;
        }
    }
}
#endif
