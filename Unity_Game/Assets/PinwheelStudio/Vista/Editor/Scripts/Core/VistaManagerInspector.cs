#if VISTA
using Pinwheel.Vista;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

namespace Pinwheel.VistaEditor
{
    [CustomEditor(typeof(VistaManager))]
    public class VistaManagerInspector : Editor
    {
        private VistaManager m_instance;

        private void OnEnable()
        {
            m_instance = target as VistaManager;
            //SceneView.duringSceneGui += DuringSceneGUI;
            m_instance.drawGizmosSelectedCallback += OnDrawGizmosSelected;
        }

        private void OnDisable()
        {
            //SceneView.duringSceneGui -= DuringSceneGUI;
            m_instance.drawGizmosSelectedCallback -= OnDrawGizmosSelected;
        }

        public override void OnInspectorGUI()
        {
            DrawGeneralGUI();
            DrawBiomesGUI();
            DrawAddTilesGUI();
            DrawChildrenTilesGUI();
            DrawTileSettingsGUI();
            DrawObjectSpawningGUI();
            DrawCallbacksGUI();
            DrawActionGUI();
        }

        private class GeneralGUI
        {
            public static readonly string ID = "pinwheel.vista.vistamanager.general";
            public static readonly GUIContent HEADER = new GUIContent("General");

            public static readonly GUIContent VM_ID = new GUIContent("Id", "An id that identify this instance, mostly for cross scenes terrain referencing.");
        }

        private void DrawGeneralGUI()
        {
            if (EditorCommon.BeginFoldout(GeneralGUI.ID, GeneralGUI.HEADER, null, true))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(GeneralGUI.VM_ID, m_instance.id);
                EditorGUILayout.EndHorizontal();
            }
            EditorCommon.EndFoldout();
        }

        private class BiomesGUI
        {
            public static readonly string ID = "pinwheel.vista.vistamanager.biomes";
            public static readonly GUIContent HEADER = new GUIContent("Biomes");
            public static readonly GUIContent NO_BIOME = new GUIContent("No biome");
        }

        private void DrawBiomesGUI()
        {
            if (EditorCommon.BeginFoldout(BiomesGUI.ID, BiomesGUI.HEADER, null, false))
            {
                IBiome[] tiles = m_instance.GetBiomes();
                if (tiles.Length == 0)
                {
                    EditorGUILayout.LabelField(BiomesGUI.NO_BIOME, EditorCommon.Styles.infoLabel);
                }
                else
                {
                    foreach (IBiome t in tiles)
                    {
                        string goName = (t as Component).gameObject.name;
                        string typeName = t.GetType().Name;
                        string label = $"{goName} ({typeName})";
                        Rect r = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
                        if (GUI.Button(r, label, EditorCommon.Styles.italicLabel))
                        {
                            EditorGUIUtility.PingObject(t.gameObject);
                        }
                    }
                }
            }
            EditorCommon.EndFoldout();
        }

        private class AddTilesGUI
        {
            public static readonly string ID = "pinwheel.vista.vistamanager.addtiles";
            public static readonly GUIContent HEADER = new GUIContent("Add Tile");
        }

        private void DrawAddTilesGUI()
        {
            if (EditorCommon.BeginFoldout(AddTilesGUI.ID, AddTilesGUI.HEADER, null, true))
            {
                IEnumerable<ITerrainSystem> terrainSystems = VistaManager.GetTerrainSystems();
                StringBuilder sb = new StringBuilder();
                foreach (ITerrainSystem s in terrainSystems)
                {
                    System.Type terrainType = s.GetTerrainComponentType();
                    if (terrainType != null)
                    {
                        sb.Append("t:").Append(terrainType.Name).Append(',');
                    }
                }
                string filter = sb.ToString().Trim(',');

                List<GameObject> droppedObjects = EditorCommon.DropZone<GameObject>("Drop Terrain Game Objects here", filter, true);
                foreach (GameObject g in droppedObjects)
                {
                    foreach (ITerrainSystem s in terrainSystems)
                    {
                        s.SetupTile(m_instance, g);
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.GetControlRect(GUILayout.Width(EditorCommon.INDENT_WIDTH));
                foreach (ITerrainSystem s in terrainSystems)
                {
                    if (GUILayout.Button($"Add All {s.terrainLabel}s"))
                    {
                        Type terrainComponentType = s.GetTerrainComponentType();
                        Object[] terrainObjects = FindObjectsOfType(terrainComponentType);
                        foreach (Object o in terrainObjects)
                        {
                            if (o is Component comp)
                            {
                                s.SetupTile(m_instance, comp.gameObject);
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorCommon.EndFoldout();
        }

        private class ChildrenTilesGUI
        {
            public static readonly string ID = "pinwheel.vista.vistamanager.childrentiles";
            public static readonly GUIContent HEADER = new GUIContent("Children Tiles");
            public static readonly GUIContent NO_ACTIVE_TILE = new GUIContent("No active tile");
        }

        private void DrawChildrenTilesGUI()
        {
            if (EditorCommon.BeginFoldout(ChildrenTilesGUI.ID, ChildrenTilesGUI.HEADER, null, false))
            {
                List<ITile> tiles = m_instance.GetTiles();
                if (tiles.Count == 0)
                {
                    EditorGUILayout.LabelField(ChildrenTilesGUI.NO_ACTIVE_TILE, EditorCommon.Styles.infoLabel);
                }
                else
                {
                    foreach (ITile t in tiles)
                    {
                        string goName = (t as Component).gameObject.name;
                        string typeName = t.GetType().Name;
                        string label = $"{goName} ({typeName})";
                        Rect r = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
                        if (GUI.Button(r, label, EditorCommon.Styles.italicLabel))
                        {
                            EditorGUIUtility.PingObject(t.gameObject);
                        }
                    }
                }
            }
            EditorCommon.EndFoldout();
        }

        private class TileSettingsGUI
        {
            public static readonly string ID = "pinwheel.vista.vistamanager.tilesettings";
            public static readonly GUIContent HEADER = new GUIContent("Tile Settings");
            public static readonly GUIContent TERRAIN_MAX_HEIGHT = new GUIContent("Max Height", "The maximum height level of all tiles belong to this instance, will be applied in the next generation");
            public static readonly GUIContent HEIGHT_MAP_RESOLUTION = new GUIContent("Height Map Resolution", "Size of the terrain height map, will be applied in the next generation");
            public static readonly GUIContent TEXTURE_RESOLUTION = new GUIContent("Texture Resolution", "Size of the terrain control map (or albedo, metallic map, if present), will be applied in the next generation");
            public static readonly GUIContent DENSITY_MAP_RESOLUTION = new GUIContent("Detail Density Resolution", "Size of the detail density map, will be applied in the next generation");
            public static readonly GUIContent CULL_BIOMES = new GUIContent("Cull Biomes", "If on, a tile will only receive data from overlapped biomes, otherwise it will receive data from all biomes.");
        }

        private void DrawTileSettingsGUI()
        {
            if (EditorCommon.BeginFoldout(TileSettingsGUI.ID, TileSettingsGUI.HEADER))
            {
                EditorGUI.BeginChangeCheck();
                float maxHeight = EditorGUILayout.FloatField(TileSettingsGUI.TERRAIN_MAX_HEIGHT, m_instance.terrainMaxHeight);
                int heightmapResolution = EditorGUILayout.IntField(TileSettingsGUI.HEIGHT_MAP_RESOLUTION, m_instance.heightMapResolution);
                int textureResolution = EditorGUILayout.IntField(TileSettingsGUI.TEXTURE_RESOLUTION, m_instance.textureResolution);
                int detailDensityResolution = EditorGUILayout.IntField(TileSettingsGUI.DENSITY_MAP_RESOLUTION, m_instance.detailDensityMapResolution);
                bool cullBiomes = EditorGUILayout.Toggle(TileSettingsGUI.CULL_BIOMES, m_instance.shouldCullBiomes);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.terrainMaxHeight = maxHeight;
                    m_instance.heightMapResolution = heightmapResolution;
                    m_instance.textureResolution = textureResolution;
                    m_instance.detailDensityMapResolution = detailDensityResolution;
                    m_instance.shouldCullBiomes = cullBiomes;
                }
            }
            EditorCommon.EndFoldout();
        }

        private class ObjectSpawningGUI
        {
            public static readonly string ID = "pinwheel.vista.vistamanager.objectspawning";
            public static readonly GUIContent HEADER = new GUIContent("Object Spawning");
            public static readonly GUIContent OBJECT_PER_FRAME = new GUIContent("Object Per Frame", "Number of prefab instances to spawn in a frame");
        }

        private void DrawObjectSpawningGUI()
        {
            if (EditorCommon.BeginFoldout(ObjectSpawningGUI.ID, ObjectSpawningGUI.HEADER))
            {
                EditorGUI.BeginChangeCheck();
                int objectPerFrame = EditorGUILayout.IntField(ObjectSpawningGUI.OBJECT_PER_FRAME, m_instance.objectToSpawnPerFrame);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_instance, $"Modify {m_instance.name}");
                    m_instance.objectToSpawnPerFrame = objectPerFrame;
                    EditorUtility.SetDirty(m_instance);
                }
            }
            EditorCommon.EndFoldout();
        }

        private class CallbackGUI
        {
            public static readonly string ID = "pinwheel.vista.vistamanager.callbacks";
            public static readonly GUIContent HEADER = new GUIContent("Callbacks");

            public static readonly string CALLBACK_BEFORE_GENERATING = "m_beforeGeneratingUnityCallback";
            public static readonly string CALLBACK_AFTER_GENERATING = "m_afterGeneratingUnityCallback";
            public static readonly string SCRIPT_CALLBACK_INFO = "Other callbacks can be subscribed to in C# script.";
            public static readonly string GET_PIPELINE_DELEGATES_METHOD = "GetPipelineDelegates";
        }

        private void DrawCallbacksGUI()
        {
            if (EditorCommon.BeginFoldout(CallbackGUI.ID, CallbackGUI.HEADER, null, false))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.GetControlRect(GUILayout.Width(EditorCommon.INDENT_WIDTH));
                EditorGUILayout.BeginVertical();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CallbackGUI.CALLBACK_BEFORE_GENERATING));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(CallbackGUI.CALLBACK_AFTER_GENERATING));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.LabelField(CallbackGUI.SCRIPT_CALLBACK_INFO, EditorCommon.Styles.italicLabel);

                MethodInfo getDelegatesMethod = typeof(VistaManager).GetMethod(CallbackGUI.GET_PIPELINE_DELEGATES_METHOD, BindingFlags.NonPublic | BindingFlags.Static);
                if (getDelegatesMethod != null)
                {
                    List<string> names = new List<string>();
                    List<System.Delegate> delegates = new List<System.Delegate>();
                    getDelegatesMethod.Invoke(null, new object[] { names, delegates });

                    for (int i = 0; i < delegates.Count; ++i)
                    {
                        if (delegates[i] == null)
                            continue;
                        EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(names[i]), EditorStyles.boldLabel);
                        EditorGUI.indentLevel += 1;
                        System.Delegate[] invocationList = delegates[i].GetInvocationList();
                        List<string> invocationLabels = new List<string>();
                        foreach (System.Delegate d in invocationList)
                        {
                            string label = $"{d.Method.DeclaringType.Name}.{d.Method.Name}";
                            if (!invocationLabels.Contains(label))
                            {
                                invocationLabels.Add(label);
                            }
                        }
                        foreach (string l in invocationLabels)
                        {
                            EditorGUILayout.LabelField(l);
                        }
                        EditorGUI.indentLevel -= 1;
                    }
                }
            }
            EditorCommon.EndFoldout();
        }

        private class ActionGUI
        {
            public static readonly string ID = "pinwheel.vista.vistamanager.action";
            public static readonly GUIContent HEADER = new GUIContent("Actions");
            public static readonly GUIContent FORCE_UPDATE = new GUIContent("Force Update");
            public static readonly GUIContent FORGET_CURRENT_TASK = new GUIContent("Forget Current Task");
        }

        private void DrawActionGUI()
        {
            if (EditorCommon.BeginFoldout(ActionGUI.ID, ActionGUI.HEADER, null, true))
            {
                if (EditorCommon.Button(ActionGUI.FORCE_UPDATE))
                {
                    m_instance.ForceGenerate();
                }
                if (EditorCommon.Button(ActionGUI.FORGET_CURRENT_TASK))
                {
                    VistaManager.CancelActiveGenerateTask();
                }
            }
            EditorCommon.EndFoldout();
        }

        private void DuringSceneGUI(SceneView sv)
        {
            DrawChildrenTilesBounds();
        }

        private void DrawChildrenTilesBounds()
        {
            List<ITile> tiles = m_instance.GetTiles();
            foreach (ITile t in tiles)
            {
                Color c = (t == m_instance.currentlyProcessingTile) ? Color.yellow : new Color(0, 1, 1, 0.5f);
                using (HandleScope scope = new HandleScope(c, UnityEngine.Rendering.CompareFunction.LessEqual))
                {
                    Handles.DrawWireCube(t.worldBounds.center, t.worldBounds.size);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            bool isChildrenSectionExpanded = SessionState.GetBool(ChildrenTilesGUI.ID, false);
            if (isChildrenSectionExpanded)
            {
                DrawChildrenTilesBounds();
            }
        }
    }
}
#endif
