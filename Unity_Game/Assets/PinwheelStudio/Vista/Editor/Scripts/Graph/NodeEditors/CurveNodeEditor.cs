#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(CurveNode))]
    public class CurveNodeEditor : ImageNodeEditorBase
    {
        private static GUIContent CURVE = new GUIContent("Curve", "The remap curve");

        public override void OnGUI(INode node)
        {
            CurveNode n = node as CurveNode;
            AnimationCurve curve = new AnimationCurve(n.curve.keys);
            EditorGUI.BeginChangeCheck();
            curve = EditorGUILayout.CurveField(CURVE, curve, Color.red, new Rect(0, 0, 1, 1));
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.curve = curve;
            }
        }
    }
}
#endif
