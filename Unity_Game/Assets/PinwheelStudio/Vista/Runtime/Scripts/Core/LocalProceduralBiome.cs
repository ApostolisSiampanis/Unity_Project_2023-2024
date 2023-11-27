#if VISTA
using Pinwheel.Vista.Geometric;
using Pinwheel.Vista.Graph;
using Pinwheel.Vista.Graphics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinwheel.Vista.ExposeProperty;

namespace Pinwheel.Vista
{
    [ExecuteInEditMode]
    [AddComponentMenu("Vista/Local Procedural Biome")]
    [HelpURL("https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.4rhryfmtelm8")]
    public class LocalProceduralBiome : MonoBehaviour, IProceduralBiome, ISerializationCallbackReceiver
    {
        protected static HashSet<LocalProceduralBiome> s_allInstances = new HashSet<LocalProceduralBiome>();
        public static IEnumerable<LocalProceduralBiome> allInstances
        {
            get
            {
                return s_allInstances;
            }
        }

        internal delegate TerrainGraph CloneAndOverrideGraphHandler(TerrainGraph src, IEnumerable<PropertyOverride> overrides);
        internal static event CloneAndOverrideGraphHandler cloneAndOverrideGraphCallback;

        [SerializeField]
        protected int m_order;
        public int order
        {
            get
            {
                return m_order;
            }
            set
            {
                m_order = value;
            }
        }

        [SerializeField]
        protected TerrainGraph m_terrainGraph;
        public TerrainGraph terrainGraph
        {
            get
            {
                return m_terrainGraph;
            }
            set
            {
                m_terrainGraph = value;
            }
        }

        [SerializeField]
        protected Space m_space;
        public Space space
        {
            get
            {
                return m_space;
            }
            set
            {
                m_space = value;
            }
        }

        [SerializeField]
        protected BiomeDataMask m_dataMask;
        public BiomeDataMask dataMask
        {
            get
            {
                return m_dataMask;
            }
            set
            {
                m_dataMask = value;
            }
        }

        [SerializeField]
        protected int m_baseResolution;
        public int baseResolution
        {
            get
            {
                return m_baseResolution;
            }
            set
            {
                m_baseResolution = Utilities.MultipleOf8(Mathf.Clamp(value, Constants.RES_MIN, Constants.RES_MAX));
            }
        }

        [SerializeField]
        protected int m_seed;
        public int seed
        {
            get
            {
                return m_seed;
            }
            set
            {
                m_seed = value;
            }
        }

        [SerializeField]
        protected bool m_shouldCollectSceneHeight;
        public bool shouldCollectSceneHeight
        {
            get
            {
                return m_shouldCollectSceneHeight;
            }
            set
            {
                m_shouldCollectSceneHeight = value;
            }
        }

        [SerializeField]
        protected int m_biomeMaskResolution;
        public int biomeMaskResolution
        {
            get
            {
                return m_biomeMaskResolution;
            }
            set
            {
                int oldRes = m_biomeMaskResolution;
                int newRes = Utilities.MultipleOf8(Mathf.Clamp(value, Constants.RES_MIN, Constants.RES_MAX));
                if (oldRes != newRes)
                {
                    m_biomeMaskResolution = newRes;
                    if (m_biomeMaskAdjustments != null && m_biomeMaskAdjustments.Length > 0)
                    {
                        m_biomeMaskAdjustments = Utilities.ResampleBilinear(m_biomeMaskAdjustments, oldRes, oldRes, newRes, newRes);
                    }
                }
            }
        }

        [SerializeField]
        protected BiomeMaskGraph m_biomeMaskGraph;
        public BiomeMaskGraph biomeMaskGraph
        {
            get
            {
                return m_biomeMaskGraph;
            }
            set
            {
                m_biomeMaskGraph = value;
            }
        }

        public Bounds worldBounds
        {
            get
            {
                return CalculateWorldBounds();
            }
        }

        protected long m_updateCounter;
        public long updateCounter
        {
            get
            {
                return m_updateCounter;
            }
            set
            {
                m_updateCounter = value;
            }
        }

        [SerializeField]
        protected Vector3[] m_anchors;
        public Vector3[] anchors
        {
            get
            {
                Vector3[] clonedAnchors = new Vector3[m_anchors.Length];
                m_anchors.CopyTo(clonedAnchors, 0);
                return clonedAnchors;
            }
            set
            {
                if (value == null)
                {
                    m_anchors = new Vector3[0];
                }
                else
                {
                    m_anchors = new Vector3[value.Length];
                    value.CopyTo(m_anchors, 0);
                }
                RecalculateFalloffAnchors();
            }
        }

        [SerializeField]
        protected FalloffDirection m_falloffDirection;
        public FalloffDirection falloffDirection
        {
            get
            {
                return m_falloffDirection;
            }
            set
            {
                FalloffDirection oldValue = m_falloffDirection;
                FalloffDirection newValue = value;
                m_falloffDirection = newValue;
                if (oldValue != newValue)
                {
                    RecalculateFalloffAnchors();
                }
            }
        }

        [SerializeField]
        protected float m_falloffDistance;
        public float falloffDistance
        {
            get
            {
                return m_falloffDistance;
            }
            set
            {
                float oldValue = m_falloffDistance;
                float newValue = Mathf.Max(0, value);
                m_falloffDistance = newValue;
                if (oldValue != newValue)
                {
                    RecalculateFalloffAnchors();
                }
            }
        }

        [SerializeField]
        protected Vector3[] m_falloffAnchors;
        public Vector3[] falloffAnchors
        {
            get
            {
                if (m_falloffAnchors == null || m_falloffAnchors.Length != m_anchors.Length)
                {
                    RecalculateFalloffAnchors();
                }
                Vector3[] clonedFalloffAnchors = new Vector3[m_falloffAnchors.Length];
                m_falloffAnchors.CopyTo(clonedFalloffAnchors, 0);
                return clonedFalloffAnchors;
            }
        }

        internal BiomeData cachedData { get; set; }

        [System.Serializable]
        public enum CleanUpMode
        {
            EachIteration, Manually
        }

        [SerializeField]
        protected CleanUpMode m_cleanUpMode;
        public CleanUpMode cleanUpMode
        {
            get
            {
                return m_cleanUpMode;
            }
            set
            {
                m_cleanUpMode = value;
            }
        }

        [SerializeField]
        protected float[] m_biomeMaskAdjustments;
        public float[] biomeMaskAdjustments
        {
            get
            {
                if (m_biomeMaskAdjustments.Length != m_biomeMaskResolution * m_biomeMaskResolution)
                {
                    m_biomeMaskAdjustments = new float[0];
                }

                float[] clonedData = new float[m_biomeMaskAdjustments.Length];
                m_biomeMaskAdjustments.CopyTo(clonedData, 0);
                return clonedData;
            }
            set
            {
                if (value == null)
                {
                    m_biomeMaskAdjustments = new float[0];
                }
                else if (value.Length != m_biomeMaskResolution * m_biomeMaskResolution)
                {
                    throw new System.ArgumentException("Wrong data dimension. Biome mask adjustment array length must be biomeMaskResolution^2");
                }
                else
                {
                    m_biomeMaskAdjustments = new float[value.Length];
                    value.CopyTo(m_biomeMaskAdjustments, 0);
                }
            }
        }

        [SerializeField]
        protected BiomeBlendOptions m_blendOptions;
        public BiomeBlendOptions blendOptions
        {
            get
            {
                return m_blendOptions;
            }
            set
            {
                m_blendOptions = value;
            }
        }

        [SerializeField]
        protected TextureInput[] m_textureInputs = new TextureInput[0];
        public TextureInput[] textureInputs
        {
            get
            {
                TextureInput[] clonedInputs = new TextureInput[m_textureInputs.Length];
                m_textureInputs.CopyTo(clonedInputs, 0);
                return clonedInputs;
            }
            set
            {
                if (value == null)
                {
                    m_textureInputs = new TextureInput[0];
                }
                else
                {
                    m_textureInputs = new TextureInput[value.Length];
                    value.CopyTo(m_textureInputs, 0);
                }
            }
        }

        [SerializeField]
        protected PositionInput[] m_positionInputs = new PositionInput[0];
        public PositionInput[] positionInputs
        {
            get
            {
                PositionInput[] clonedInputs = new PositionInput[m_positionInputs.Length];
                m_positionInputs.CopyTo(clonedInputs, 0);
                return clonedInputs;
            }
            set
            {
                if (value == null)
                {
                    m_positionInputs = new PositionInput[0];
                }
                else
                {
                    m_positionInputs = new PositionInput[value.Length];
                    value.CopyTo(m_positionInputs, 0);
                }
            }
        }

        [SerializeField]
        internal string m_guid = System.Guid.NewGuid().ToString();

        [SerializeField]
        internal PropertyOverride[] m_propertyOverrides = new PropertyOverride[0];
        public PropertyOverride[] propertyOverrides
        {
            get
            {
                PropertyOverride[] clonedInputs = new PropertyOverride[m_propertyOverrides.Length];
                m_propertyOverrides.CopyTo(clonedInputs, 0);
                return clonedInputs;
            }
            set
            {
                if (value == null)
                {
                    m_propertyOverrides = new PropertyOverride[0];
                }
                else
                {
                    m_propertyOverrides = new PropertyOverride[value.Length];
                    value.CopyTo(m_propertyOverrides, 0);
                }
            }
        }

        internal bool isGeneratingCacheData { get; private set; }

        public void Reset()
        {
            m_order = 0;
            m_terrainGraph = null;
            m_space = Space.World;
            m_dataMask = (BiomeDataMask)(~0);
            m_baseResolution = 1024;
            m_seed = 0;
            m_shouldCollectSceneHeight = false;

            m_biomeMaskResolution = 512;
            m_biomeMaskGraph = null;
            m_falloffDirection = FalloffDirection.Outer;
            m_falloffDistance = 100;
            m_anchors = new Vector3[]
            {
                new Vector3(-500, 0, -500), new Vector3(-500, 0, 500), new Vector3(500, 0, 500), new Vector3(500, 0, -500)
            };
            RecalculateFalloffAnchors();

            m_cleanUpMode = CleanUpMode.EachIteration;
            m_biomeMaskAdjustments = new float[0];

            m_blendOptions = BiomeBlendOptions.Default();

            m_textureInputs = new TextureInput[0];
            m_positionInputs = new PositionInput[0];
        }

        protected void OnEnable()
        {
            s_allInstances.Add(this);
            GraphAsset.graphChanged += OnGraphChanged;
        }

        protected void OnDisable()
        {
            s_allInstances.Remove(this);
            GraphAsset.graphChanged -= OnGraphChanged;
            CleanUp();
        }

        protected void OnGraphChanged(GraphAsset graph)
        {
            if (graph != m_terrainGraph)
                return;
            CleanUp();
            this.MarkChanged();
            this.GenerateBiomesInGroup();
        }

        public static LocalProceduralBiome CreateInstanceInScene(VistaManager manager)
        {
            GameObject biomeGO = new GameObject("Local Procedural Biome");
            LocalProceduralBiome biome = biomeGO.AddComponent<LocalProceduralBiome>();

            if (manager != null)
            {
                biome.transform.parent = manager.transform;
                biome.transform.localPosition = Vector3.zero;
                biome.transform.localRotation = Quaternion.identity;
                biome.transform.localScale = Vector3.one;
            }

            return biome;
        }

        public BiomeDataRequest RequestData(Bounds worldBounds, int heightMapResolution, int textureResolution)
        {
            BiomeDataRequest request = new BiomeDataRequest();
            BiomeData data = new BiomeData();
            request.data = data;
            if (m_terrainGraph != null)
            {
                CoroutineUtility.StartCoroutine(RequestDataProgressive(request, worldBounds, heightMapResolution, textureResolution));
                return request;
            }
            else
            {
                request.Complete();
                return request;
            }
        }

        private IEnumerator RequestDataProgressive(BiomeDataRequest request, Bounds worldBounds, int heightMapResolution, int textureResolution)
        {
            Bounds biomeWorldBoundsInt = this.worldBounds;
            Vector3 boundsCenter = biomeWorldBoundsInt.center;
            boundsCenter.x = Mathf.Round(boundsCenter.x);
            boundsCenter.z = Mathf.Round(boundsCenter.z);
            Vector3 boundsSize = biomeWorldBoundsInt.size;
            boundsSize.x = Mathf.Round(boundsSize.x);
            boundsSize.y = worldBounds.size.y;
            boundsSize.z = Mathf.Round(boundsSize.z);

            biomeWorldBoundsInt.center = boundsCenter;
            biomeWorldBoundsInt.size = boundsSize;

            //If it is a fresh generation(no cache), then generate cache data in the biome self bounds
            if (cachedData == null)
            {
                isGeneratingCacheData = true;

                BiomeDataRequest cacheDataRequest = new BiomeDataRequest();
                BiomeData cache = new BiomeData();
                cacheDataRequest.data = cache;

                GraphInputContainer inputContainer = new GraphInputContainer();
                LPBInputProvider inputProvider = new LPBInputProvider(this);
                inputProvider.SetInput(inputContainer);

                TerrainGraph graphToExecute;
                if (m_terrainGraph.HasExposedProperties && cloneAndOverrideGraphCallback != null)
                {
                    graphToExecute = cloneAndOverrideGraphCallback.Invoke(terrainGraph, m_propertyOverrides);
                }
                else
                {
                    //graphToExecute = Instantiate<TerrainGraph>(m_terrainGraph);
                    graphToExecute = m_terrainGraph;
                }

                CoroutineUtility.StartCoroutine(TerrainGraphUtilities.RequestBiomeData(this, cacheDataRequest, graphToExecute, biomeWorldBoundsInt, space, m_baseResolution, m_seed, inputContainer, m_dataMask, inputProvider.FillTerrainGraphArguments));
                yield return cacheDataRequest;

                RenderTexture combinedBiomeMask = inputProvider.RemoveTexture(GraphConstants.BIOME_MASK_INPUT_NAME);

                if (m_biomeMaskGraph == null)
                {
                    cacheDataRequest.data.biomeMaskMap = combinedBiomeMask;
                }
                else
                {
                    BiomeDataRequest biomeMaskPostProcessRequest = new BiomeDataRequest();
                    BiomeData biomeMaskPostProcessData = new BiomeData();
                    biomeMaskPostProcessRequest.data = biomeMaskPostProcessData;
                    CoroutineUtility.StartCoroutine(BiomeMaskGraphUtilities.RequestData(biomeMaskPostProcessRequest, m_biomeMaskGraph, biomeWorldBoundsInt, space, combinedBiomeMask));
                    yield return biomeMaskPostProcessRequest;
                    cacheDataRequest.data.biomeMaskMap = biomeMaskPostProcessData.biomeMaskMap;

                    biomeMaskPostProcessData.biomeMaskMap = null;
                    biomeMaskPostProcessData.Dispose();
                    combinedBiomeMask.Release();
                    Object.DestroyImmediate(combinedBiomeMask);
                }
                cachedData = cacheDataRequest.data;

                if (graphToExecute != m_terrainGraph)
                {
                    Object.DestroyImmediate(graphToExecute);
                }
                inputProvider.CleanUp();
                isGeneratingCacheData = false;
            }

            //Copy cache data from the biome self bounds to the target world bounds
            BiomeDataUtilities.Copy(cachedData, biomeWorldBoundsInt, request.data, worldBounds, heightMapResolution, textureResolution);
            request.Complete();
            yield break;
        }

        public bool IsOverlap(Bounds area)
        {
            Vector2[] biomeVertices = new Vector2[falloffAnchors.Length];
            for (int i = 0; i < biomeVertices.Length; ++i)
            {
                biomeVertices[i] = transform.TransformPoint(falloffAnchors[i]).XZ();
            }
            Polygon2D biomePolygon = new Polygon2D(biomeVertices);

            Vector2[] areaVertices = new Vector2[4];
            areaVertices[0] = new Vector2(area.min.x, area.min.z);
            areaVertices[1] = new Vector2(area.min.x, area.max.z);
            areaVertices[2] = new Vector2(area.max.x, area.max.z);
            areaVertices[3] = new Vector2(area.max.x, area.min.z);
            Polygon2D areaPolygon = new Polygon2D(areaVertices);

            return Polygon2D.IsOverlap(biomePolygon, areaPolygon);
        }

        public void RecalculateFalloffAnchors()
        {
            if (m_anchors == null)
            {
                m_falloffAnchors = null;
            }
            else
            {
                m_falloffAnchors = AnchorUtilities.GetFalloff(m_anchors, m_falloffDistance, m_falloffDirection);
            }
        }

        protected Bounds CalculateWorldBounds()
        {
            Bounds worldBounds;
            Vector3[] outerAnchors = m_falloffDirection == FalloffDirection.Outer ? m_falloffAnchors : m_anchors;
            if (outerAnchors == null || outerAnchors.Length == 0)
            {
                worldBounds = new Bounds(Vector3.zero, Vector3.zero);
            }
            else
            {
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float minZ = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;
                float maxZ = float.MinValue;
                foreach (Vector3 a in outerAnchors)
                {
                    Vector3 worldPos = transform.TransformPoint(a);
                    minX = Mathf.Min(minX, worldPos.x);
                    minY = Mathf.Min(minY, worldPos.y);
                    minZ = Mathf.Min(minZ, worldPos.z);

                    maxX = Mathf.Max(maxX, worldPos.x);
                    maxY = Mathf.Max(maxY, worldPos.y);
                    maxZ = Mathf.Max(maxZ, worldPos.z);
                }
                Vector3 center = new Vector3(minX + maxX, minY + maxY, minZ + maxZ) * 0.5f;
                Vector3 size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);

                worldBounds = new Bounds(center, size);
            }

            return worldBounds;
        }

        public void CleanUp()
        {
            if (cachedData != null)
            {
                cachedData.Dispose();
                cachedData = null;
            }
        }

        public void OnBeforeVMGenerate()
        {

        }

        public void OnAfterVMGenerate()
        {
            if (m_cleanUpMode == CleanUpMode.EachIteration)
            {
                CleanUp();
            }
        }

        internal RenderTexture RenderBaseBiomeMask()
        {
            Bounds b = worldBounds;
            Vector2[] vertices = new Vector2[m_anchors.Length];
            for (int i = 0; i < vertices.Length; ++i)
            {
                Vector3 worldPoint = transform.TransformPoint(m_anchors[i]);
                Vector2 v = new Vector2
                (
                    (worldPoint.x - b.min.x) / (b.max.x - b.min.x),
                    (worldPoint.z - b.min.z) / (b.max.z - b.min.z)
                );
                vertices[i] = v;
            }

            Vector2[] falloffVertices = new Vector2[m_falloffAnchors.Length];
            for (int i = 0; i < falloffVertices.Length; ++i)
            {
                Vector3 worldPoint = transform.TransformPoint(m_falloffAnchors[i]);
                Vector2 v = new Vector2
                (
                    (worldPoint.x - b.min.x) / (b.max.x - b.min.x),
                    (worldPoint.z - b.min.z) / (b.max.z - b.min.z)
                );
                falloffVertices[i] = v;
            }

            PolygonMaskRenderer.Configs maskRendererConfigs = new PolygonMaskRenderer.Configs();
            maskRendererConfigs.vertices = m_falloffDirection == FalloffDirection.Outer ? vertices : falloffVertices;
            maskRendererConfigs.falloffVertices = m_falloffDirection == FalloffDirection.Outer ? falloffVertices : vertices;
            maskRendererConfigs.falloffTexture = null;

            RenderTexture biomeMask = new RenderTexture(m_biomeMaskResolution, m_biomeMaskResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            biomeMask.name = $"{gameObject.name} - Biome Mask";
            biomeMask.wrapMode = TextureWrapMode.Clamp;
            biomeMask.filterMode = FilterMode.Bilinear;
            biomeMask.enableRandomWrite = true;
            biomeMask.antiAliasing = 1;
            biomeMask.Create();
            PolygonMaskRenderer.Render(biomeMask, maskRendererConfigs);

            return biomeMask;
        }

        internal RenderTexture RenderCombinedBiomeMask()
        {
            RenderTexture baseMask = RenderBaseBiomeMask();
            if (m_biomeMaskAdjustments != null && m_biomeMaskAdjustments.Length > 0)
            {
                Texture2D adjustmentTex = Utilities.TextureFromFloats(m_biomeMaskAdjustments, m_biomeMaskResolution, m_biomeMaskResolution);
                LPBUtilities.CombineBiomeMask(baseMask, adjustmentTex);
                Object.DestroyImmediate(adjustmentTex);
            }
            return baseMask;
        }

        public virtual RenderTexture RenderSceneHeightMap()
        {
            RenderTexture sceneHeightMap = new RenderTexture(m_baseResolution, m_baseResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            sceneHeightMap.name = $"{gameObject.name} - Scene Height Map";
            sceneHeightMap.wrapMode = TextureWrapMode.Clamp;
            sceneHeightMap.filterMode = FilterMode.Bilinear;
            sceneHeightMap.enableRandomWrite = true;
            sceneHeightMap.antiAliasing = 1;
            sceneHeightMap.Create();

            VistaManager vm = this.GetVistaManagerInstance();
            if (vm != null)
            {
                vm.CollectSceneHeight(sceneHeightMap, worldBounds);
            }

            return sceneHeightMap;
        }

        public void OnBeforeSerialize()
        {
            if (string.IsNullOrEmpty(m_guid))
            {
                m_guid = System.Guid.NewGuid().ToString();
            }
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
#endif
