#if VISTA
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Pinwheel.Vista
{
    [AddComponentMenu("Vista/Vista Manager")]
    [HelpURL("https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.ssforp6xnxtf")]
    [ExecuteInEditMode]
    public class VistaManager : MonoBehaviour
    {
        protected static HashSet<VistaManager> s_allInstances = new HashSet<VistaManager>();
        public static IEnumerable<VistaManager> allInstances
        {
            get
            {
                return s_allInstances;
            }
        }

        public delegate void CollectTilesHandler(VistaManager sender, Collector<ITile> tiles);
        public static event CollectTilesHandler collectTiles;

        internal delegate void CollectBiomesHandler(VistaManager sender, Collector<IBiome> biomes);
        internal static CollectBiomesHandler collectFreeBiomes;

        public delegate void GeneratePipelineHandler(VistaManager sender);
        public static event GeneratePipelineHandler beforeGenerating;
        public static event GeneratePipelineHandler afterGenerating;

        [SerializeField]
        private UnityEvent m_beforeGeneratingUnityCallback;
        public UnityEvent beforeGeneratingUnityCallback
        {
            get
            {
                return m_beforeGeneratingUnityCallback;
            }
        }

        [SerializeField]
        private UnityEvent m_afterGeneratingUnityCallback;
        public UnityEvent afterGeneratingUnityCallback
        {
            get
            {
                return m_afterGeneratingUnityCallback;
            }
        }

        public delegate void TexturePopulatedHandler(VistaManager sender, ITile tile, RenderTexture texture);
        public static event TexturePopulatedHandler heightMapPopulated;
        public static event TexturePopulatedHandler holeMapPopulated;
        public static event TexturePopulatedHandler meshDensityMapPopulated;
        public static event TexturePopulatedHandler albedoMapPopulated;
        public static event TexturePopulatedHandler metallicMapPopulated;

        public delegate void LayerWeightPopulatedHandler(VistaManager sender, ITile tile, List<TerrainLayer> layers, List<RenderTexture> weights);
        public static event LayerWeightPopulatedHandler layerWeightPopulated;

        public delegate void TreePopulatedHandler(VistaManager sender, ITile tile, List<TreeTemplate> treeTemplates, List<ComputeBuffer> treeBuffers);
        public static event TreePopulatedHandler treePopulated;

        public delegate void DetailDensityPopulatedHandler(VistaManager sender, ITile tile, List<DetailTemplate> detailTemplates, List<RenderTexture> densityMaps);
        public static event DetailDensityPopulatedHandler detailDensityPopulated;

        public delegate void DetailInstancePopulatedHandler(VistaManager sender, ITile tile, List<DetailTemplate> detailTemplates, List<ComputeBuffer> detailBuffers);
        public static event DetailInstancePopulatedHandler detailInstancePopulated;

        public delegate void ObjectPopulatedHandler(VistaManager sender, ITile tile, List<ObjectTemplate> objectTemplates, List<ComputeBuffer> objectBuffers);
        public static event ObjectPopulatedHandler objectPopulated;

        public delegate void GenericTexturePopulatedHandler(VistaManager sender, ITile tile, List<string> labels, List<RenderTexture> textures);
        public static event GenericTexturePopulatedHandler genericTexturesPopulated;

        public delegate void GenericBufferPopulatedHandler(VistaManager sender, ITile tile, List<string> labels, List<ComputeBuffer> buffers);
        public static event GenericBufferPopulatedHandler genericBuffersPopulated;

        internal static event Func<VistaManager, IBiome[]> getBiomesCallback;

        internal delegate BiomeData BiomeDataBlendHandler(List<BiomeData> srcDatas, List<BiomeBlendOptions> blendOptions);
        internal static event BiomeDataBlendHandler blendBiomeDataCallback;

        internal event Action drawGizmosSelectedCallback;

        protected static List<ITerrainSystem> s_terrainSystems;
        public ITile currentlyProcessingTile { get; protected set; }

        protected static ProgressiveTask s_activeGenerateTask;

        public struct ObjectPopulateArgs
        {
            public int objectsPerFrame { get; set; }
        }

        [SerializeField]
        protected string m_id;
        public string id
        {
            get
            {
                return m_id;
            }
        }

        [SerializeField]
        protected float m_terrainMaxHeight;
        public float terrainMaxHeight
        {
            get
            {
                return m_terrainMaxHeight;
            }
            set
            {
                m_terrainMaxHeight = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected int m_heightMapResolution;
        public int heightMapResolution
        {
            get
            {
                return m_heightMapResolution;
            }
            set
            {
                m_heightMapResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value) + 1, Constants.HM_RES_MIN, Constants.HM_RES_MAX);
            }
        }

        [SerializeField]
        protected int m_textureResolution;
        public int textureResolution
        {
            get
            {
                return m_textureResolution;
            }
            set
            {
                m_textureResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), Constants.HM_RES_MIN, Constants.HM_RES_MAX);
            }
        }

        [SerializeField]
        protected int m_detailDensityMapResolution;
        public int detailDensityMapResolution
        {
            get
            {
                return m_detailDensityMapResolution;
            }
            set
            {
                m_detailDensityMapResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), Constants.RES_MIN, Constants.RES_MAX);
            }
        }

        [SerializeField]
        protected bool m_shouldCullBiomes;
        public bool shouldCullBiomes
        {
            get
            {
                return m_shouldCullBiomes;
            }
            set
            {
                m_shouldCullBiomes = value;
            }
        }

        [SerializeField]
        protected int m_objectToSpawnPerFrame;
        public int objectToSpawnPerFrame
        {
            get
            {
                return m_objectToSpawnPerFrame;
            }
            set
            {
                m_objectToSpawnPerFrame = Mathf.Max(1, value);
            }
        }

        protected long m_updateCounter = 0;

        public static void RegisterTerrainSystem<T>() where T : class, ITerrainSystem, new()
        {
            if (s_terrainSystems == null)
            {
                s_terrainSystems = new List<ITerrainSystem>();
            }
            if (s_terrainSystems.Exists(s => s.GetType().Equals(typeof(T))))
            {
                throw new ArgumentException($"Terrain System {typeof(T).Name} is already registered.");
            }
            s_terrainSystems.Add(new T());
        }

        public static void UnregisterTerrainSystem<T>() where T : class, ITerrainSystem, new()
        {
            if (s_terrainSystems == null)
            {
                s_terrainSystems = new List<ITerrainSystem>();
            }
            s_terrainSystems.RemoveAll(s => s.GetType().Equals(typeof(T)));
        }

        public static IEnumerable<ITerrainSystem> GetTerrainSystems()
        {
            if (s_terrainSystems == null)
                s_terrainSystems = new List<ITerrainSystem>();
            return s_terrainSystems;
        }

        public static ITerrainSystem GetTerrainSystem<T>() where T : ITerrainSystem
        {
            if (s_terrainSystems == null)
            {
                s_terrainSystems = new List<ITerrainSystem>();
            }
            ITerrainSystem system = s_terrainSystems.Find(s => s.GetType().Equals(typeof(T)));
            return system;
        }

        public static VistaManager CreateInstanceInScene()
        {
            GameObject managerGO = new GameObject("VistaManager");
            VistaManager manager = managerGO.AddComponent<VistaManager>();
            return manager;
        }

        public void Reset()
        {
            m_id = Guid.NewGuid().ToString();
            m_terrainMaxHeight = 500;
            m_heightMapResolution = 513;
            m_textureResolution = 512;
            m_detailDensityMapResolution = 1024;
            m_shouldCullBiomes = true;
            m_objectToSpawnPerFrame = 20;
        }

        protected void OnEnable()
        {
            s_allInstances.Add(this);
        }

        protected void OnDisable()
        {
            s_allInstances.Remove(this);
        }

        public IBiome[] GetBiomes()
        {
            if (getBiomesCallback != null)
            {
                return getBiomesCallback.Invoke(this);
            }
            else
            {
                return new IBiome[] { GetComponentInChildren<IBiome>() };
            }
        }

        public List<ITile> GetTiles()
        {
            Collector<ITile> collector = new Collector<ITile>();
            if (collectTiles != null)
            {
                collectTiles.Invoke(this, collector);
            }
            return collector.ToList();
        }

        public ITile[] GetTileArray()
        {
            Collector<ITile> collector = new Collector<ITile>();
            if (collectTiles != null)
            {
                collectTiles.Invoke(this, collector);
            }
            return collector.ToArray();
        }

        protected static void SetActiveTask(ProgressiveTask task)
        {
            if (s_activeGenerateTask != null && task != null && s_activeGenerateTask != task)
            {
                //Debug.LogWarning("VISTA: There is other task running. Having multiple tasks at the same time may lead to performance drop.");
            }
            s_activeGenerateTask = task;
        }

        public static bool HasActiveTask()
        {
            return s_activeGenerateTask != null;
        }

        public static void CancelActiveGenerateTask()
        {
            if (s_activeGenerateTask != null)
            {
                s_activeGenerateTask.Complete();
                SetActiveTask(null);
            }
        }

        public void GenerateAllAndForget()
        {
            ITile[] tiles = GetTileArray();
            Generate(tiles);
        }

        public ProgressiveTask GenerateAll()
        {
            ITile[] tiles = GetTileArray();
            return Generate(tiles);
        }

        public ProgressiveTask Generate(ITile tile)
        {
            return Generate(new ITile[] { tile });
        }

        public ProgressiveTask Generate(IEnumerable<ITile> tiles)
        {
            if (HasActiveTask())
                throw new Exception("Vista is processing another task, please wait for a few seconds and try again.");

            ProgressiveTask taskHandle = new ProgressiveTask();
            SetActiveTask(taskHandle);

            IBiome[] biomes = GetBiomes();
            if (biomes.Length == 0)
            {
                m_updateCounter = DateTime.Now.Ticks;
                taskHandle.Complete();
                return taskHandle;
            }
            else
            {
                List<ITile> overlappedTiles = new List<ITile>();
                HashSet<KeyValuePair<ITile, IBiome>> overlapTests = new HashSet<KeyValuePair<ITile, IBiome>>();
                foreach (ITile t in tiles)
                {
                    if (!t.OverlapTest(biomes, overlapTests))
                        continue;
                    overlappedTiles.Add(t);
                }

                CoroutineUtility.StartCoroutine(ProcessBiomesProgressive(taskHandle, biomes, overlappedTiles, overlapTests));
                return taskHandle;
            }
        }

        private IEnumerator ProcessBiomesProgressive(ProgressiveTask taskHandle, IEnumerable<IBiome> biomes, IEnumerable<ITile> overlappedTiles, ICollection<KeyValuePair<ITile, IBiome>> overlapTests)
        {
#if UNITY_EDITOR
            int editorProgressId = Progress.Start("VistaManager.Generate()");
            Progress.Report(editorProgressId, 0);

            int currentTileIndex = 0;
            int tileCount = overlappedTiles.Count();
#endif

            beforeGeneratingUnityCallback.Invoke();
            beforeGenerating?.Invoke(this);

            foreach (IBiome b in biomes)
            {
                b.OnBeforeVMGenerate();
            }

            foreach (ITile t in overlappedTiles)
            {
                t.maxHeight = terrainMaxHeight;
                t.heightMapResolution = heightMapResolution;
                t.textureResolution = textureResolution;
                t.detailDensityMapResolution = detailDensityMapResolution;
            }

            ObjectPopulateArgs objectPopulateArgs = new ObjectPopulateArgs();
            objectPopulateArgs.objectsPerFrame = m_objectToSpawnPerFrame;

            foreach (ITile t in overlappedTiles)
            {
#if UNITY_EDITOR
                Progress.Report(editorProgressId, currentTileIndex, tileCount, "Processing tiles");
                currentTileIndex += 1;
#endif
                currentlyProcessingTile = t;

                List<BiomeDataRequest> requests = new List<BiomeDataRequest>();
                List<BiomeBlendOptions> blendOptions = new List<BiomeBlendOptions>();
                foreach (IBiome b in biomes)
                {
                    if (m_shouldCullBiomes && !overlapTests.Contains(new KeyValuePair<ITile, IBiome>(t, b)))
                    {
                        continue;
                    }

                    BiomeDataRequest r = b.RequestData(t.worldBounds, heightMapResolution, textureResolution);
                    requests.Add(r);
                    blendOptions.Add(b.blendOptions);
                    yield return r;
                }

                List<BiomeData> biomeDatas = new List<BiomeData>();
                foreach (BiomeDataRequest r in requests)
                {
                    biomeDatas.Add(r.data);
                }

                BiomeData data = null;
                if (biomeDatas.Count == 1)
                {
                    data = biomeDatas[0];
                    for (int i = 1; i < biomeDatas.Count; ++i)
                    {
                        biomeDatas[i].Dispose();
                    }
                }
                else
                {
                    data = blendBiomeDataCallback.Invoke(biomeDatas, blendOptions);
                    foreach (BiomeData d in biomeDatas)
                    {
                        d.Dispose();
                    }
                }

                yield return null;
                if (taskHandle.isCompleted) yield break;

                t.OnBeforeApplyingData();
                if (t is IGeometryPopulator gp)
                {
                    if (data.heightMap != null)
                    {
                        gp.PopulateHeightMap(data.heightMap);
                        heightMapPopulated?.Invoke(this, t, data.heightMap);
                    }
                    if (data.holeMap != null)
                    {
                        gp.PopulateHoleMap(data.holeMap);
                        holeMapPopulated?.Invoke(this, t, data.holeMap);
                    }
                    if (data.meshDensityMap != null)
                    {
                        gp.PopulateMeshDensityMap(data.meshDensityMap);
                        meshDensityMapPopulated?.Invoke(this, t, data.meshDensityMap);
                    }
                    gp.UpdateGeometry();
                }
                if (t is IAlbedoMapPopulator amp && data.albedoMap != null)
                {
                    amp.PopulateAlbedoMap(data.albedoMap);
                    albedoMapPopulated?.Invoke(this, t, data.albedoMap);
                }
                if (t is IMetallicMapPopulator mmp && data.metallicMap != null)
                {
                    mmp.PopulateMetallicMap(data.metallicMap);
                    metallicMapPopulated?.Invoke(this, t, data.metallicMap);
                }

                if (t is ILayerWeightsPopulator lwp)
                {
                    List<TerrainLayer> layers = new List<TerrainLayer>();
                    List<RenderTexture> layerWeights = new List<RenderTexture>();
                    data.GetLayerWeights(layers, layerWeights);
                    lwp.PopulateLayerWeights(layers, layerWeights);
                    layerWeightPopulated?.Invoke(this, t, layers, layerWeights);
                }
                yield return null;
                if (taskHandle.isCompleted) yield break;

                if (t is ITreePopulator tp)
                {
                    List<TreeTemplate> treeTemplates = new List<TreeTemplate>();
                    List<ComputeBuffer> treeBuffers = new List<ComputeBuffer>();
                    data.GetTrees(treeTemplates, treeBuffers);
                    tp.PopulateTrees(treeTemplates, treeBuffers);
                    treePopulated?.Invoke(this, t, treeTemplates, treeBuffers);
                }
                yield return null;
                if (taskHandle.isCompleted) yield break;

                if (t is IDetailDensityPopulator ddp)
                {
                    List<DetailTemplate> detailTemplates = new List<DetailTemplate>();
                    List<RenderTexture> densityMaps = new List<RenderTexture>();
                    data.GetDensityMaps(detailTemplates, densityMaps);
                    yield return ddp.PopulateDetailDensity(detailTemplates, densityMaps);
                    detailDensityPopulated?.Invoke(this, t, detailTemplates, densityMaps);
                }
                yield return null;
                if (taskHandle.isCompleted) yield break;

                if (t is IDetailInstancePopulator dip)
                {
                    List<DetailTemplate> detailTemplates = new List<DetailTemplate>();
                    List<ComputeBuffer> detailBuffers = new List<ComputeBuffer>();
                    data.GetDetailInstances(detailTemplates, detailBuffers);
                    dip.PopulateDetailInstance(detailTemplates, detailBuffers);
                    detailInstancePopulated?.Invoke(this, t, detailTemplates, detailBuffers);
                }
                yield return null;
                if (taskHandle.isCompleted) yield break;

                if (t is IObjectPopulator op)
                {
                    List<ObjectTemplate> objectTemplates = new List<ObjectTemplate>();
                    List<ComputeBuffer> sampleBuffers = new List<ComputeBuffer>();
                    data.GetObjects(objectTemplates, sampleBuffers);
                    yield return op.PopulateObject(objectTemplates, sampleBuffers, objectPopulateArgs);
                    objectPopulated?.Invoke(this, t, objectTemplates, sampleBuffers);
                }
                yield return null;
                if (taskHandle.isCompleted) yield break;

                if (t is IGenericTexturePopulator gtp)
                {
                    List<string> genericTextureLabels = new List<string>();
                    List<RenderTexture> genericTextures = new List<RenderTexture>();
                    data.GetGenericTextures(genericTextureLabels, genericTextures);
                    gtp.PopulateGenericTextures(genericTextureLabels, genericTextures);
                    genericTexturesPopulated?.Invoke(this, t, genericTextureLabels, genericTextures);
                }
                yield return null;
                if (taskHandle.isCompleted) yield break;

                if (t is IGenericBufferPopulator gbp)
                {
                    List<string> genericBufferLabels = new List<string>();
                    List<ComputeBuffer> genericBuffers = new List<ComputeBuffer>();
                    data.GetGenericBuffers(genericBufferLabels, genericBuffers);
                    gbp.PopulateGenericBuffers(genericBufferLabels, genericBuffers);
                    genericBuffersPopulated?.Invoke(this, t, genericBufferLabels, genericBuffers);
                }

                data.Dispose();
                yield return null;
                if (taskHandle.isCompleted) yield break;
            }
            yield return null;
            if (taskHandle.isCompleted) yield break;

#if UNITY_EDITOR
            Progress.Report(editorProgressId, 1, "Finishing up");
#endif

            foreach (ITile t in overlappedTiles)
            {
                currentlyProcessingTile = t;
                if (t is IGeometryPopulator gp)
                {
                    gp.MatchSeams();
                }
            }
            yield return null;
            if (taskHandle.isCompleted) yield break;

            foreach (ITile t in overlappedTiles)
            {
                currentlyProcessingTile = t;
                t.OnAfterApplyingData();
            }

            foreach (IBiome b in biomes)
            {
                b.OnAfterVMGenerate();
            }

            currentlyProcessingTile = null;
            UpdateBiomeCounter(biomes);

            afterGeneratingUnityCallback.Invoke();
            afterGenerating?.Invoke(this);

            taskHandle.Complete();
            SetActiveTask(null);
#if UNITY_EDITOR
            Progress.Finish(editorProgressId);
#endif
        }

        protected void UpdateBiomeCounter(IEnumerable<IBiome> biomes)
        {
            foreach (IBiome b in biomes)
            {
                b.updateCounter = m_updateCounter;
            }
        }

        public ProgressiveTask ForceGenerate()
        {
            m_updateCounter = DateTime.Now.Ticks;
            return GenerateAll();
        }

        protected static void GetPipelineDelegates(List<string> names, List<Delegate> delegates)
        {
            names.Add(nameof(collectTiles)); delegates.Add(collectTiles);

            names.Add(nameof(beforeGenerating)); delegates.Add(beforeGenerating);

            names.Add(nameof(heightMapPopulated)); delegates.Add(heightMapPopulated);
            names.Add(nameof(holeMapPopulated)); delegates.Add(holeMapPopulated);
            names.Add(nameof(meshDensityMapPopulated)); delegates.Add(meshDensityMapPopulated);

            names.Add(nameof(albedoMapPopulated)); delegates.Add(albedoMapPopulated);
            names.Add(nameof(metallicMapPopulated)); delegates.Add(metallicMapPopulated);

            names.Add(nameof(layerWeightPopulated)); delegates.Add(layerWeightPopulated);

            names.Add(nameof(treePopulated)); delegates.Add(treePopulated);

            names.Add(nameof(detailDensityPopulated)); delegates.Add(detailDensityPopulated);
            names.Add(nameof(detailInstancePopulated)); delegates.Add(detailInstancePopulated);

            names.Add(nameof(objectPopulated)); delegates.Add(objectPopulated);

            names.Add(nameof(genericTexturesPopulated)); delegates.Add(genericTexturesPopulated);
            names.Add(nameof(genericBuffersPopulated)); delegates.Add(genericBuffersPopulated);

            names.Add(nameof(afterGenerating)); delegates.Add(afterGenerating);
        }

        public void CollectSceneHeight(RenderTexture targetRt, Bounds worldBounds)
        {
            ITile[] tiles = GetTileArray();
            SceneDataUtils.CollectWorldHeight(tiles, targetRt, worldBounds);
        }

        private void OnDrawGizmosSelected()
        {
            drawGizmosSelectedCallback?.Invoke();
        }
    }
}
#endif