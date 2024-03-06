#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using Pinwheel.VistaEditor.Graph;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using Object = UnityEngine.Object;
using System.Linq;

namespace Pinwheel.VistaEditor
{
    [CustomEditor(typeof(LocalProceduralBiome))]
    public class LocalProceduralBiomeInspector : Editor
    {
        public delegate void InjectGUIHandler(LocalProceduralBiomeInspector inspector, LocalProceduralBiome biome);
        public static event InjectGUIHandler onEnableCallback;
        public static event InjectGUIHandler onDisableCallback;
        public static event InjectGUIHandler injectBiomeMaskGUICallback;
        public static event InjectGUIHandler drawExposedPropertiesCallback;

        public delegate void InjectSceneGUIHandler(LocalProceduralBiomeInspector inspector, LocalProceduralBiome biome, SceneView sceneView);
        public static event InjectSceneGUIHandler injectSceneGUICallback;

        public class ExitSceneGUIException : System.Exception { }

        private LocalProceduralBiome m_instance;
        private Editor m_terrainGraphInspector;

        private static bool? s_isBigWorldModuleInstalled;
        private static bool isBigWorldModuleInstalled
        {
            get
            {
                if (s_isBigWorldModuleInstalled == null)
                {
                    s_isBigWorldModuleInstalled = EditorCommon.HasAssembly("Pinwheel.Vista.BigWorld.Runtime");
                }
                return s_isBigWorldModuleInstalled.Value;
            }
        }

        internal class Prefs
        {
            public static bool isEditingAnchor;

            public static readonly string DEFERRED_UPDATE = "pinwheel.vista.localproceduralbiome.deferredupdate";
            public static bool useDeferredUpdate;

            public static void Load()
            {
                useDeferredUpdate = EditorPrefs.GetBool(DEFERRED_UPDATE, false);
            }

            public static void Save()
            {
                isEditingAnchor = false;
                EditorPrefs.SetBool(DEFERRED_UPDATE, useDeferredUpdate);
            }
        }

        private void OnEnable()
        {
            m_instance = target as LocalProceduralBiome;
            SceneView.duringSceneGui += DuringSceneGUI;
            Prefs.Load();

            onEnableCallback?.Invoke(this, m_instance);
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            Prefs.Save();

            if (m_terrainGraphInspector != null)
            {
                Object.DestroyImmediate(m_terrainGraphInspector);
            }

            onDisableCallback?.Invoke(this, m_instance);
        }

        public override void OnInspectorGUI()
        {
            DrawOrphanedBiomeWarningGUI();
            DrawGeneralGUI();
            DrawGenerationConfigsGUI();
            DrawTextureInputsGUI();
            DrawPositionInputsGUI();
            DrawAnchorsGUI();
            DrawBiomeMaskGUI();
            DrawCachingGUI();
            DrawBlendOptionsGUI();
            DrawActionGUI();
        }

        private void DrawOrphanedBiomeWarningGUI()
        {
            VistaManager vm = m_instance.GetVistaManagerInstance();
            if (vm == null)
            {
                EditorCommon.DrawWarning("This biome must be a child of a Vista Manager instance, otherwise it won't take effect.");
            }
        }

        private class GeneralGUI
        {
            public static readonly string ID = "pinwheel.vista.localproceduralbiome.general";
            public static readonly GUIContent HEADER = new GUIContent("General");
            public static readonly GUIContent ORDER = new GUIContent("Order", "The order of this biome among others, used for biomes sorting");
            public static readonly GUIContent TERRAIN_GRAPH = new GUIContent("Terrain Graph", "The Terrain Graph Asset used for generating this biome");
            public static readonly GUIContent EDIT_GRAPH = new GUIContent("Edit/Sync");

            public static readonly string NULL_GRAPH_WARNING = "You need to assign a Terrain Graph asset.";

            public static readonly string EXPOSED_PROPERTY_ID = "pinwheel.vista.localproceduralbiome.graph.exposedproperties";
            public static readonly GUIContent EXPOSED_PROPERTIES = new GUIContent("Exposed Properties");
        }

        private void DrawGeneralGUI()
        {
            if (EditorCommon.BeginFoldout(GeneralGUI.ID, GeneralGUI.HEADER, null, true))
            {
                GUI.enabled = !Prefs.isEditingAnchor;
                EditorGUI.BeginChangeCheck();
                int order = m_instance.order;
                if (isBigWorldModuleInstalled)
                {
                    order = EditorGUILayout.IntField(GeneralGUI.ORDER, m_instance.order);
                }
                EditorGUILayout.BeginHorizontal();
                TerrainGraph terrainGraph = EditorGUILayout.ObjectField(GeneralGUI.TERRAIN_GRAPH, m_instance.terrainGraph, typeof(TerrainGraph), false) as TerrainGraph;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.order = order;
                    m_instance.terrainGraph = terrainGraph;
                    if (!Prefs.useDeferredUpdate)
                    {
                        MarkChangedAndGenerate();
                    }
                }

                GUI.enabled &= m_instance.terrainGraph != null;
                if (GUILayout.Button(GeneralGUI.EDIT_GRAPH, GUILayout.Width(75)))
                {
                    if (m_instance.terrainGraph != null)
                    {
                        TerrainGenerationConfigs configs = CreateDebugConfig();
                        m_instance.terrainGraph.debugConfigs = configs;

                        TerrainGraphEditor graphEditor = TerrainGraphEditor.OpenGraph<TerrainGraphEditor>(m_instance.terrainGraph, new LPBInputProvider(m_instance));
                        TerrainGraph cloneGraph = graphEditor.clonedGraph as TerrainGraph;
                        cloneGraph.debugConfigs = configs;
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
                if (terrainGraph == null)
                {
                    EditorCommon.DrawWarning(GeneralGUI.NULL_GRAPH_WARNING, true);
                }
                else if (terrainGraph.HasExposedProperties && drawExposedPropertiesCallback != null)
                {
                    Rect box = EditorGUILayout.BeginVertical();
                    GUI.Box(box, string.Empty, EditorStyles.helpBox);
                    EditorGUILayout.Space();
                    bool expanded = SessionState.GetBool(GeneralGUI.EXPOSED_PROPERTY_ID, true);
                    expanded = EditorGUILayout.Foldout(expanded, GeneralGUI.EXPOSED_PROPERTIES, true, EditorCommon.Styles.foldoutBold);
                    SessionState.SetBool(GeneralGUI.EXPOSED_PROPERTY_ID, expanded);
                    if (expanded)
                    {
                        EditorGUI.indentLevel += 1;
                        drawExposedPropertiesCallback.Invoke(this, m_instance);
                        EditorGUI.indentLevel -= 1;
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                }
            }
            EditorCommon.EndFoldout();
        }

        TerrainGenerationConfigs CreateDebugConfig()
        {
            float maxHeight = 600;
            VistaManager vm = m_instance.GetVistaManagerInstance();
            if (vm != null)
            {
                maxHeight = vm.terrainMaxHeight;
            }

            Bounds worldBounds = m_instance.worldBounds;
            TerrainGenerationConfigs configs = TerrainGenerationConfigs.Create();
            configs.resolution = m_instance.baseResolution;
            configs.seed = m_instance.seed;
            configs.terrainHeight = maxHeight;
            configs.worldBounds = new Rect(m_instance.space == Space.World ? worldBounds.min.x : 0, m_instance.space == Space.World ? worldBounds.min.z : 0, worldBounds.size.x, worldBounds.size.z);
            m_instance.terrainGraph.debugConfigs = configs;
            return configs;
        }

        private class GenerationConfigsGUI
        {
            public static readonly string ID = "pinwheel.vista.localproceduralbiome.generationconfigs";
            public static readonly GUIContent HEADER = new GUIContent("Generation Configs");
            public static readonly GUIContent SPACE = new GUIContent("Space", "The coordinate for the generation. World space will affect some nodes (noise, etc) depends on the biome position, while Local space will not.");
            public static readonly GUIContent DATA_MASK = new GUIContent("Data Mask", $"Filter out the biome output where unnecessary data will be ignored. For example, if you uncheck {BiomeDataMask.HeightMap} flag, the graph won't output height data even when you have added a {ObjectNames.NicifyVariableName(typeof(HeightOutputNode).Name)}");
            public static readonly GUIContent BASE_RESOLUTION = new GUIContent("Base Resolution", "Base resolution for generated textures to inherit from. Final result will depends on the graph.");
            public static readonly GUIContent PPM = new GUIContent("Pixel Per Meter", "The number of pixels to cover 1 meter in world space, calculated based on the Base Resolution and the biome anchors. Higher value means higher quality but uses more VRAM.");
            public static readonly GUIContent SEED = new GUIContent("Seed", "An integer to randomize the result");
            public static readonly GUIContent COLLECT_SCENE_HEIGHT = new GUIContent("Collect Scene Height", $"Should it collect height data from the scene and feed to the graph as input? The input name is {GraphConstants.SCENE_HEIGHT_INPUT_NAME}");

            public static readonly string DATA_MASK_WARNING = "Nothing? Are you sure?";
            public static readonly string SCENE_HEIGHT_WARNING = $"There is no Input Node with the variable name of \"{GraphConstants.SCENE_HEIGHT_INPUT_NAME}\", consider to turn this checkbox off to improve its performance.";
        }

        private void DrawGenerationConfigsGUI()
        {
            if (EditorCommon.BeginFoldout(GenerationConfigsGUI.ID, GenerationConfigsGUI.HEADER, null, true))
            {
                GUI.enabled = !Prefs.isEditingAnchor;
                EditorGUI.BeginChangeCheck();
                Space space = (Space)EditorGUILayout.EnumPopup(GenerationConfigsGUI.SPACE, m_instance.space);
                BiomeDataMask dataMask = (BiomeDataMask)EditorGUILayout.EnumFlagsField(GenerationConfigsGUI.DATA_MASK, m_instance.dataMask);
                if (dataMask == 0)
                {
                    EditorCommon.DrawWarning(GenerationConfigsGUI.DATA_MASK_WARNING, true);
                }

                int baseResolution = EditorGUILayout.DelayedIntField(GenerationConfigsGUI.BASE_RESOLUTION, m_instance.baseResolution);
                GUI.enabled = false;
                Bounds bounds = m_instance.worldBounds;
                float ppm = baseResolution * 1.0f / Mathf.Max(bounds.size.x, bounds.size.z);
                EditorGUILayout.FloatField(GenerationConfigsGUI.PPM, ppm);
                GUI.enabled = !Prefs.isEditingAnchor;
                int seed = EditorGUILayout.DelayedIntField(GenerationConfigsGUI.SEED, m_instance.seed);
                bool collectSceneHeight = EditorGUILayout.Toggle(GenerationConfigsGUI.COLLECT_SCENE_HEIGHT, m_instance.shouldCollectSceneHeight);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.space = space;
                    m_instance.dataMask = dataMask;
                    m_instance.baseResolution = baseResolution;
                    m_instance.seed = seed;
                    m_instance.shouldCollectSceneHeight = collectSceneHeight;
                    if (!Prefs.useDeferredUpdate)
                    {
                        MarkChangedAndGenerate();
                    }
                }
                GUI.enabled = true;
                CheckAndShowSceneHeightWarning();
            }
            EditorCommon.EndFoldout();
        }

        private void CheckAndShowSceneHeightWarning()
        {
            if (!m_instance.shouldCollectSceneHeight)
                return;

            TerrainGraph graph = m_instance.terrainGraph;
            if (graph == null)
                return;
            List<InputNode> inputNodes = graph.GetNodesOfType<InputNode>();
            bool willShowWarning = true;
            for (int i = 0; i < inputNodes.Count; ++i)
            {
                if (string.Equals(inputNodes[i].inputName, GraphConstants.SCENE_HEIGHT_INPUT_NAME))
                {
                    willShowWarning = false;
                }
            }

            if (willShowWarning)
            {
                EditorCommon.DrawWarning(GenerationConfigsGUI.SCENE_HEIGHT_WARNING, true);
            }
        }

        private class TextureInputsGUI
        {
            public static readonly GUIContent TEXTURE_INPUTS = new GUIContent("Texture Inputs", "Providing textures that can be accessed with Input Node. The input name you typed in here should match with the one in the graph. Make sure to use unique name for each entry.");
            public static readonly string TEXTURE_INPUTS_PROP_NAME = "m_textureInputs";
        }

        private void DrawTextureInputsGUI()
        {
            SerializedProperty textureInputsProp = serializedObject.FindProperty(TextureInputsGUI.TEXTURE_INPUTS_PROP_NAME);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(textureInputsProp, TextureInputsGUI.TEXTURE_INPUTS, true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.Space();
        }

        private class PositionInputsGUI
        {
            public static readonly GUIContent POSITION_INPUTS = new GUIContent("Position Inputs", "Providing positions that can be accessed with Input Node. The input name you typed in here should match with the one in the graph. Make sure to use unique name for each entry.");
            public static readonly string POSITION_INPUTS_PROP_NAME = "m_positionInputs";
        }

        private void DrawPositionInputsGUI()
        {
            SerializedProperty positionInputsProp = serializedObject.FindProperty(PositionInputsGUI.POSITION_INPUTS_PROP_NAME);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(positionInputsProp, PositionInputsGUI.POSITION_INPUTS, true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.Space();
        }

        private class AnchorsGUI
        {
            public static readonly string ID = "pinwheel.vista.localproceduralbiome.anchors";
            public static readonly GUIContent HEADER = new GUIContent("Anchors");
            public static readonly GUIContent FALLOFF_DIRECTION = new GUIContent("Falloff Direction", "If set to Outer, the total size of the biome expands, otherwise the total size persist.");
            public static readonly GUIContent FALLOFF_DISTANCE = new GUIContent("Falloff Distance", "Determine the size of the biome falloff area");
            public static readonly GUIContent WORLD_BOUNDS = new GUIContent("Bounds", "Boundaries of the biome in world space");
            public static readonly GUIContent SHOW_OVERLAPPED_TILES = new GUIContent("Show Overlapped Tiles", "Highlight the overlapped tiles in the scene view");

            public static readonly GUIContent EDIT_ANCHORS_HELP = new GUIContent(
                "- Use arrow gizmos to move an anchor.\n" +
                "- Shift Click to add a new anchor between the 2 nearest ones.\n" +
                "- Ctrl Click on an anchor to remove it.");
            public static readonly GUIContent EDIT_ANCHORS = new GUIContent("Edit Anchors");
            public static readonly GUIContent END_EDIT_ANCHORS = new GUIContent("End Editing Anchors");
            public static readonly GUIContent DOWN_ARROW = new GUIContent("▼");


            public static readonly GUIContent SNAP_CURRENT_TILE = new GUIContent("Snap To Current Tile");
            public static readonly GUIContent SNAP_SELECTED_TILES = new GUIContent("Snap To Selected Tiles");
            public static readonly GUIContent SNAP_ALL_TILES = new GUIContent("Snap To All Tiles");

            public static readonly GUIContent CENTERIZE_PIVOT_POINT = new GUIContent("Centerize Pivot Point");
            public static readonly GUIContent FLIP = new GUIContent("Flip");
            public static readonly GUIContent SQUARE = new GUIContent("Square");
            public static readonly GUIContent CIRCLE = new GUIContent("Circle");
        }

        private void DrawAnchorsGUI()
        {
            if (EditorCommon.BeginFoldout(AnchorsGUI.ID, AnchorsGUI.HEADER, null, true))
            {
                GUI.enabled = !Prefs.isEditingAnchor;
                EditorGUI.BeginChangeCheck();
                FalloffDirection falloffDirection = (FalloffDirection)EditorGUILayout.EnumPopup(AnchorsGUI.FALLOFF_DIRECTION, m_instance.falloffDirection);
                float falloffDistance = EditorGUILayout.DelayedFloatField(AnchorsGUI.FALLOFF_DISTANCE, m_instance.falloffDistance);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.falloffDirection = falloffDirection;
                    m_instance.falloffDistance = falloffDistance;
                    if (!Prefs.useDeferredUpdate)
                    {
                        MarkChangedAndGenerate();
                    }
                }
                GUI.enabled = true;

                GUI.enabled = false;
                EditorGUILayout.BoundsField(AnchorsGUI.WORLD_BOUNDS, m_instance.worldBounds);
                GUI.enabled = true;

                if (Prefs.isEditingAnchor)
                {
                    EditorGUILayout.LabelField(AnchorsGUI.EDIT_ANCHORS_HELP, EditorCommon.Styles.infoLabel);
                }

                EditorGUILayout.BeginHorizontal();
                EditorCommon.IndentSpace();
                if (!Prefs.isEditingAnchor)
                {
                    if (GUILayout.Button(AnchorsGUI.EDIT_ANCHORS, EditorStyles.miniButtonLeft))
                    {
                        Prefs.isEditingAnchor = true;
                        SceneView.RepaintAll();
                    }
                }
                else
                {
                    if (GUILayout.Button(AnchorsGUI.END_EDIT_ANCHORS, EditorStyles.miniButtonLeft))
                    {
                        Prefs.isEditingAnchor = false;
                        if (!Prefs.useDeferredUpdate)
                        {
                            MarkChangedAndGenerate();
                        }
                    }
                }
                if (GUILayout.Button(AnchorsGUI.DOWN_ARROW, EditorStyles.miniButtonRight, GUILayout.Width(25)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(AnchorsGUI.FLIP, false, FlipAnchors);
                    menu.AddSeparator(null);
                    menu.AddItem(AnchorsGUI.SQUARE, false, SetAnchorsSquare);
                    menu.AddItem(AnchorsGUI.CIRCLE, false, SetAnchorsCircle);

                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();

                GUI.enabled = !Prefs.isEditingAnchor;
                EditorGUILayout.BeginHorizontal();
                EditorCommon.IndentSpace();
                if (GUILayout.Button(AnchorsGUI.SNAP_CURRENT_TILE, EditorStyles.miniButtonLeft))
                {
                    SnapToCurrentTile();
                    if (!Prefs.useDeferredUpdate)
                    {
                        MarkChangedAndGenerate();
                    }
                }
                if (GUILayout.Button(AnchorsGUI.SNAP_SELECTED_TILES, EditorStyles.miniButtonMid))
                {
                    SnapToSelectedTiles();
                    if (!Prefs.useDeferredUpdate)
                    {
                        MarkChangedAndGenerate();
                    }
                }
                if (GUILayout.Button(AnchorsGUI.SNAP_ALL_TILES, EditorStyles.miniButtonRight))
                {
                    SnapToAllTiles();
                    if (!Prefs.useDeferredUpdate)
                    {
                        MarkChangedAndGenerate();
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (EditorCommon.Button(AnchorsGUI.CENTERIZE_PIVOT_POINT, EditorStyles.miniButton))
                {
                    CenterizePivotPoint();
                }
                GUI.enabled = true;
            }
            EditorCommon.EndFoldout();
        }

        private void SnapToCurrentTile()
        {
            VistaManager vmInstance = m_instance.GetComponentInParent<VistaManager>();
            if (vmInstance == null)
                return;

            List<ITile> tiles = new List<ITile>();
            IEnumerable<ITerrainSystem> terrainSystems = VistaManager.GetTerrainSystems();
            foreach (ITerrainSystem ts in terrainSystems)
            {
                Type tileComponentType = ts.GetTileComponentType();
                Object[] tileObjects = Object.FindObjectsOfType(tileComponentType);
                foreach (Object t in tileObjects)
                {
                    if (t is ITile itile && string.Equals(vmInstance.id, itile.managerId))
                    {
                        tiles.Add(itile);
                    }
                }
            }

            Vector3 pos = m_instance.transform.position;
            ITile currentTile = null;
            foreach (ITile t in tiles)
            {
                Bounds b = t.worldBounds;
                if (pos.x >= b.min.x && pos.x <= b.max.x &&
                    pos.z >= b.min.z && pos.z <= b.max.z)
                {
                    currentTile = t;
                    break;
                }
            }

            if (currentTile == null)
                return;

            Bounds worldBounds = currentTile.worldBounds;
            Undo.RecordObject(m_instance.transform, $"Modify {m_instance.name}");
            m_instance.transform.position = new Vector3(worldBounds.center.x, 0, worldBounds.center.z);

            Vector3[] anchors = new Vector3[4];
            anchors[0] = m_instance.transform.InverseTransformPoint(new Vector3(worldBounds.min.x, 0, worldBounds.min.z));
            anchors[1] = m_instance.transform.InverseTransformPoint(new Vector3(worldBounds.min.x, 0, worldBounds.max.z));
            anchors[2] = m_instance.transform.InverseTransformPoint(new Vector3(worldBounds.max.x, 0, worldBounds.max.z));
            anchors[3] = m_instance.transform.InverseTransformPoint(new Vector3(worldBounds.max.x, 0, worldBounds.min.z));
            Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
            m_instance.anchors = anchors;
        }

        private void SnapToSelectedTiles()
        {
            VistaManager vmInstance = m_instance.GetComponentInParent<VistaManager>();
            if (vmInstance == null)
                return;

            List<ITile> tiles = new List<ITile>();
            GameObject[] selectedObjects = Selection.gameObjects;
            foreach (GameObject g in selectedObjects)
            {
                ITile itile = g.GetComponent<ITile>();
                if (itile != null && string.Equals(vmInstance.id, itile.managerId))
                {
                    tiles.Add(itile);
                }
            }

            if (tiles.Count == 0)
                return;

            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;
            foreach (ITile t in tiles)
            {
                Bounds b = t.worldBounds;
                minX = Mathf.Min(minX, b.min.x);
                minZ = Mathf.Min(minZ, b.min.z);
                maxX = Mathf.Max(maxX, b.max.x);
                maxZ = Mathf.Max(maxZ, b.max.z);
            }

            Undo.RecordObject(m_instance.transform, $"Modify {m_instance.name}");
            m_instance.transform.position = new Vector3((minX + maxX) * 0.5f, 0, (minZ + maxZ) * 0.5f);

            Vector3[] anchors = new Vector3[4];
            anchors[0] = m_instance.transform.InverseTransformPoint(new Vector3(minX, 0, minZ));
            anchors[1] = m_instance.transform.InverseTransformPoint(new Vector3(minX, 0, maxZ));
            anchors[2] = m_instance.transform.InverseTransformPoint(new Vector3(maxX, 0, maxZ));
            anchors[3] = m_instance.transform.InverseTransformPoint(new Vector3(maxX, 0, minZ));
            Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
            m_instance.anchors = anchors;
        }

        private void SnapToAllTiles()
        {
            VistaManager vmInstance = m_instance.GetComponentInParent<VistaManager>();
            if (vmInstance == null)
                return;

            List<ITile> tiles = new List<ITile>();
            IEnumerable<ITerrainSystem> terrainSystems = VistaManager.GetTerrainSystems();
            foreach (ITerrainSystem ts in terrainSystems)
            {
                Type tileComponentType = ts.GetTileComponentType();
                Object[] tileObjects = Object.FindObjectsOfType(tileComponentType);
                foreach (Object t in tileObjects)
                {
                    if (t is ITile itile && string.Equals(vmInstance.id, itile.managerId))
                    {
                        tiles.Add(itile);
                    }
                }
            }

            if (tiles.Count == 0)
                return;

            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;
            foreach (ITile t in tiles)
            {
                Bounds b = t.worldBounds;
                minX = Mathf.Min(minX, b.min.x);
                minZ = Mathf.Min(minZ, b.min.z);
                maxX = Mathf.Max(maxX, b.max.x);
                maxZ = Mathf.Max(maxZ, b.max.z);
            }

            Undo.RecordObject(m_instance.transform, $"Modify {m_instance.name}");
            m_instance.transform.position = new Vector3((minX + maxX) * 0.5f, 0, (minZ + maxZ) * 0.5f);

            Vector3[] anchors = new Vector3[4];
            anchors[0] = m_instance.transform.InverseTransformPoint(new Vector3(minX, 0, minZ));
            anchors[1] = m_instance.transform.InverseTransformPoint(new Vector3(minX, 0, maxZ));
            anchors[2] = m_instance.transform.InverseTransformPoint(new Vector3(maxX, 0, maxZ));
            anchors[3] = m_instance.transform.InverseTransformPoint(new Vector3(maxX, 0, minZ));
            Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
            m_instance.anchors = anchors;
        }

        private void CenterizePivotPoint()
        {
            Vector3[] anchors = m_instance.anchors;
            if (anchors.Length == 0)
                return;

            Vector3 sum = Vector3.zero;
            foreach (Vector3 v in anchors)
            {
                sum += v;
            }
            Vector3 avg = sum / anchors.Length;
            Vector3 offset = m_instance.transform.TransformPoint(avg) - m_instance.transform.position;
            for (int i = 0; i < anchors.Length; ++i)
            {
                anchors[i] -= offset;
            }

            m_instance.transform.position = m_instance.transform.position + offset;
            m_instance.anchors = anchors;
        }

        private void FlipAnchors()
        {
            Vector3[] anchors = m_instance.anchors;
            Vector3[] flippedAnchors = new Vector3[anchors.Length];
            for (int i = 0; i < anchors.Length; ++i)
            {
                flippedAnchors[anchors.Length - 1 - i] = anchors[i];
            }
            m_instance.anchors = flippedAnchors;
            SceneView.RepaintAll();
        }

        private void SetAnchorsSquare()
        {
            Undo.RecordObject(m_instance, "Modify biome anchors");
            m_instance.anchors = new Vector3[]
            {
                new Vector3(-500, 0, -500), new Vector3(-500, 0, 500), new Vector3(500, 0, 500), new Vector3(500, 0, -500)
            };

            if (!Prefs.useDeferredUpdate)
            {
                MarkChangedAndGenerate();
            }
        }

        private void SetAnchorsCircle()
        {
            List<Vector3> anchors = new List<Vector3>();
            int segmentCount = 16;
            for (int i = 0; i < segmentCount; ++i)
            {
                float deg = 360 * (i * 1f / segmentCount);
                Vector3 v = new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad), 0, Mathf.Sin(deg * Mathf.Deg2Rad)) * 500;
                anchors.Add(v);
            }
            Undo.RecordObject(m_instance, "Modify biome anchors");
            m_instance.anchors = anchors.ToArray();

            if (!Prefs.useDeferredUpdate)
            {
                MarkChangedAndGenerate();
            }
        }

        private class BiomeMaskGUI
        {
            public static readonly string ID = "pinwheel.vista.localproceduralbiome.biomemask";
            public static readonly GUIContent HEADER = new GUIContent("Biome Mask");
            public static readonly GUIContent RESOLUTION = new GUIContent("Resolution", "Size of the biome mask texture. This texture will be used to blend multiple biomes together.");
            public static readonly GUIContent POST_PROCESS_GRAPH = new GUIContent("Post Process", "Post process the biome mask for better blending");
            public static readonly GUIContent EDIT_GRAPH = new GUIContent("Edit/Sync");

            public static readonly string BIOME_MASK_INPUT_WARNING = $"There is no Input Node with the variable name of {GraphConstants.BIOME_MASK_INPUT_NAME}, please add one.";
            public static readonly string BIOME_MASK_OUTPUT_WARNING = $"There is no Output Node with the variable name of {GraphConstants.BIOME_MASK_OUTPUT_NAME}, please add one.";
        }

        private void DrawBiomeMaskGUI()
        {
            if (!isBigWorldModuleInstalled)
            {
                m_instance.biomeMaskGraph = null;
                return;
            }

            if (EditorCommon.BeginFoldout(BiomeMaskGUI.ID, BiomeMaskGUI.HEADER, null, true))
            {
                GUI.enabled = !Prefs.isEditingAnchor;
                EditorGUI.BeginChangeCheck();
                int resolution = EditorGUILayout.DelayedIntField(BiomeMaskGUI.RESOLUTION, m_instance.biomeMaskResolution);
                EditorGUILayout.BeginHorizontal();
                BiomeMaskGraph graph = EditorGUILayout.ObjectField(BiomeMaskGUI.POST_PROCESS_GRAPH, m_instance.biomeMaskGraph, typeof(BiomeMaskGraph), false) as BiomeMaskGraph;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.biomeMaskResolution = resolution;
                    m_instance.biomeMaskGraph = graph;
                    if (!Prefs.useDeferredUpdate)
                    {
                        MarkChangedAndGenerate();
                    }
                }

                GUI.enabled &= m_instance.biomeMaskGraph != null;
                if (GUILayout.Button(BiomeMaskGUI.EDIT_GRAPH, GUILayout.Width(75)))
                {
                    if (m_instance.biomeMaskGraph != null)
                    {
                        TerrainGenerationConfigs configs = CreateDebugConfig();
                        m_instance.biomeMaskGraph.debugConfigs = configs;

                        BiomeMaskGraphEditor graphEditor = BiomeMaskGraphEditor.OpenGraph<BiomeMaskGraphEditor>(m_instance.biomeMaskGraph, new LPBInputProvider(m_instance));
                        BiomeMaskGraph cloneGraph = graphEditor.clonedGraph as BiomeMaskGraph;
                        cloneGraph.debugConfigs = configs;
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
                CheckAndShowBiomeMaskIOWarning();

                injectBiomeMaskGUICallback?.Invoke(this, m_instance);
            }
            EditorCommon.EndFoldout();
        }

        private void CheckAndShowBiomeMaskIOWarning()
        {
            BiomeMaskGraph graph = m_instance.biomeMaskGraph;
            if (graph == null)
                return;
            List<InputNode> inputNodes = graph.GetNodesOfType<InputNode>();
            bool willShowInputWarning = true;
            for (int i = 0; i < inputNodes.Count; ++i)
            {
                if (string.Equals(inputNodes[i].inputName, GraphConstants.BIOME_MASK_INPUT_NAME))
                {
                    willShowInputWarning = false;
                }
            }

            if (willShowInputWarning)
            {
                EditorCommon.DrawWarning(BiomeMaskGUI.BIOME_MASK_INPUT_WARNING, true);
            }

            List<OutputNode> outputNodes = graph.GetNodesOfType<OutputNode>();
            bool willShowOutputWarning = true;
            for (int i = 0; i < outputNodes.Count; ++i)
            {
                if (string.Equals(outputNodes[i].outputName, GraphConstants.BIOME_MASK_OUTPUT_NAME))
                {
                    willShowOutputWarning = false;
                }
            }

            if (willShowOutputWarning)
            {
                EditorCommon.DrawWarning(BiomeMaskGUI.BIOME_MASK_OUTPUT_WARNING, true);
            }
        }

        private class CachingGUI
        {
            public static readonly string ID = "pinwheel.vista.localproceduralbiome.caching";
            public static readonly GUIContent HEADER = new GUIContent("Caching");
            public static readonly GUIContent CLEAN_UP_MODE = new GUIContent("Clean Up Mode", $"Decide when to release the cache data. Choose {LocalProceduralBiome.CleanUpMode.EachIteration} when your graph is still in draft to ensure data is up to date. Choose {LocalProceduralBiome.CleanUpMode.Manually} if you're working with another biome and don't want to re-generate this one every time. Note that the cache is forced to clean up on some event such as scene reloading, object deactivating, etc.");

            public static readonly GUIContent CACHE = new GUIContent("Cache");
            public static Vector2 cacheScrollPos;
        }

        private void DrawCachingGUI()
        {
            if (!isBigWorldModuleInstalled)
            {
                m_instance.cleanUpMode = LocalProceduralBiome.CleanUpMode.EachIteration;
                return;
            }

            if (EditorCommon.BeginFoldout(CachingGUI.ID, CachingGUI.HEADER, null, true))
            {
                GUI.enabled = !Prefs.isEditingAnchor;
                EditorGUI.BeginChangeCheck();
                LocalProceduralBiome.CleanUpMode cleanUpMode = (LocalProceduralBiome.CleanUpMode)EditorGUILayout.EnumPopup(CachingGUI.CLEAN_UP_MODE, m_instance.cleanUpMode);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.cleanUpMode = cleanUpMode;
                    m_instance.CleanUp();
                }

                BiomeData cachedData = null;
                PropertyInfo propInfo = m_instance.GetType().GetProperty("cachedData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (propInfo != null)
                {
                    cachedData = propInfo.GetValue(m_instance) as BiomeData;
                }

                EditorCommon.Header(CachingGUI.CACHE);
                if (cachedData != null)
                {
                    Rect scrollRect = EditorGUILayout.BeginVertical();
                    EditorCommon.Box(scrollRect);
                    CachingGUI.cacheScrollPos = EditorGUILayout.BeginScrollView(CachingGUI.cacheScrollPos, GUILayout.MaxHeight(188));
                    EditorGUILayout.LabelField("Textures");
                    EditorGUILayout.BeginHorizontal();
                    EditorCommon.IndentSpace();
                    if (cachedData.heightMap != null)
                    {
                        DrawCacheTexture(cachedData.heightMap, "Height Map");
                    }
                    if (cachedData.holeMap != null)
                    {
                        DrawCacheTexture(cachedData.holeMap, "Hole Map");
                    }
                    if (cachedData.meshDensityMap != null)
                    {
                        DrawCacheTexture(cachedData.meshDensityMap, "Mesh Density Map");
                    }
                    if (cachedData.albedoMap != null)
                    {
                        DrawCacheTexture(cachedData.albedoMap, "Albedo Map");
                    }
                    if (cachedData.metallicMap != null)
                    {
                        DrawCacheTexture(cachedData.metallicMap, "Metallic Map");
                    }

                    List<TerrainLayer> layers = new List<TerrainLayer>();
                    List<RenderTexture> layerWeights = new List<RenderTexture>();
                    cachedData.GetLayerWeights(layers, layerWeights);
                    for (int i = 0; i < layers.Count; ++i)
                    {
                        DrawCacheTexture(layerWeights[i], $"Layer Weight: {layers[i].name}");
                    }

                    List<DetailTemplate> detailTemplates = new List<DetailTemplate>();
                    List<RenderTexture> detailDensity = new List<RenderTexture>();
                    cachedData.GetDensityMaps(detailTemplates, detailDensity);
                    for (int i = 0; i < detailTemplates.Count; ++i)
                    {
                        DrawCacheTexture(detailDensity[i], $"Density Map: {detailTemplates[i].name}");
                    }

                    List<string> genericTextureLabel = new List<string>();
                    List<RenderTexture> genericTexture = new List<RenderTexture>();
                    cachedData.GetGenericTextures(genericTextureLabel, genericTexture);
                    for (int i = 0; i < genericTextureLabel.Count; ++i)
                    {
                        DrawCacheTexture(genericTexture[i], $"{genericTextureLabel}");
                    }

                    if (cachedData.biomeMaskMap != null)
                    {
                        DrawCacheTexture(cachedData.biomeMaskMap, "Biome Mask");
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("Trees");
                    List<TreeTemplate> treeTemplates = new List<TreeTemplate>();
                    List<ComputeBuffer> treeBuffers = new List<ComputeBuffer>();
                    cachedData.GetTrees(treeTemplates, treeBuffers);
                    if (treeTemplates.Count == 0)
                    {
                        EditorGUILayout.LabelField("None", EditorCommon.Styles.grayMiniLabel);
                    }
                    else
                    {
                        for (int i = 0; i < treeTemplates.Count; ++i)
                        {
                            int instanceCount = treeBuffers[i].count / InstanceSample.SIZE;
                            EditorGUILayout.LabelField($"{treeTemplates[i].name}: {instanceCount} instance{(instanceCount >= 2 ? "s" : "")}", EditorCommon.Styles.grayMiniLabel);
                        }
                    }

                    EditorGUILayout.LabelField("Detail Instances");
                    List<DetailTemplate> detailTemplates_Instance = new List<DetailTemplate>();
                    List<ComputeBuffer> detailBuffer = new List<ComputeBuffer>();
                    cachedData.GetDetailInstances(detailTemplates_Instance, detailBuffer);
                    if (detailTemplates_Instance.Count == 0)
                    {
                        EditorGUILayout.LabelField("None", EditorCommon.Styles.grayMiniLabel);
                    }
                    else
                    {
                        for (int i = 0; i < detailTemplates_Instance.Count; ++i)
                        {
                            int instanceCount = detailBuffer[i].count / InstanceSample.SIZE;
                            EditorGUILayout.LabelField($"{detailTemplates_Instance[i].name}: {instanceCount} instance{(instanceCount >= 2 ? "s" : "")}", EditorCommon.Styles.grayMiniLabel);
                        }
                    }

                    EditorGUILayout.LabelField("Objects");
                    List<ObjectTemplate> objectTemplates = new List<ObjectTemplate>();
                    List<ComputeBuffer> objectBuffers = new List<ComputeBuffer>();
                    cachedData.GetObjects(objectTemplates, objectBuffers);
                    if (objectTemplates.Count == 0)
                    {
                        EditorGUILayout.LabelField("None", EditorCommon.Styles.grayMiniLabel);
                    }
                    else
                    {
                        for (int i = 0; i < objectTemplates.Count; ++i)
                        {
                            int instanceCount = objectBuffers[i].count / InstanceSample.SIZE;
                            EditorGUILayout.LabelField($"{objectTemplates[i].name}: {instanceCount} instance{(instanceCount >= 2 ? "s" : "")}", EditorCommon.Styles.grayMiniLabel);
                        }
                    }

                    EditorGUILayout.LabelField("Generic Buffers");
                    List<string> genericBufferLabels = new List<string>();
                    List<ComputeBuffer> genericBuffers = new List<ComputeBuffer>();
                    cachedData.GetGenericBuffers(genericBufferLabels, genericBuffers);
                    if (genericBufferLabels.Count == 0)
                    {
                        EditorGUILayout.LabelField("None", EditorCommon.Styles.grayMiniLabel);
                    }
                    else
                    {
                        for (int i = 0; i < genericBufferLabels.Count; ++i)
                        {
                            int instanceCount = genericBuffers[i].count / PositionSample.SIZE;
                            EditorGUILayout.LabelField($"{genericBufferLabels[i]}: {instanceCount} instance{(instanceCount >= 2 ? "s" : "")}", EditorCommon.Styles.grayMiniLabel);
                        }
                    }

                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.LabelField("Not available");
                }
                GUI.enabled = true;
            }
            EditorCommon.EndFoldout();
        }

        private void DrawCacheTexture(RenderTexture t, string label)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(128), GUILayout.Height(128 + EditorGUIUtility.singleLineHeight));
            Rect textureRect = new Rect(r.min.x, r.min.y, r.width, r.width);
            EditorGUI.DrawPreviewTexture(textureRect, t);
            using (IndentScope s = new IndentScope(0))
            {
                Rect labelRect = new Rect(r.min.x, r.max.y - EditorGUIUtility.singleLineHeight, r.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, new GUIContent(label, label), EditorCommon.Styles.grayMiniLabel);
            }
        }

        private class BlendOptionsGUI
        {
            public static readonly string ID = "pinwheel.vista.localproceduralbiome.blendoptions";
            public static readonly GUIContent HEADER = new GUIContent("Blending");

            public static readonly GUIContent HEIGHT_MAP_BLEND_MODE = new GUIContent(
                "Height Map",
                "Blend mode for the height map.\n" +
                "Let A be the current blend result, B be the height map of this biome:\n" +
                "- Linear: The height map will gradually change from A to B.\n" +
                "- Additive: B will be added to A.\n" +
                "- Subtractive: B will be subtracted from A.\n" +
                "- Min/Max: Take the min/max value of A and B.");

            public static readonly GUIContent TEXTURE_BLEND_MODE = new GUIContent(
                "Textures",
                "Blend mode for terrain textures, including alpha maps, albedo map, metallic map & generic textures.\n" +
                "Only Linear mode is available in this version, which means the texture will gradually change from one to the other.");

            public static readonly GUIContent DETAIL_DENSITY_BLEND_MODE = new GUIContent(
                "Detail Density",
                "Blend mode for detail density maps. You will use Linear or Additive for most cases.\n" +
                "Let A be the current blend result, B be the density map of a detail template in this biome:\n" +
                "- Linear: The density map will gradually change from A to B, which means a grass type will win over the others and become dominant.\n" +
                "- Additive: B will be added to A, which means many grass species will coexist in the region.\n" +
                "- Subtractive: B will be subtracted from A.\n" +
                "- Min/Max: Take the min/max value of A and B.");

            public static readonly GUIContent INSTANCES_BLEND_MODES = new GUIContent(
                "Instances",
                "Blend mode for instance buffers, including trees, grass instances (Polaris), objects & generic buffers.\n" +
                "Let A be the current blend result, B be the instances of a tree template in this biome:\n" +
                "- Linear: The tree instances will gradually change from A to B, which means a tree type will win over the others and become dominant.\n" +
                "- Additive: B will be added to A, which means many tree species will coexist in the region.");

            public static readonly GUIContent DEFAULT = new GUIContent("Default");
        }

        private void DrawBlendOptionsGUI()
        {
            if (!isBigWorldModuleInstalled)
                return;

            if (EditorCommon.BeginFoldout(BlendOptionsGUI.ID, BlendOptionsGUI.HEADER, null, false))
            {
                GUI.enabled = !Prefs.isEditingAnchor;
                BiomeBlendOptions options = m_instance.blendOptions;
                EditorGUI.BeginChangeCheck();
                options.heightMapBlendMode = (BiomeBlendOptions.TextureBlendMode)EditorGUILayout.EnumPopup(BlendOptionsGUI.HEIGHT_MAP_BLEND_MODE, options.heightMapBlendMode);

                GUI.enabled = false;
                EditorGUILayout.EnumPopup(BlendOptionsGUI.TEXTURE_BLEND_MODE, BiomeBlendOptions.TextureBlendMode.Linear);
                GUI.enabled = !Prefs.isEditingAnchor;

                options.detailDensityBlendMode = (BiomeBlendOptions.TextureBlendMode)EditorGUILayout.EnumPopup(BlendOptionsGUI.DETAIL_DENSITY_BLEND_MODE, options.detailDensityBlendMode);
                options.instancesBlendMode = (BiomeBlendOptions.BufferBlendMode)EditorGUILayout.EnumPopup(BlendOptionsGUI.INSTANCES_BLEND_MODES, options.instancesBlendMode);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify ${m_instance.name}");
                    m_instance.blendOptions = options;
                    MarkChangedAndGenerate();
                }
                if (EditorCommon.Button(BlendOptionsGUI.DEFAULT))
                {
                    Undo.RecordObject(m_instance, $"Modify ${m_instance.name}");
                    m_instance.blendOptions = BiomeBlendOptions.Default();
                    MarkChangedAndGenerate();
                }
                GUI.enabled = true;
            }
            EditorCommon.EndFoldout();
        }

        private class ActionGUI
        {
            public static readonly string ID = "pinwheel.vista.localproceduralbiome.action";
            public static readonly GUIContent HEADER = new GUIContent("Actions");
            public static readonly GUIContent DEFERRED_UPDATE = new GUIContent("Deferred Update", "Turn this on if you want to make several changes to the biome before re-generating. You have to click Force Update.");
            public static readonly GUIContent FORCE_UPDATE = new GUIContent("Force Update");
        }

        private void DrawActionGUI()
        {
            if (EditorCommon.BeginFoldout(ActionGUI.ID, ActionGUI.HEADER, null, true))
            {
                GUI.enabled = !Prefs.isEditingAnchor;
                Prefs.useDeferredUpdate = EditorGUILayout.Toggle(ActionGUI.DEFERRED_UPDATE, Prefs.useDeferredUpdate);
                if (EditorCommon.Button(ActionGUI.FORCE_UPDATE))
                {
                    MarkChangedAndGenerate();
                }
                GUI.enabled = true;
            }
            EditorCommon.EndFoldout();
        }

        private class DebugGUI
        {
            public static readonly string ID = "pinwheel.vista.localproceduralbiome.debug";
            public static readonly GUIContent HEADER = new GUIContent("Debug");
        }

        private class SceneGUI
        {
            public static readonly string SHOW_OVERLAPPED_TILES_KEY = "pinwheel.vista.localproceduralbiome.scenegui.showoverlappedtiles";

            public static readonly float ANCHOR_SIZE = 0.1f;
            public static readonly Color ANCHOR_COLOR = Color.white;
            public static readonly Color SEGMENT_COLOR = Color.red;
            public static readonly Color FALLOFF_COLOR = new Color(SEGMENT_COLOR.r, SEGMENT_COLOR.g, SEGMENT_COLOR.b, 0.5f);
            public static readonly float SEGMENT_WIDTH = 5;
        }

        private void DuringSceneGUI(SceneView sv)
        {
            bool willDrawDefaultGUI = true;
            try
            {
                injectSceneGUICallback?.Invoke(this, m_instance, sv);
            }
            catch (ExitSceneGUIException)
            {
                willDrawDefaultGUI = false;
            }
            if (willDrawDefaultGUI)
            {
                DrawOverlappedTilesBounds();
                DrawAnchors(Prefs.isEditingAnchor, 1);
            }
        }

        private void DrawOverlappedTilesBounds()
        {
            VistaManager manager = m_instance.GetComponentInParent<VistaManager>();
            if (manager == null)
                return;
            List<ITile> tiles = manager.GetTiles();
            using (new HandleScope(new Color(0, 1, 1, 0.5f), UnityEngine.Rendering.CompareFunction.LessEqual))
            {
                foreach (ITile t in tiles)
                {
                    Bounds bounds = t.worldBounds;
                    if (m_instance.IsOverlap(bounds))
                    {
                        Handles.DrawWireCube(bounds.center, bounds.size);
                    }
                }
            }
        }

        public void DrawAnchors(bool editMode, float alphaMul = 1)
        {
            m_instance.transform.position = AnchorUtilities.SnapToWorld(m_instance.transform.position);

            Color c;
            Vector3[] srcAnchors = m_instance.anchors;
            AnchorUtilities.Transform(srcAnchors, m_instance.transform.localToWorldMatrix);
            AnchorUtilities.SnapToWorld(srcAnchors);

            if (srcAnchors.Length > 1)
            {
                Vector3[] falloffAnchors = m_instance.falloffAnchors;
                AnchorUtilities.Transform(falloffAnchors, m_instance.transform.localToWorldMatrix);
                AnchorUtilities.SnapToWorld(falloffAnchors);

                c = SceneGUI.FALLOFF_COLOR;
                c.a *= alphaMul;
                Handles.color = c;
                Handles.DrawAAPolyLine(SceneGUI.SEGMENT_WIDTH, falloffAnchors[0], falloffAnchors[srcAnchors.Length - 1]);
                Handles.DrawAAPolyLine(SceneGUI.SEGMENT_WIDTH, falloffAnchors);

                c = SceneGUI.SEGMENT_COLOR;
                c.a *= alphaMul;
                Handles.color = c;
                Handles.DrawAAPolyLine(SceneGUI.SEGMENT_WIDTH, srcAnchors[0], srcAnchors[srcAnchors.Length - 1]);
                Handles.DrawAAPolyLine(SceneGUI.SEGMENT_WIDTH, srcAnchors);
            }

            if (!editMode || Event.current.alt)
            {
                Tools.hidden = false;
            }
            else
            {
                Tools.hidden = true;

                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < srcAnchors.Length; ++i)
                {
                    srcAnchors[i] = Handles.PositionHandle(srcAnchors[i], Quaternion.identity);
                }

                for (int i = 0; i < srcAnchors.Length; ++i)
                {
                    c = SceneGUI.ANCHOR_COLOR;
                    c.a *= alphaMul;
                    Handles.color = c;
                    float buttonSize = HandleUtility.GetHandleSize(srcAnchors[i]) * SceneGUI.ANCHOR_SIZE;
                    if (Handles.Button(srcAnchors[i], Quaternion.identity, buttonSize, buttonSize, Handles.CubeHandleCap))
                    {
                        if (Event.current.control && Event.current.button == 0)
                        {
                            srcAnchors = AnchorUtilities.RemoveAt(srcAnchors, i);
                            GUI.changed = true;
                        }
                        Event.current.Use();
                    }
                }
                if (Event.current.shift)
                {
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(r, out hit))
                        {
                            Vector3 newAnchor = hit.point;
                            srcAnchors = AnchorUtilities.Insert(srcAnchors, newAnchor);
                            GUI.changed = true;
                        }
                        else
                        {
                            Plane p = new Plane(Vector3.up, m_instance.transform.position);
                            float d;
                            if (p.Raycast(r, out d))
                            {
                                Vector3 newAnchor = r.origin + r.direction * d;
                                srcAnchors = AnchorUtilities.Insert(srcAnchors, newAnchor);
                                GUI.changed = true;
                            }
                        }
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify ${m_instance.name}");
                    AnchorUtilities.FlattenY(srcAnchors, 0);
                    AnchorUtilities.Transform(srcAnchors, m_instance.transform.worldToLocalMatrix);
                    m_instance.anchors = srcAnchors;
                    EditorUtility.SetDirty(m_instance);
                    GUI.changed = true;
                }

                CatchHotControl();
            }
        }

        private void CatchHotControl()
        {
            int controlId = GUIUtility.GetControlID(this.GetHashCode(), FocusType.Passive);
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    //Set the hot control to this tool, to disable marquee selection tool on mouse dragging
                    GUIUtility.hotControl = controlId;
                }
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                if (GUIUtility.hotControl == controlId)
                {
                    //Return the hot control back to Unity, use the default
                    GUIUtility.hotControl = 0;
                }
            }
        }

        private void DrawBounds()
        {
            Bounds bounds = m_instance.worldBounds;
            using (HandleScope s = new HandleScope(Color.yellow, UnityEngine.Rendering.CompareFunction.Always))
            {
                Handles.DrawWireCube(bounds.center, bounds.size);
            }
        }

        public void MarkChangedAndGenerate()
        {
            EditorUtility.SetDirty(m_instance);
            if (m_instance.terrainGraph != null)
            {
                m_instance.CleanUp();
                m_instance.MarkChanged();
                m_instance.GenerateBiomesInGroup();
            }
        }

        public void ExitSceneGUI()
        {
            throw new ExitSceneGUIException();
        }
    }
}
#endif
