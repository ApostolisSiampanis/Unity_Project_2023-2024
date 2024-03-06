#if VISTA
using Pinwheel.Vista;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.VistaEditor
{
    [CustomEditor(typeof(DetailTemplate))]
    public class DetailTemplateInspector : Editor
    {
        public enum DisplayMode
        {
            UnityTerrain = 0,
#if GRIFFIN
            Polaris = 1,
#endif
            All = 2
        }

        private const string VIEW_KEY_DISPLAY_MODE = "pinwheel.vista.detailtemplate.displaymode";


        private DetailTemplate m_instance;
        private DisplayMode m_displayMode;

        private void OnEnable()
        {
            m_instance = target as DetailTemplate;
            m_displayMode = (DisplayMode)EditorPrefs.GetInt(VIEW_KEY_DISPLAY_MODE, 0);
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt(VIEW_KEY_DISPLAY_MODE, (int)m_displayMode);
        }

        private static readonly GUIContent DISPLAY = new GUIContent("Display", "Inspector display mode, to show only properties that matter in your favourite terrain system");
        private static readonly GUIContent RENDER_MODE = new GUIContent("Render Mode", "Determine how to render the detail");
        private static readonly GUIContent TEXTURE = new GUIContent("Texture", "The grass texture");
        private static readonly GUIContent TEXTURE_VARIANTS = new GUIContent("Variants", "Variants of the texture, this will allow you to spawn different detail textures using the same graph data");
        private static readonly GUIContent PREFAB = new GUIContent("Prefab", "The game object to render as detail");
        private static readonly GUIContent PREFAB_VARIANTS = new GUIContent("Variants", "Variants of the prefab, this allow you to spawn different detail prefabs using the same graph data");
        private static readonly GUIContent PRIMARY_COLOR = new GUIContent("Primary Color", "Detail's primary color");
        private static readonly GUIContent SECONDARY_COLOR = new GUIContent("Secondary Color", "Detail's secondary color");
        private static readonly GUIContent MIN_HEIGHT = new GUIContent("Min Height", "Grass minimum/base height");
        private static readonly GUIContent MAX_HEIGHT = new GUIContent("Max Height", "Grass maximum height");
        private static readonly GUIContent MIN_WIDTH = new GUIContent("Min Width", "Grass minimum/base width");
        private static readonly GUIContent MAX_WIDTH = new GUIContent("Max Width", "Grass maximum width");
        private static readonly GUIContent NOISE_SPREAD = new GUIContent("Noise Spread", "Grass noise spread");
        private static readonly GUIContent HOLE_EDGE_PADDING = new GUIContent("Hole Edge Padding", "Controls how far away detail objects are from the edge of the hole area");
        private static readonly GUIContent USE_INSTANCING = new GUIContent("Use Instancing", "Enable GPU Instancing");
        private static readonly GUIContent PIVOT_OFFSET = new GUIContent("Pivot Offset", "Move grass instances up/down along Y-axis");
        private static readonly GUIContent BEND_FACTOR = new GUIContent("Bend Factor", "How much grass react to wind");
        private static readonly GUIContent LAYER = new GUIContent("Layer", "The layer to render grass instances to");
        private static readonly GUIContent ALIGN_TO_SURFACE = new GUIContent("Align To Surface", "Determine if grass should align to surface normal");
        private static readonly GUIContent CAST_SHADOW = new GUIContent("Cast Shadow", "Shadow casting mode");
        private static readonly GUIContent RECEIVE_SHADOW = new GUIContent("Receive Shadow", "Should they receive shadow?");
        private static readonly GUIContent DENSITY = new GUIContent("Density", "Base density of the detail template");
        private static readonly string VEGETATION_ASSETS_AFF_TEXT = "Explore high quality vegetation assets";

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DisplayMode displayMode = (DisplayMode)EditorGUILayout.EnumPopup(DISPLAY, m_displayMode);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Change Detail Template display mode");
                m_displayMode = displayMode;
            }

            EditorGUI.BeginChangeCheck();
            DetailRenderMode renderMode = (DetailRenderMode)EditorGUILayout.EnumPopup(RENDER_MODE, m_instance.renderMode);
            Texture2D texture = m_instance.texture;
            GameObject prefab = m_instance.prefab;
            if (renderMode == DetailRenderMode.Grass || renderMode == DetailRenderMode.GrassBillboard)
            {
                texture = EditorGUILayout.ObjectField(TEXTURE, m_instance.texture, typeof(Texture2D), false) as Texture2D;
            }
            else if (renderMode == DetailRenderMode.VertexLit)
            {
                prefab = EditorGUILayout.ObjectField(PREFAB, m_instance.prefab, typeof(GameObject), false) as GameObject;
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                m_instance.renderMode = renderMode;
                m_instance.texture = texture;
                m_instance.prefab = prefab;
                EditorUtility.SetDirty(m_instance);
                serializedObject.Update();
            }

            if ((renderMode == DetailRenderMode.Grass || renderMode == DetailRenderMode.GrassBillboard) && m_instance.texture != null)
            {
                if (TemplateUtils.IsVariantsSupported())
                {
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty variantsProps = serializedObject.FindProperty("m_textureVariants");
                    EditorGUILayout.PropertyField(variantsProps, TEXTURE_VARIANTS, true);
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
            else if (renderMode == DetailRenderMode.VertexLit && m_instance.prefab != null)
            {
                if (TemplateUtils.IsVariantsSupported())
                {
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty variantsProps = serializedObject.FindProperty("m_prefabVariants");
                    EditorGUILayout.PropertyField(variantsProps, PREFAB_VARIANTS, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(m_instance);
                        serializedObject.Update();
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            Color primaryColor = EditorGUILayout.ColorField(PRIMARY_COLOR, m_instance.primaryColor);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                m_instance.primaryColor = primaryColor;
                EditorUtility.SetDirty(m_instance);
                serializedObject.Update();
            }

            if (m_displayMode == DisplayMode.UnityTerrain || m_displayMode == DisplayMode.All)
            {
                EditorGUI.BeginChangeCheck();
                Color secondaryColor = EditorGUILayout.ColorField(SECONDARY_COLOR, m_instance.secondaryColor);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.secondaryColor = secondaryColor;
                    EditorUtility.SetDirty(m_instance);
                    serializedObject.Update();
                }
            }

            EditorGUI.BeginChangeCheck();
            float minWidth = EditorGUILayout.FloatField(MIN_WIDTH, m_instance.minWidth);
            float minHeight = EditorGUILayout.FloatField(MIN_HEIGHT, m_instance.minHeight);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                m_instance.minWidth = minWidth;
                m_instance.minHeight = minHeight;
                EditorUtility.SetDirty(m_instance);
                serializedObject.Update();
            }

            if (m_displayMode == DisplayMode.UnityTerrain || m_displayMode == DisplayMode.All)
            {
                EditorGUI.BeginChangeCheck();
                float maxWidth = EditorGUILayout.FloatField(MAX_WIDTH, m_instance.maxWidth);
                float maxHeight = EditorGUILayout.FloatField(MAX_HEIGHT, m_instance.maxHeight);
                float noiseSpread = EditorGUILayout.FloatField(NOISE_SPREAD, m_instance.noiseSpread);
                float holeEdgePadding = EditorGUILayout.Slider(HOLE_EDGE_PADDING, m_instance.holeEdgePadding, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.maxWidth = maxWidth;
                    m_instance.maxHeight = maxHeight;
                    m_instance.noiseSpread = noiseSpread;
                    m_instance.holeEdgePadding = holeEdgePadding;
                    EditorUtility.SetDirty(m_instance);
                    serializedObject.Update();
                }

#if UNITY_2021_2_OR_NEWER
                if (renderMode == DetailRenderMode.VertexLit)
                {
                    EditorGUI.BeginChangeCheck();
                    bool useInstancing = EditorGUILayout.Toggle(USE_INSTANCING, m_instance.useInstancing);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                        m_instance.useInstancing = useInstancing;
                        EditorUtility.SetDirty(m_instance);
                        serializedObject.Update();
                    }
                }
#endif
            }

            if (m_displayMode != DisplayMode.UnityTerrain)
            {
                EditorGUI.BeginChangeCheck();
                float pivotOffset = EditorGUILayout.FloatField(PIVOT_OFFSET, m_instance.pivotOffset);
                float bendFactor = EditorGUILayout.FloatField(BEND_FACTOR, m_instance.bendFactor);
                int layer = EditorGUILayout.LayerField(LAYER, m_instance.layer);
                bool alignToSurface = EditorGUILayout.Toggle(ALIGN_TO_SURFACE, m_instance.alignToSurface);
                ShadowCastingMode castShadow = (ShadowCastingMode)EditorGUILayout.EnumPopup(CAST_SHADOW, m_instance.castShadow);
                bool receiveShadow = EditorGUILayout.Toggle(RECEIVE_SHADOW, m_instance.receiveShadow);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.pivotOffset = pivotOffset;
                    m_instance.bendFactor = bendFactor;
                    m_instance.layer = layer;
                    m_instance.alignToSurface = alignToSurface;
                    m_instance.castShadow = castShadow;
                    m_instance.receiveShadow = receiveShadow;
                    EditorUtility.SetDirty(m_instance);
                    serializedObject.Update();
                }
            }

            EditorGUI.BeginChangeCheck();
            int density = EditorGUILayout.IntField(DENSITY, m_instance.density);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                m_instance.density = density;
                EditorUtility.SetDirty(m_instance);
                serializedObject.Update();
            }

            EditorCommon.DrawAffLinks(VEGETATION_ASSETS_AFF_TEXT, Links.VEGETATION_ASSETS);
        }
    }
}
#endif
