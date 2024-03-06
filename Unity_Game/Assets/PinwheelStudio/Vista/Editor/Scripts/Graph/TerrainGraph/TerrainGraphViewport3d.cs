#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using Pinwheel.VistaEditor.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    public class TerrainGraphViewport3d : Viewport
    {
        private TerrainGraphEditor m_editor;
        internal WasdTracker m_wasdManipulator;

        private VisualElement m_buttonContainer;
        private UtilityButton m_takeScreenshotButton;
        private UtilityButton m_resetCameraButton;
        private UtilityButton m_defaultTextureDisplayModeButton;
        private UtilityButton m_toggleGridButton;
        private UtilityButton m_toggleWaterButton;

        private Vector3 m_cameraPosition;
        private Vector3 m_cameraRotation;
        private Vector3 m_modelRotation;
        private RenderTexture m_targetRt;
        private Mesh m_patchMesh;
        private Material m_terrainMaterial;
        private Material m_positionVisMaterial;
        private Material m_gridlineMaterial;
        private Material m_axisMaterial;
        private Material m_waterMaterial;

        private const string KEY_CAM_POS_X = "cam-pos-x";
        private const string KEY_CAM_POS_Y = "cam-pos-y";
        private const string KEY_CAM_POS_Z = "cam-pos-z";

        private const string KEY_CAM_ROT_X = "cam-rot-x";
        private const string KEY_CAM_ROT_Y = "cam-rot-y";
        private const string KEY_CAM_ROT_Z = "cam-rot-z";

        private const string KEY_MODEL_ROT_X = "model-rot-x";
        private const string KEY_MODEL_ROT_Y = "model-rot-y";
        private const string KEY_MODEL_ROT_Z = "model-rot-z";

        protected const string KEY_DISPLAY = "display";

        private static readonly int HEIGHT_MAP = Shader.PropertyToID("_HeightMap");
        private static readonly int MASK_MAP = Shader.PropertyToID("_MaskMap");
        private static readonly int GRADIENT_MAP = Shader.PropertyToID("_GradientMap");
        private static readonly int TERRAIN_SIZE = Shader.PropertyToID("_TerrainSize");
        private static readonly int TERRAIN_POS = Shader.PropertyToID("_TerrainPos");
        private static readonly int UV_REMAP = Shader.PropertyToID("_UvRemap");
        private static readonly int POSITION_SAMPLES = Shader.PropertyToID("_PositionSamples");
        private static readonly int ORIGIN = Shader.PropertyToID("_Origin");
        private static readonly int OFFSET = Shader.PropertyToID("_Offset");

        private static readonly string KW_MASK_IS_COLOR = "MASK_IS_COLOR";
        private static readonly string KW_DATA_TYPE_INSTANCE_SAMPLE = "DATA_TYPE_INSTANCE_SAMPLE";

        private static readonly string PATCH_MESH_NAME = "Vista/Meshes/Patch";
        private static readonly string TERRAIN_VIS_SHADER = "Hidden/Vista/TerrainGraphEditor/TerrainVisualize";
        private static readonly string POSITION_VIS_SHADER = "Hidden/Vista/TerrainGraphEditor/PositionVisualize";
        private static readonly string GRIDLINE_SHADER = "Hidden/Vista/TerrainGraphEditor/View3dGridline";
        private static readonly string AXIS_SHADER = "Hidden/Vista/TerrainGraphEditor/View3dAxis";
        private static readonly string WATER_SHADER = "Hidden/Vista/TerrainGraphEditor/Water";

        private List<Matrix4x4> m_terrainPatchTransforms = new List<Matrix4x4>();
        private List<Vector4> m_terrainPatchUvRemaps = new List<Vector4>();

        public delegate void AddUtilityButtonHandler(TerrainGraphViewport3d graphView, List<UtilityButton> buttons);
        public static event AddUtilityButtonHandler addUtilityButtonCallback;

        private struct RenderSetup
        {
            public const int MODE_MASK = 0;
            public const int MODE_SPLAT = 1;
            public const int MODE_COLOR = 2;

            public int renderMode { get; set; }
            public RenderTexture heightMap { get; set; }
            public RenderTexture maskMap { get; set; }
            public List<RenderTexture> splatDiffuseMap { get; set; }
            public List<RenderTexture> weightMaps { get; set; }
            public ComputeBuffer positionBuffer { get; set; }
            public bool isInstanceBuffer { get; set; }
        }

        public TerrainGraphViewport3d(TerrainGraphEditor editor) : base()
        {
            m_editor = editor;

            AddToClassList("viewport-3d");
            AddToClassList("panel");
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            EditorApplication.update += Update;

            m_wasdManipulator = new WasdTracker();
            this.AddManipulator(m_wasdManipulator);

            m_buttonContainer = new VisualElement() { name = "utility-button-container" };
            m_buttonContainer.AddToClassList("utility-button-container");
            Add(m_buttonContainer);

            m_takeScreenshotButton = new UtilityButton() { name = "take-screenshot-button" };
            m_takeScreenshotButton.image = Resources.Load<Texture2D>("Vista/Textures/TakeScreenshot");
            m_takeScreenshotButton.tooltip = "Take screenshot";
            m_takeScreenshotButton.clicked += OnTakeScreenshotButtonClicked;
            m_buttonContainer.Add(m_takeScreenshotButton);

            m_resetCameraButton = new UtilityButton() { name = "reset-camera-button" };
            m_resetCameraButton.image = Resources.Load<Texture2D>("Vista/Textures/ResetCamera");
            m_resetCameraButton.tooltip = "Reset camera";
            m_resetCameraButton.clicked += OnResetCameraButtonClicked;
            m_buttonContainer.Add(m_resetCameraButton);

            m_defaultTextureDisplayModeButton = new UtilityButton() { name = "default-texture-display-mode-button" };
            m_defaultTextureDisplayModeButton.image = Resources.Load<Texture2D>("Vista/Textures/Display3dHeight");
            m_defaultTextureDisplayModeButton.tooltip = "Default texture display mode";
            m_defaultTextureDisplayModeButton.clicked += OnDefaultTextureDisplayModeButtonClicked;
            m_buttonContainer.Add(m_defaultTextureDisplayModeButton);

            m_toggleGridButton = new UtilityButton() { name = "toggle-gridline-button" };
            m_toggleGridButton.image = Resources.Load<Texture2D>("Vista/Textures/Gridline");
            m_toggleGridButton.tooltip = "Toggle grid";
            m_toggleGridButton.clicked += OnGridlineButtonClicked;
            m_buttonContainer.Add(m_toggleGridButton);

            m_toggleWaterButton = new UtilityButton() { name = "toggle-water-button" };
            m_toggleWaterButton.image = Resources.Load<Texture2D>("Vista/Textures/WaterLevel");
            m_toggleWaterButton.tooltip = "Toggle water level";
            m_toggleWaterButton.clicked += OnWaterButtonClicked;
            m_buttonContainer.Add(m_toggleWaterButton);

            this.AddManipulator(new Draggable(OnDrag));
            this.AddManipulator(new Scrollable(OnScroll));

            if (addUtilityButtonCallback != null)
            {
                List<UtilityButton> additionalButtons = new List<UtilityButton>();
                addUtilityButtonCallback.Invoke(this, additionalButtons);
                foreach (UtilityButton b in additionalButtons)
                {
                    if (b != null)
                    {
                        m_buttonContainer.Add(b);
                    }
                }
            }

            UpdateToggleButtons();
        }

        public void OnEnable()
        {
            string typeName = GetType().Name;
            m_cameraPosition.x = EditorPrefs.GetFloat(typeName + KEY_CAM_POS_X, -500);
            m_cameraPosition.y = EditorPrefs.GetFloat(typeName + KEY_CAM_POS_Y, 500);
            m_cameraPosition.z = EditorPrefs.GetFloat(typeName + KEY_CAM_POS_Z, -500);

            m_cameraRotation.x = EditorPrefs.GetFloat(typeName + KEY_CAM_ROT_X, 15);
            m_cameraRotation.y = EditorPrefs.GetFloat(typeName + KEY_CAM_ROT_Y, 45);
            m_cameraRotation.z = EditorPrefs.GetFloat(typeName + KEY_CAM_ROT_Z, 0);

            m_modelRotation.x = EditorPrefs.GetFloat(typeName + KEY_MODEL_ROT_X, 0);
            m_modelRotation.y = EditorPrefs.GetFloat(typeName + KEY_MODEL_ROT_Y, 0);
            m_modelRotation.z = EditorPrefs.GetFloat(typeName + KEY_MODEL_ROT_Z, 0);

            this.style.display = (DisplayStyle)EditorPrefs.GetInt(typeName + KEY_DISPLAY, (int)DisplayStyle.Flex);

            EditorSettings editorSettings = EditorSettings.Get();
            bool shouldDrawGridline = editorSettings.graphEditorSettings.showGrid;
            m_toggleGridButton.SetToggled(shouldDrawGridline);

            bool shouldDrawWater = editorSettings.graphEditorSettings.showWaterLevel;
            m_toggleWaterButton.SetToggled(shouldDrawWater);

        }

        public void OnDisable()
        {
            string typeName = GetType().Name;
            EditorPrefs.SetFloat(typeName + KEY_CAM_POS_X, m_cameraPosition.x);
            EditorPrefs.SetFloat(typeName + KEY_CAM_POS_Y, m_cameraPosition.y);
            EditorPrefs.SetFloat(typeName + KEY_CAM_POS_Z, m_cameraPosition.z);
            EditorPrefs.SetFloat(typeName + KEY_CAM_ROT_X, m_cameraRotation.x);
            EditorPrefs.SetFloat(typeName + KEY_CAM_ROT_Y, m_cameraRotation.y);
            EditorPrefs.SetFloat(typeName + KEY_CAM_ROT_Z, m_cameraRotation.z);
            EditorPrefs.SetFloat(typeName + KEY_MODEL_ROT_X, m_modelRotation.x);
            EditorPrefs.SetFloat(typeName + KEY_MODEL_ROT_Y, m_modelRotation.y);
            EditorPrefs.SetFloat(typeName + KEY_MODEL_ROT_Z, m_modelRotation.z);

            EditorPrefs.SetInt(typeName + KEY_DISPLAY, (int)this.resolvedStyle.display);
            CleanUp();
        }

        private void Update()
        {
            if (m_wasdManipulator != null && m_wasdManipulator.isActive)
            {
                OnWasd(m_wasdManipulator.wasdInfo);
            }
        }

        public void RenderViewport()
        {
            if (m_editor.clonedGraph == null)
                return;
            if (this.resolvedStyle == null)
                return;

            if (m_patchMesh == null)
            {
                m_patchMesh = Resources.Load<Mesh>(PATCH_MESH_NAME);
                m_patchMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
            }
            if (m_terrainMaterial == null)
            {
                m_terrainMaterial = new Material(Shader.Find(TERRAIN_VIS_SHADER));
                m_terrainMaterial.enableInstancing = true;
            }
            if (m_positionVisMaterial == null)
            {
                m_positionVisMaterial = new Material(Shader.Find(POSITION_VIS_SHADER));
            }
            if (m_gridlineMaterial == null)
            {
                m_gridlineMaterial = new Material(Shader.Find(GRIDLINE_SHADER));
            }
            if (m_axisMaterial == null)
            {
                m_axisMaterial = new Material(Shader.Find(AXIS_SHADER));
            }
            if (m_waterMaterial == null)
            {
                m_waterMaterial = new Material(Shader.Find(WATER_SHADER));
            }

            if (m_targetRt == null)
            {
                int viewPortWidth = Mathf.Max(1, (int)this.resolvedStyle.width);
                int viewPortHeight = Mathf.Max(1, (int)this.resolvedStyle.height);
                m_targetRt = new RenderTexture(viewPortWidth, viewPortHeight, 16, RenderTextureFormat.ARGB32);
            }

            RenderScene();
            image = m_targetRt;
        }

        private void CleanUp()
        {
            if (m_targetRt != null)
            {
                m_targetRt.Release();
                m_targetRt = null;
            }
            if (m_terrainMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(m_terrainMaterial);
            }
        }

        private void RenderScene()
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            GameObject cameraObject = new GameObject() { hideFlags = HideFlags.HideAndDontSave };
            SceneManager.MoveGameObjectToScene(cameraObject, previewScene);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = m_cameraPosition;
            camera.transform.rotation = Quaternion.Euler(m_cameraRotation);
            camera.transform.localScale = Vector3.one;
            camera.nearClipPlane = 1f;
            camera.farClipPlane = 10000;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(56, 56, 56, 255);
            camera.cameraType = CameraType.Preview;
            camera.useOcclusionCulling = false;
            camera.scene = previewScene;
            camera.enabled = false;
            camera.renderingPath = RenderingPath.Forward;
            camera.targetTexture = m_targetRt;
            //camera.cullingMatrix = Matrix4x4.identity; //prevent patch from being culled 

            EditorSettings editorSettings = EditorSettings.Get();
            if (editorSettings.graphEditorSettings.showGrid)
            {
                RenderGridline(camera);
            }

            int visQuality = 5;
            if (editorSettings != null)
            {
                if (editorSettings.graphEditorSettings != null)
                {
                    visQuality = editorSettings.graphEditorSettings.terrainVisualizationQuality;
                }
            }

            RenderTerrain(camera, visQuality);
            if (editorSettings.graphEditorSettings.showWaterLevel)
            {
                RenderWater(camera);
            }

            camera.Render();
            camera.targetTexture = null;
            GameObject.DestroyImmediate(camera.gameObject);
            EditorSceneManager.ClosePreviewScene(previewScene);
        }

        private void RenderTerrain(Camera camera, int gridSize)
        {
            EditorSettings editorSettings = EditorSettings.Get();
            RenderSetup setup = CreateRenderSetup();
            if (setup.heightMap != null)
            {
                m_terrainMaterial.SetTexture(HEIGHT_MAP, setup.heightMap);
            }
            else
            {
                m_terrainMaterial.SetTexture(HEIGHT_MAP, Texture2D.blackTexture);
            }
            if (setup.maskMap != null)
            {
                m_terrainMaterial.SetTexture(MASK_MAP, setup.maskMap);
                m_terrainMaterial.SetTexture(GRADIENT_MAP, editorSettings.graphEditorSettings.GetViewportGradient());
                if (setup.maskMap.format == RenderTextureFormat.RFloat ||
                    setup.maskMap.format == RenderTextureFormat.RHalf ||
                    setup.maskMap.format == RenderTextureFormat.R8)
                {
                    m_terrainMaterial.DisableKeyword(KW_MASK_IS_COLOR);
                }
                else
                {
                    m_terrainMaterial.EnableKeyword(KW_MASK_IS_COLOR);
                }
            }
            else
            {
                m_terrainMaterial.SetTexture(MASK_MAP, Texture2D.whiteTexture);
                m_terrainMaterial.EnableKeyword(KW_MASK_IS_COLOR);
            }

            TerrainGraph graph = m_editor.clonedGraph as TerrainGraph;
            m_terrainMaterial.SetVector(TERRAIN_SIZE, new Vector3(graph.debugConfigs.worldBounds.width, graph.debugConfigs.terrainHeight, graph.debugConfigs.worldBounds.height));

            Vector3 terrainPos = new Vector3(graph.debugConfigs.worldBounds.x, 0, graph.debugConfigs.worldBounds.y);
            m_modelRotation = Vector3.zero;
            Matrix4x4 terrainToWorld = Matrix4x4.TRS(terrainPos, Quaternion.Euler(m_modelRotation), new Vector3(1, 1, 1));
            Vector3 terrainSize = new Vector3(graph.debugConfigs.worldBounds.width, graph.debugConfigs.terrainHeight, graph.debugConfigs.worldBounds.height);
            Vector3 patchSize = terrainSize / gridSize;
            float uvPatchSize = 1.0f / gridSize;

            if (m_terrainPatchTransforms == null)
            {
                m_terrainPatchTransforms = new List<Matrix4x4>();
            }
            if (m_terrainPatchUvRemaps == null)
            {
                m_terrainPatchUvRemaps = new List<Vector4>();
            }
            m_terrainPatchTransforms.Clear();
            m_terrainPatchUvRemaps.Clear();

            MaterialPropertyBlock props = new MaterialPropertyBlock();
            int start = 0;
            int end = gridSize - 1;
            for (int x = start; x <= end; ++x)
            {
                for (int z = start; z <= end; ++z)
                {
                    props.Clear();
                    Vector3 localPos = new Vector3((x + 0.5f) * patchSize.x, 0, (z + 0.5f) * patchSize.z);
                    Quaternion localRotation = Quaternion.identity;
                    Vector3 localScale = new Vector3(patchSize.x, terrainSize.y, patchSize.z);
                    Matrix4x4 patchToTerrainMatrix = Matrix4x4.TRS(localPos, localRotation, localScale);
                    Matrix4x4 trs = terrainToWorld * patchToTerrainMatrix;

                    Vector4 uvRemap = new Vector4((x - start) * uvPatchSize, (z - start) * uvPatchSize, uvPatchSize, uvPatchSize);

                    props.SetVector(UV_REMAP, uvRemap);
                    Graphics.DrawMesh(m_patchMesh, trs, m_terrainMaterial, 0, camera, 0, props, ShadowCastingMode.Off, false, null, LightProbeUsage.Off, null);
                }
            }

            if (setup.positionBuffer != null)
            {
                props.Clear();
                props.SetBuffer(POSITION_SAMPLES, setup.positionBuffer);
                props.SetVector(TERRAIN_SIZE, terrainSize);
                props.SetVector(TERRAIN_POS, terrainPos);
                if (setup.heightMap != null)
                {
                    props.SetTexture(HEIGHT_MAP, setup.heightMap);
                }
                else
                {
                    props.SetTexture(HEIGHT_MAP, Texture2D.blackTexture);
                }
                if (setup.isInstanceBuffer)
                {
                    m_positionVisMaterial.EnableKeyword(KW_DATA_TYPE_INSTANCE_SAMPLE);
                }
                else
                {
                    m_positionVisMaterial.DisableKeyword(KW_DATA_TYPE_INSTANCE_SAMPLE);
                }

                Bounds bounds = new Bounds();
                bounds.center = terrainSize * 0.5f;
                bounds.size = terrainSize;

                int vertexCount = setup.positionBuffer.count * 2 / PositionSample.SIZE;

                Graphics.DrawProcedural(m_positionVisMaterial, bounds, MeshTopology.Lines, vertexCount, 1, camera, props, ShadowCastingMode.Off, false, 0);
            }
        }

        private void RenderWater(Camera camera)
        {
            EditorSettings editorSettings = EditorSettings.Get();
            TerrainGraph graph = m_editor.clonedGraph as TerrainGraph;

            Vector3 terrainPos = new Vector3(graph.debugConfigs.worldBounds.x, 0, graph.debugConfigs.worldBounds.y);
            Vector3 terrainSize = new Vector3(graph.debugConfigs.worldBounds.width, graph.debugConfigs.terrainHeight, graph.debugConfigs.worldBounds.height);
            Matrix4x4 trs = Matrix4x4.TRS(
                new Vector3(terrainPos.x + terrainSize.x * 0.5f, editorSettings.graphEditorSettings.waterLevel, terrainPos.z + terrainSize.z * 0.5f),
                Quaternion.identity,
                new Vector3(terrainSize.x, 1, terrainSize.z));
            Graphics.DrawMesh(m_patchMesh, trs, m_waterMaterial, 0, camera, 0, null, ShadowCastingMode.Off, false, null, LightProbeUsage.Off, null);
        }

        private void RenderGridline(Camera camera)
        {
            Bounds b = new Bounds(Vector3.zero, Vector3.one * 10000);
            MaterialPropertyBlock blocks = new MaterialPropertyBlock();

            //Draw unit grid
            blocks.SetVector(ORIGIN, -Vector4.one * 10000);
            blocks.SetVector(OFFSET, new Vector4(500, 0, 0, 20000));
            Graphics.DrawProcedural(
                m_gridlineMaterial,
                b,
                MeshTopology.Lines,
                82,
                1,
                camera,
                blocks,
                ShadowCastingMode.Off,
                false,
                0);
            blocks.SetVector(OFFSET, new Vector4(0, 500, 20000, 0));
            Graphics.DrawProcedural(
                m_gridlineMaterial,
                b,
                MeshTopology.Lines,
                82,
                1,
                camera,
                blocks,
                ShadowCastingMode.Off,
                false,
                0);

            //Draw y axis
            blocks.SetVector(ORIGIN, new Vector4(0, -10000, 0, 0));
            blocks.SetVector(OFFSET, new Vector4(500, 0, 0, 20000));
            Graphics.DrawProcedural(
                m_axisMaterial,
                b,
                MeshTopology.Lines,
                2,
                1,
                camera,
                blocks,
                ShadowCastingMode.Off,
                false,
                0);

            //draw x axis
            blocks.SetVector(ORIGIN, new Vector4(-10000, 0, 0, 0));
            blocks.SetVector(OFFSET, new Vector4(0, 500, 20000, 0));
            Graphics.DrawProcedural(
                m_axisMaterial,
                b,
                MeshTopology.Lines,
                2,
                1,
                camera,
                blocks,
                ShadowCastingMode.Off,
                false,
                0);
        }

        private void OnDrag(Draggable.DragInfo drag)
        {
            if (drag.button == 0)
                return;
            if (drag.button == 2) //panning
            {
                float speed = m_cameraPosition.magnitude * 0.001f;
                Matrix4x4 transform = Matrix4x4.TRS(m_cameraPosition, Quaternion.Euler(m_cameraRotation), Vector3.one);
                Vector3 direction = transform.MultiplyVector(new Vector3(-drag.delta.x, drag.delta.y, 0));
                m_cameraPosition.x += direction.x * speed;
                m_cameraPosition.y += direction.y * speed;
                m_cameraPosition.z += direction.z * speed;
            }
            else if (drag.isAlt && drag.button == 1) //rotate model
            {
                m_modelRotation.y -= drag.delta.x * 0.1f;
            }
            else if (drag.button == 1) //rotate camera
            {
                m_cameraRotation.x += drag.delta.y * 0.1f;
                m_cameraRotation.y += drag.delta.x * 0.1f;
            }

            m_cameraRotation.x = Mathf.Clamp(m_cameraRotation.x, -89f, 89f);

            RenderViewport();
            MarkDirtyRepaint();
        }

        private void OnScroll(Vector2 delta)
        {
            //move camera forward
            float speed = m_cameraPosition.magnitude * 0.005f;
            Matrix4x4 transform = Matrix4x4.TRS(m_cameraPosition, Quaternion.Euler(m_cameraRotation), Vector3.one);
            Vector3 forward = transform.MultiplyVector(new Vector3(0, 0, 1));
            m_cameraPosition -= forward * delta.y * speed;
            RenderViewport();
            MarkDirtyRepaint();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            CleanUp();
            RenderViewport();
        }

        private RenderSetup CreateRenderSetup()
        {
            EditorSettings editorSettings = EditorSettings.Get();

            RenderSetup setup = new RenderSetup();
            setup.heightMap = null;
            setup.renderMode = RenderSetup.MODE_MASK;
            setup.maskMap = null;
            setup.splatDiffuseMap = new List<RenderTexture>();
            setup.weightMaps = new List<RenderTexture>();
            if (m_editor.m_lastExecution != null)
            {
                INode heightNode = m_editor.clonedGraph.GetNode(m_editor.m_display3dHeightNodeId);
                if (heightNode == null && editorSettings.graphEditorSettings.defaultTextureDisplayMode == EditorSettings.GraphEditorSettings.TextureDisplayMode.Height)
                {
                    heightNode = m_editor.clonedGraph.GetNode(m_editor.m_activeNodeId);
                }

                if (heightNode != null)
                {
                    ISlot[] outputSlot = heightNode.GetOutputSlots();
                    if (outputSlot != null && outputSlot.Length > 0)
                    {
                        SlotRef outputRef = new SlotRef(heightNode.id, outputSlot[0].id);
                        setup.heightMap = m_editor.m_lastExecution.data.GetRT(outputRef);
                    }
                }

                INode maskNode = m_editor.clonedGraph.GetNode(m_editor.m_display3dMaskNodeId);
                if (maskNode == null && editorSettings.graphEditorSettings.defaultTextureDisplayMode == EditorSettings.GraphEditorSettings.TextureDisplayMode.Mask)
                {
                    maskNode = m_editor.clonedGraph.GetNode(m_editor.m_activeNodeId);
                }
                if (maskNode != null)
                {
                    ISlot[] outputSlot = maskNode.GetOutputSlots();
                    if (outputSlot != null && outputSlot.Length > 0)
                    {
                        SlotRef outputRef = new SlotRef(maskNode.id, outputSlot[0].id);
                        setup.maskMap = m_editor.m_lastExecution.data.GetRT(outputRef);
                    }
                }

                ComputeBuffer bf = null;
                INode positionNode = m_editor.clonedGraph.GetNode(m_editor.m_display3dPositionNodeId);
                if (positionNode == null)
                {
                    positionNode = m_editor.clonedGraph.GetNode(m_editor.m_activeNodeId);
                }

                if (positionNode != null)
                {
                    ISlot[] outputSlot = positionNode.GetOutputSlots();
                    if (outputSlot != null && outputSlot.Length > 0)
                    {
                        SlotRef outputRef = new SlotRef(positionNode.id, outputSlot[0].id);
                        GraphBuffer gb = m_editor.m_lastExecution.data.GetBuffer(outputRef);
                        if (gb != null)
                        {
                            bf = gb.buffer;
                        }
                    }
                }

                setup.positionBuffer = bf;
                if (positionNode != null && positionNode is InstanceOutputNodeBase)
                {
                    setup.isInstanceBuffer = true;
                }
                else
                {
                    setup.isInstanceBuffer = false;
                }
            }
            return setup;
        }

        private void OnWasd(WasdTracker.WasdInfo info)
        {
            Vector3 delta = Vector3.zero;
            delta.x -= info.isA ? 1 : 0;
            delta.x += info.isD ? 1 : 0;

            delta.y += info.isE ? 1 : 0;
            delta.y -= info.isQ ? 1 : 0;

            delta.z += info.isW ? 1 : 0;
            delta.z -= info.isS ? 1 : 0;

            float speed = info.isShift ? 5f : 1f;
            Matrix4x4 transform = Matrix4x4.TRS(m_cameraPosition, Quaternion.Euler(m_cameraRotation), Vector3.one);
            Vector3 direction = transform.MultiplyVector(delta);
            m_cameraPosition.x += direction.x * speed;
            m_cameraPosition.y += direction.y * speed;
            m_cameraPosition.z += direction.z * speed;

            RenderViewport();
            MarkDirtyRepaint();
        }

        private void OnGridlineButtonClicked()
        {
            EditorSettings editorSettings = EditorSettings.Get();
            editorSettings.graphEditorSettings.showGrid = !editorSettings.graphEditorSettings.showGrid;
            m_toggleGridButton.SetToggled(editorSettings.graphEditorSettings.showGrid);
            EditorUtility.SetDirty(editorSettings);
            RenderViewport();
            MarkDirtyRepaint();
        }

        private void OnWaterButtonClicked()
        {
            EditorSettings editorSettings = EditorSettings.Get();
            editorSettings.graphEditorSettings.showWaterLevel = !editorSettings.graphEditorSettings.showWaterLevel;
            m_toggleWaterButton.SetToggled(editorSettings.graphEditorSettings.showWaterLevel);
            EditorUtility.SetDirty(editorSettings);
            RenderViewport();
            MarkDirtyRepaint();
        }

        internal void UpdateToggleButtons()
        {
            EditorSettings editorSettings = EditorSettings.Get();
            m_toggleGridButton.SetToggled(editorSettings.graphEditorSettings.showGrid);
            m_toggleWaterButton.SetToggled(editorSettings.graphEditorSettings.showWaterLevel);

            if (editorSettings.graphEditorSettings.defaultTextureDisplayMode == EditorSettings.GraphEditorSettings.TextureDisplayMode.Height)
            {
                m_defaultTextureDisplayModeButton.image = Resources.Load<Texture2D>("Vista/Textures/Display3dHeight");
            }
            else
            {
                m_defaultTextureDisplayModeButton.image = Resources.Load<Texture2D>("Vista/Textures/Display3dMask");
            }
        }

        private void OnResetCameraButtonClicked()
        {
            TerrainGraph graph = m_editor.clonedGraph as TerrainGraph;
            Rect worldBounds = graph.debugConfigs.worldBounds;
            Vector3 terrainPos = new Vector3(worldBounds.center.x, 0, worldBounds.center.y);
            m_cameraPosition = new Vector3(0, graph.debugConfigs.terrainHeight * 1.5f, -worldBound.size.y * 3) + terrainPos;
            m_cameraRotation = new Vector3(30, 0, 0);
            RenderViewport();
            MarkDirtyRepaint();
        }

        private void OnDefaultTextureDisplayModeButtonClicked()
        {
            EditorSettings editorSettings = EditorSettings.Get();
            EditorSettings.GraphEditorSettings.TextureDisplayMode mode = editorSettings.graphEditorSettings.defaultTextureDisplayMode;
            if (mode == EditorSettings.GraphEditorSettings.TextureDisplayMode.Height)
            {
                mode = EditorSettings.GraphEditorSettings.TextureDisplayMode.Mask;
                m_defaultTextureDisplayModeButton.image = Resources.Load<Texture2D>("Vista/Textures/Display3dMask");
            }
            else if (mode == EditorSettings.GraphEditorSettings.TextureDisplayMode.Mask)
            {
                mode = EditorSettings.GraphEditorSettings.TextureDisplayMode.Height;
                m_defaultTextureDisplayModeButton.image = Resources.Load<Texture2D>("Vista/Textures/Display3dHeight");
            }
            editorSettings.graphEditorSettings.defaultTextureDisplayMode = mode;
            EditorUtility.SetDirty(editorSettings);
            RenderViewport();
            MarkDirtyRepaint();
        }

        private void OnTakeScreenshotButtonClicked()
        {
            string folder = "Assets/VistaScreenshots/";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string fileName = $"{m_editor.sourceGraph.name}_3D_{DateTime.Now.Ticks}.png";
            string fullPath = Path.Combine(folder, fileName);
            EditorCoroutineUtility.StartCoroutine(DoTakeScreenshot(fullPath), this);
        }

        private IEnumerator DoTakeScreenshot(string fullPath)
        {
            m_buttonContainer.visible = false;
            yield return null;

            Rect r = this.worldBound;
            r.position += m_editor.position.position;

            Texture2D tex = EditorScreenshot.Capture(r.position, (int)r.width, (int)r.height);
            byte[] data = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, data);
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();

            yield return null;
            m_buttonContainer.visible = true;
            Texture2D assetTex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            EditorGUIUtility.PingObject(assetTex);
            Selection.activeObject = assetTex;
            AssetDatabase.OpenAsset(assetTex);
            Debug.Log($"Screenshot saved at {fullPath}", assetTex);
        }
    }
}
#endif
