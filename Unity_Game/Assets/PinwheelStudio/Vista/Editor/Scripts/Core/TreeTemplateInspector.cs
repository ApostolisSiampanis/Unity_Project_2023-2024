#if VISTA
using Pinwheel.Vista;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.VistaEditor
{
    [CustomEditor(typeof(TreeTemplate))]
    public class TreeTemplateInspector : Editor
    {
        public enum DisplayMode
        {
            UnityTerrain = 0,
#if GRIFFIN
            Polaris = 1,
#endif
            All = 2
        }

        private const string VIEW_KEY_DISPLAY_MODE = "pinwheel.vista.treetemplate.displaymode";

        private TreeTemplate m_instance;
        private DisplayMode m_displayMode;

        private void OnEnable()
        {
            m_instance = target as TreeTemplate;
            m_displayMode = (DisplayMode)EditorPrefs.GetInt(VIEW_KEY_DISPLAY_MODE, 0);
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt(VIEW_KEY_DISPLAY_MODE, (int)m_displayMode);
        }

        private static readonly GUIContent DISPLAY = new GUIContent("Display", "Inspector display mode, to show only properties that matter in your favourite terrain system");
        private static readonly GUIContent PREFAB = new GUIContent("Prefab", "The tree prefab");
        private static readonly GUIContent PREFAB_VARIANTS = new GUIContent("Variants", "Variants of the prefab");
        private static readonly GUIContent NAV_MESH_LOD = new GUIContent("Nav Mesh LOD", "The LOD to be used with Nav Mesh");
        private static readonly GUIContent BEND_FACTOR = new GUIContent("Bend Factor", "Bend factor for the tree");
        private static readonly GUIContent BILLBOARD = new GUIContent("Billboard", "Billboard asset for the tree");
        private static readonly GUIContent PIVOT_OFFSET = new GUIContent("Pivot Offset", "Offset the tree on Y-axis for better ground snapping");
        private static readonly GUIContent BASE_ROTATION = new GUIContent("Base Rotation", "Base rotation for the source tree model");
        private static readonly GUIContent BASE_SCALE = new GUIContent("Base Scale", "Base scale for the source tree model");
        private static readonly GUIContent LAYER = new GUIContent("Layer", "The layer to render the tree");
        private static readonly GUIContent KEEP_PREFAB_LAYER = new GUIContent("Keep Prefab Layer", "Use the layer from the tree prefab");
        private static readonly GUIContent CAST_SHADOW = new GUIContent("Cast Shadow", "Should the tree cast shadow?");
        private static readonly GUIContent RECEIVE_SHADOW = new GUIContent("Receive Shadow", "Should the tree receive shadow?");
        private static readonly GUIContent BILLBOARD_CAST_SHADOW = new GUIContent("Billboard Cast Shadow", "Should the tree cast shadow when rendered as billboard?");
        private static readonly GUIContent BILLBOARD_RECEIVE_SHADOW = new GUIContent("Billboard Receive Shadow", "Should the tree receive shadow when rendered as billboard??");
        private static readonly string VEGETATION_ASSETS_AFF_TEXT = "Explore high quality vegetation assets";


        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DisplayMode displayMode = (DisplayMode)EditorGUILayout.EnumPopup(DISPLAY, m_displayMode);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Change Tree Template display mode");
                m_displayMode = displayMode;
            }

            EditorGUI.BeginChangeCheck();
            GameObject prefab = EditorGUILayout.ObjectField(PREFAB, m_instance.prefab, typeof(GameObject), false) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                EditorUtility.SetDirty(m_instance);
                m_instance.prefab = prefab;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            if (prefab != null)
            {
                if (TemplateUtils.IsVariantsSupported())
                {
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty variantsProps = serializedObject.FindProperty("m_prefabVariants");
                    EditorGUILayout.PropertyField(variantsProps, PREFAB_VARIANTS, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                        EditorUtility.SetDirty(m_instance);
                    }
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    variantsProps.Dispose();
                }
            }

            if (m_displayMode == DisplayMode.UnityTerrain || m_displayMode == DisplayMode.All)
            {
                EditorGUI.BeginChangeCheck();
                int navMeshLod = EditorGUILayout.IntField(NAV_MESH_LOD, m_instance.navMeshLod);
                float bendFactor = EditorGUILayout.FloatField(BEND_FACTOR, m_instance.bendFactor);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.navMeshLod = navMeshLod;
                    m_instance.bendFactor = bendFactor;
                    EditorUtility.SetDirty(m_instance);
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }
            }

            if (m_displayMode != DisplayMode.UnityTerrain)
            {
                EditorGUI.BeginChangeCheck();
                BillboardAsset billboard = EditorGUILayout.ObjectField(BILLBOARD, m_instance.billboard, typeof(BillboardAsset), false) as BillboardAsset;

                float pivotOffset = EditorGUILayout.Slider(PIVOT_OFFSET, m_instance.pivotOffset, -1f, 1f);
                Quaternion baseRotation = Quaternion.Euler(EditorCommon.InlineVector3Field(BASE_ROTATION, m_instance.baseRotation.eulerAngles));
                Vector3 baseScale = EditorCommon.InlineVector3Field(BASE_SCALE, m_instance.baseScale);

                GUI.enabled = !m_instance.keepPrefabLayer;
                int layer = EditorGUILayout.LayerField(LAYER, m_instance.layer);
                GUI.enabled = true;
                bool keepPrefabLayer = EditorGUILayout.Toggle(KEEP_PREFAB_LAYER, m_instance.keepPrefabLayer);

                ShadowCastingMode castShadow = (ShadowCastingMode)EditorGUILayout.EnumPopup(CAST_SHADOW, m_instance.shadowCastingMode);
                bool receiveShadow = EditorGUILayout.Toggle(RECEIVE_SHADOW, m_instance.receiveShadow);

                ShadowCastingMode billboardCastShadow = (ShadowCastingMode)EditorGUILayout.EnumPopup(BILLBOARD_CAST_SHADOW, m_instance.billboardShadowCastingMode);
                bool billboardReceiveShadow = EditorGUILayout.Toggle(BILLBOARD_RECEIVE_SHADOW, m_instance.billboardReceiveShadow);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.billboard = billboard;
                    m_instance.pivotOffset = pivotOffset;
                    m_instance.baseRotation = baseRotation;
                    m_instance.baseScale = baseScale;
                    m_instance.layer = layer;
                    m_instance.keepPrefabLayer = keepPrefabLayer;
                    m_instance.shadowCastingMode = castShadow;
                    m_instance.receiveShadow = receiveShadow;
                    m_instance.billboardShadowCastingMode = billboardCastShadow;
                    m_instance.billboardReceiveShadow = billboardReceiveShadow;
                    EditorUtility.SetDirty(m_instance);
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }
            }

            EditorCommon.DrawAffLinks(VEGETATION_ASSETS_AFF_TEXT, Links.VEGETATION_ASSETS);
        }
    }
}
#endif
