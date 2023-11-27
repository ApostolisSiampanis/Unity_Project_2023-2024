#if VISTA
using Pinwheel.Vista.Graph;
using Pinwheel.VistaEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TextureDisplayMode = Pinwheel.VistaEditor.EditorSettings.GraphEditorSettings.TextureDisplayMode;
using ViewportGradientOptions = Pinwheel.VistaEditor.EditorSettings.GraphEditorSettings.ViewportGradientOptions;

namespace Pinwheel.VistaEditor.Graph
{
    public class TerrainGraphPropertiesDrawer : IGraphPropertiesDrawer
    {
        private static readonly GUIContent PREVIEW_CONFIGS_HEADER = new GUIContent("PREVIEW CONFIGS", "Settings to render preview terrain in the viewports");

        private static readonly GUIContent EXECUTION_HEADER = new GUIContent("EXECUTION", "Settings for graph execution");
        private static readonly GUIContent ALLOW_SPLIT_EXECUTION = new GUIContent("Allow Split Execution", "Toggling Split Execution mode for this graph, you also need to setup children nodes to decide which nodes to split the execution");


        private static readonly GUIContent VIEWPORTS_HEADER = new GUIContent("VIEWPORTS", "Settings for the 3D & 2D viewport");
        private static readonly GUIContent GRADIENT_MODE = new GUIContent("Gradient Mode", "Generated textures/masks will be displayed using this gradient for better visualization");
        private static readonly GUIContent CUSTOM_GRADIENT_TEXTURE = new GUIContent("Custom Gradient Texture", "Provide your horizontal gradient texture here for 3D/2D map visualization");
        private static readonly string GRADIENT_WARNING = "Please provide a gradient texture";

        private static readonly GUIContent VIEWPORT_3D_HEADER = new GUIContent("3D VIEWPORT", "Settings for the 3D viewport in the editor");
        private static readonly GUIContent TERRAIN_VIS_QUALITY = new GUIContent("Terrain Quality", "The quality of terrain rendering in 3d viewport");
        private static readonly GUIContent SHOW_GRID = new GUIContent("Show Grid", "Render the grid in the 3d viewport");
        private static readonly GUIContent SHOW_WATER_LEVEL = new GUIContent("Show Water Level", "Render the water plane in the 3d viewport");
        private static readonly GUIContent WATER_LEVEL = new GUIContent("Water Level", "Altitude of the water line");
        private static readonly GUIContent TEXTURE_DISPLAY_MODE = new GUIContent("Default Texture Display", "How to visualize the generated texture when you select a node");

        private static readonly GUIContent TROUBLESHOOTING_HEADER = new GUIContent("TROUBLESHOOTING");
        private static readonly GUIContent ENABLE_TROUBLESHOOTING_MODE = new GUIContent("Enable", "Enable trouble shooting mode, this will perform logging on graph execution and other tasks to provide useful insight for debugging. No personal info recorded.");
        private static readonly GUIContent DONT_EXECUTE_ON_SELECTION = new GUIContent("Don't Execute Graph", "Turn this on will prevent it from running the graph when selecting a node. Only use when there is problem with a node and you need to delete it from the graph.");


        public GraphEditorBase editor { get; set; }

        public void OnDrawProperties(GraphAsset graph)
        {
            EditorSettings editorSettings = EditorSettings.Get();
            EditorSettings.GraphEditorSettings graphEditorSettings = editorSettings.graphEditorSettings;
            TerrainGraph instance = graph as TerrainGraph;

            EditorCommon.Header(PREVIEW_CONFIGS_HEADER);
            EditorGUI.BeginChangeCheck();
            TerrainGenerationConfigs debugConfigs = EditorCommon.TerrainGenConfigField(instance.debugConfigs);
            if (EditorGUI.EndChangeCheck())
            {
                editor.RegisterUndo("Modify Graph Settings");
                instance.debugConfigs = debugConfigs;
                EditorUtility.SetDirty(instance);
            }

            EditorCommon.Header(EXECUTION_HEADER);
            EditorGUI.BeginChangeCheck();
            bool allowSplitExecution = EditorGUILayout.Toggle(ALLOW_SPLIT_EXECUTION, instance.allowSplitExecution);
            if (EditorGUI.EndChangeCheck())
            {
                editor.RegisterUndo("Modify Graph Settings");
                instance.allowSplitExecution = allowSplitExecution;
                EditorUtility.SetDirty(instance);
            }

            EditorCommon.Header(VIEWPORTS_HEADER);
            EditorGUI.BeginChangeCheck();
            ViewportGradientOptions viewportGradient = (ViewportGradientOptions)EditorGUILayout.EnumPopup(GRADIENT_MODE, graphEditorSettings.viewportGradient);
            Texture2D customGradient = graphEditorSettings.customViewportGradient;
            if (viewportGradient == ViewportGradientOptions.Custom)
            {
                customGradient = EditorCommon.InlineTexture2DField(CUSTOM_GRADIENT_TEXTURE, graphEditorSettings.customViewportGradient);
                if (customGradient == null)
                {
                    EditorCommon.DrawWarning(GRADIENT_WARNING, true);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editorSettings, "Modify Editor Settings");
                graphEditorSettings.viewportGradient = viewportGradient;
                graphEditorSettings.customViewportGradient = customGradient;
                EditorUtility.SetDirty(editorSettings);
                TerrainGraphViewport2d viewport2d = editor.rootVisualElement.Q<TerrainGraphViewport2d>();
                if (viewport2d != null)
                {
                    viewport2d.RenderViewport();
                }
                TerrainGraphViewport3d viewport3d = editor.rootVisualElement.Q<TerrainGraphViewport3d>();
                if (viewport3d != null)
                {
                    viewport3d.RenderViewport();
                }
            }

            EditorCommon.Header(VIEWPORT_3D_HEADER);
            EditorGUI.BeginChangeCheck();
            bool showGrid = EditorGUILayout.Toggle(SHOW_GRID, graphEditorSettings.showGrid);
            bool showWaterLevel = EditorGUILayout.Toggle(SHOW_WATER_LEVEL, graphEditorSettings.showWaterLevel);
            float waterLevel = EditorGUILayout.FloatField(WATER_LEVEL, graphEditorSettings.waterLevel);
            int terrainVisQuality = EditorGUILayout.IntSlider(TERRAIN_VIS_QUALITY, graphEditorSettings.terrainVisualizationQuality, EditorSettings.GraphEditorSettings.MIN_VIS_QUALITY, EditorSettings.GraphEditorSettings.MAX_VIS_QUALITY);
            TextureDisplayMode textureDisplayMode = (TextureDisplayMode)EditorGUILayout.EnumPopup(TEXTURE_DISPLAY_MODE, graphEditorSettings.defaultTextureDisplayMode);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editorSettings, "Modify Editor Settings");
                graphEditorSettings.showGrid = showGrid;
                graphEditorSettings.showWaterLevel = showWaterLevel;
                graphEditorSettings.waterLevel = waterLevel;
                graphEditorSettings.terrainVisualizationQuality = terrainVisQuality;
                graphEditorSettings.defaultTextureDisplayMode = textureDisplayMode;
                EditorUtility.SetDirty(editorSettings);

                TerrainGraphViewport3d viewport3d = editor.rootVisualElement.Q<TerrainGraphViewport3d>();
                if (viewport3d != null)
                {
                    viewport3d.UpdateToggleButtons();
                    viewport3d.RenderViewport();
                }
            }

            EditorCommon.Header(TROUBLESHOOTING_HEADER);
            EditorSettings.TroubleshootingSettings troubleshootingSettings = editorSettings.troubleshootingSettings;
            EditorGUI.BeginChangeCheck();
            bool enableTroubleshooting = EditorGUILayout.Toggle(ENABLE_TROUBLESHOOTING_MODE, troubleshootingSettings.enableTroubleshootingMode);
            bool dontExecuteGraphOnSelection = EditorGUILayout.Toggle(DONT_EXECUTE_ON_SELECTION, troubleshootingSettings.dontExecuteGraphOnSelection);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editorSettings, "Modify Editor Settings");
                troubleshootingSettings.enableTroubleshootingMode = enableTroubleshooting;
                troubleshootingSettings.dontExecuteGraphOnSelection = dontExecuteGraphOnSelection;
                EditorUtility.SetDirty(editorSettings);
            }
        }
    }
}
#endif
