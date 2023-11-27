#if VISTA
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor
{
    //[CustomEditor(typeof(EditorSettings))]
    public class EditorSettingsInspector : Editor
    {
        private EditorSettings m_instance;

        private void OnEnable()
        {
            m_instance = target as EditorSettings;
        }

        public override void OnInspectorGUI()
        {
            DrawGeneralSettingsGUI();
            DrawGraphEditorSettingsGUI();
        }

        private class GeneralSettingsGUI
        {
            public static readonly string ID = "com.pinwheel.vista.editorsettings.generalsettings";
            public static readonly GUIContent LABEL = new GUIContent("General");
            public static readonly GUIContent ENABLE_AFF_LINKS = new GUIContent("Enable Affiliate Links", "Allows it to show links to compatible, recommended assets.");
        }

        private void DrawGeneralSettingsGUI()
        {
            if (EditorCommon.BeginFoldout(GeneralSettingsGUI.ID, GeneralSettingsGUI.LABEL))
            {
                EditorSettings.GeneralSettings s = m_instance.generalSettings;
                EditorGUI.BeginChangeCheck();
                bool enableAffLink = EditorGUILayout.Toggle(GeneralSettingsGUI.ENABLE_AFF_LINKS, s.enableAffLinks);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, "Modify Editor Settings");
                    s.enableAffLinks = enableAffLink;
                    EditorUtility.SetDirty(m_instance);
                }
            }
            EditorCommon.EndFoldout();
        }

        private class GraphEditorSettingsGUI
        {
            public static readonly string ID = "com.pinwheel.vista.editorsettings.grapheditorsettings";
            public static readonly GUIContent LABEL = new GUIContent("Graph Editor");
            public static readonly GUIContent TERRAIN_VIS_QUALITY = new GUIContent("Terrain Quality", "The quality of terrain rendering in 3d viewport");
        }

        private void DrawGraphEditorSettingsGUI()
        {
            if (EditorCommon.BeginFoldout(GraphEditorSettingsGUI.ID, GraphEditorSettingsGUI.LABEL))
            {
                EditorSettings.GraphEditorSettings s = m_instance.graphEditorSettings;
                EditorGUI.BeginChangeCheck();
                int visQuality = EditorGUILayout.IntSlider(GraphEditorSettingsGUI.TERRAIN_VIS_QUALITY, s.terrainVisualizationQuality, EditorSettings.GraphEditorSettings.MIN_VIS_QUALITY, EditorSettings.GraphEditorSettings.MAX_VIS_QUALITY);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, "Modify Editor Settings");
                    s.terrainVisualizationQuality = visQuality;
                    EditorUtility.SetDirty(m_instance);
                }
            }
            EditorCommon.EndFoldout();
        }
    }
}
#endif
