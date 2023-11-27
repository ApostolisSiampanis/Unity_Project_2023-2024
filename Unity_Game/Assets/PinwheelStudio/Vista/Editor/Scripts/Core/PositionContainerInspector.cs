#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.Vista;

namespace Pinwheel.VistaEditor
{
    [CustomEditor(typeof(PositionContainer))]
    public class PositionContainerInspector : Editor
    {
        private static readonly GUIContent HELP = new GUIContent(
               "Use 2D View to edit points:\n" +
               "- Shift Click to add.\n" +
               "- Ctrl Click to remove.\n" +
               "- Drag Left Mouse to move.");
        private PositionContainer m_instance;

        private void OnEnable()
        {
            m_instance = target as PositionContainer;
        }

        public override void OnInspectorGUI()
        {
            Rect outerRect = GUILayoutUtility.GetAspectRect(1);
            Rect innerRect = new Rect() { size = outerRect.size * 0.65f, center = outerRect.center };
            Vector2[] vectors = VectorsFromPositionSamples(m_instance.positions);
            EditorGUI.BeginChangeCheck();
            Vector2[] positions = EditorCommon.PositionSelector(outerRect, innerRect, vectors);
            EditorGUILayout.HelpBox(HELP);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_instance, "Modify Position Container");
                EditorUtility.SetDirty(m_instance);
                m_instance.positions = PositionSamplesFromVectors(positions);
            }

            base.OnInspectorGUI();
        }

        private Vector2[] VectorsFromPositionSamples(PositionSample[] samples)
        {
            Vector2[] vectors = new Vector2[samples.Length];
            for (int i = 0; i < vectors.Length; ++i)
            {
                Vector3 pos = samples[i].position;
                vectors[i] = new Vector2(pos.x, pos.z);
            }
            return vectors;
        }

        private PositionSample[] PositionSamplesFromVectors(Vector2[] vectors)
        {
            PositionSample[] samples = new PositionSample[vectors.Length];
            for (int i = 0; i < samples.Length; ++i)
            {
                Vector2 v = vectors[i];
                PositionSample s = new PositionSample();
                s.position = new Vector3(v.x, 0, v.y);
                s.isValid = 1;
                samples[i] = s;
            }
            return samples;
        }
    }
}
#endif
