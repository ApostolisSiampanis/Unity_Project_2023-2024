#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pinwheel.Vista.UnityTerrain
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Terrain))]
    [AddComponentMenu("Vista/Terrain Tile")]
    public class TerrainTile : MonoBehaviour, ITile, IGeometryPopulator, ILayerWeightsPopulator, ITreePopulator, IDetailDensityPopulator, IObjectPopulator, IGenericTexturePopulator, IGenericBufferPopulator, ISceneHeightProvider
    {
        public event PopulateGenericTexturesHandler populateGenericTexturesCallback;
        public event PopulateGenericBuffersHandler populateGenericBuffersCallback;
        public event PopulatePrefabHandler populatePrefabInstanceCallback;

        [SerializeField]
        private string m_managerId;
        public string managerId
        {
            get
            {
                return m_managerId;
            }
            set
            {
                m_managerId = value;
            }
        }

        public Terrain terrain { get; private set; }

        public Bounds worldBounds
        {
            get
            {
                Vector3 worldCenter = transform.TransformPoint(terrain.terrainData.size * 0.5f);
                Vector3 worldSize = transform.TransformVector(terrain.terrainData.size);
                return new Bounds(worldCenter, worldSize);
            }
        }

        public float maxHeight
        {
            get
            {
                if (terrain != null && terrain.terrainData != null)
                {
                    return terrain.terrainData.size.y;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (terrain != null && terrain.terrainData != null)
                {
                    Vector3 size = terrain.terrainData.size;
                    size.y = value;
                    terrain.terrainData.size = size;
                }
            }
        }

        public int heightMapResolution
        {
            get
            {
                if (terrain != null && terrain.terrainData != null)
                {
                    return terrain.terrainData.heightmapResolution;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (terrain != null && terrain.terrainData != null && terrain.terrainData.heightmapResolution != value)
                {
                    SetHeightMapResolution(value);
                }
            }
        }

        public int textureResolution
        {
            get
            {
                if (terrain != null && terrain.terrainData != null)
                {
                    return terrain.terrainData.alphamapResolution;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (terrain != null && terrain.terrainData != null && terrain.terrainData.alphamapResolution != value)
                {
                    SetTextureResolution(value);
                }
            }
        }

        public int detailDensityMapResolution
        {
            get
            {
                if (terrain != null && terrain.terrainData != null)
                {
                    return terrain.terrainData.detailResolution;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (terrain != null && terrain.terrainData != null && terrain.terrainData.detailResolution != value)
                {
                    terrain.terrainData.SetDetailResolution(value, terrain.terrainData.detailResolutionPerPatch);
                }
            }
        }

        [SerializeField]
        private TerrainLayer[] m_terrainLayers;
        public TerrainLayer[] terrainLayers
        {
            get
            {
                return m_terrainLayers;
            }
        }

        private void OnEnable()
        {
            terrain = GetComponent<Terrain>();
            VistaManager.collectTiles += OnCollectTiles;
            MatchSeams();
        }

        private void OnDisable()
        {
            VistaManager.collectTiles -= OnCollectTiles;
        }

        private void OnCollectTiles(VistaManager manager, Collector<ITile> tiles)
        {
            if (string.Equals(manager.id, m_managerId) && terrain != null && terrain.terrainData != null)
            {
                if (terrain != null && terrain.terrainData != null)
                {
                    tiles.Add(this);
                }
            }
        }

        private void SetHeightMapResolution(int res)
        {
            RenderTexture scaledHm = new RenderTexture(res, res, 0, Terrain.heightmapFormat);
            Drawing.Blit(terrain.terrainData.heightmapTexture, scaledHm);

            Vector3 size = terrain.terrainData.size;
            terrain.terrainData.heightmapResolution = res;
            terrain.terrainData.size = size;

            RenderTexture.active = scaledHm;
            RectInt srcRect = new RectInt(0, 0, scaledHm.width, scaledHm.height);
            Vector2Int dst = new Vector2Int(0, 0);
            terrain.terrainData.CopyActiveRenderTextureToHeightmap(srcRect, dst, TerrainHeightmapSyncControl.None);
            RenderTexture.active = null;

            scaledHm.Release();
            Object.DestroyImmediate(scaledHm);
        }

        private void SetTextureResolution(int res)
        {
            int textureCount = terrain.terrainData.alphamapTextureCount;
            RenderTexture[] scaledAlphaMaps = new RenderTexture[textureCount];
            for (int i = 0; i < textureCount; ++i)
            {
                scaledAlphaMaps[i] = new RenderTexture(res, res, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Drawing.Blit(terrain.terrainData.alphamapTextures[i], scaledAlphaMaps[i]);
            }

            terrain.terrainData.alphamapResolution = res;
            terrain.terrainData.baseMapResolution = res;

            for (int i = 0; i < textureCount; ++i)
            {
                RenderTexture.active = scaledAlphaMaps[i];
                terrain.terrainData.alphamapTextures[i].ReadPixels(new Rect(0, 0, res, res), 0, 0);
                terrain.terrainData.alphamapTextures[i].Apply();
                RenderTexture.active = null;

                scaledAlphaMaps[i].Release();
                Object.DestroyImmediate(scaledAlphaMaps[i]);
            }
        }

        public void PopulateHeightMap(RenderTexture heightMap)
        {
            int resolution = terrain.terrainData.heightmapResolution;
            //generated height map is in range [0,1]
            //Unity uses some packing for its height value
            RenderTexture remappedHeightMap = new RenderTexture(resolution, resolution, 0, Terrain.heightmapRenderTextureFormat);
            TerrainTileUtilities.ConvertHeightMapToUnity(heightMap, remappedHeightMap);

            RenderTexture.active = remappedHeightMap;
            RectInt srcRect = new RectInt(0, 0, remappedHeightMap.width, remappedHeightMap.height);
            Vector2Int dst = new Vector2Int(0, 0);
            terrain.terrainData.CopyActiveRenderTextureToHeightmap(srcRect, dst, TerrainHeightmapSyncControl.None);
            RenderTexture.active = null;

            remappedHeightMap.Release();
            Object.DestroyImmediate(remappedHeightMap);
        }

        public void PopulateHoleMap(RenderTexture holeMap)
        {
            int resolution = terrain.terrainData.holesResolution;
            RenderTexture scaledHoleMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R8);
            Drawing.Blit(holeMap, scaledHoleMap);

            Texture2D map = new Texture2D(resolution, resolution, TextureFormat.R8, false);
            RenderTexture.active = scaledHoleMap;
            map.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            map.Apply();
            RenderTexture.active = null;

            byte[] data = map.GetRawTextureData();
            bool[,] holes = new bool[resolution, resolution];
            for (int y = 0; y < resolution; ++y)
            {
                for (int x = 0; x < resolution; ++x)
                {
                    byte b = data[y * resolution + x];
                    holes[y, x] = (b == 0);
                }
            }
            terrain.terrainData.SetHolesDelayLOD(0, 0, holes);

            scaledHoleMap.Release();
            Object.DestroyImmediate(scaledHoleMap);
            Object.DestroyImmediate(map);
        }

        public void PopulateMeshDensityMap(RenderTexture meshDensityMap)
        {

        }

        public void UpdateGeometry()
        {
            terrain.terrainData.SyncHeightmap();
        }

        public void MatchSeams()
        {
            if (terrain.terrainData == null)
                return;

            MatchSeamLeft();
            MatchSeamTop();
            MatchSeamRight();
            MatchSeamBottom();
            terrain.terrainData.SyncHeightmap();
        }

        public void PopulateLayerWeights(List<TerrainLayer> layers, List<RenderTexture> weights)
        {
            List<TerrainLayer> distinctLayers;
            List<RenderTexture> alphaMaps;
            int resolution = textureResolution;

            AlphaMapsCombiner combiner = new AlphaMapsCombiner();
            combiner.CombineAndMerge(layers, weights, resolution, out distinctLayers, out alphaMaps);

            m_terrainLayers = distinctLayers.ToArray();
            terrain.terrainData.terrainLayers = m_terrainLayers;

            for (int i = 0; i < alphaMaps.Count; ++i)
            {
                Texture2D alphaMap = terrain.terrainData.GetAlphamapTexture(i);
                RenderTexture.active = alphaMaps[i];
                alphaMap.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                alphaMap.Apply();
            }

            RenderTexture.active = null;
            for (int i = 0; i < alphaMaps.Count; ++i)
            {
                alphaMaps[i].Release();
                Object.DestroyImmediate(alphaMaps[i]);
            }
        }

        private void MatchSeamLeft()
        {
            Terrain leftNeighbor = terrain.leftNeighbor;
            if (leftNeighbor == null || leftNeighbor.terrainData == null)
                return;
            if (leftNeighbor.terrainData.heightmapResolution == terrain.terrainData.heightmapResolution)
            {
                int resolution = terrain.terrainData.heightmapResolution;
                float[,] neighborHeights = leftNeighbor.terrainData.GetHeights(resolution - 1, 0, 1, resolution);
                float[,] selfHeights = terrain.terrainData.GetHeights(0, 0, 1, resolution);
                float[,] avgHeights = new float[resolution, 1];
                for (int i = 0; i < resolution; ++i)
                {
                    avgHeights[i, 0] = (selfHeights[i, 0] + neighborHeights[i, 0]) * 0.5f;
                }
                terrain.terrainData.SetHeightsDelayLOD(0, 0, avgHeights);
                leftNeighbor.terrainData.SetHeights(resolution - 1, 0, avgHeights);
            }

            if (leftNeighbor.terrainData.alphamapResolution == terrain.terrainData.alphamapResolution)
            {
                int resolution = terrain.terrainData.alphamapResolution;
                float[,,] neighborAlpha = leftNeighbor.terrainData.GetAlphamaps(resolution - 1, 0, 1, resolution);
                float[,,] selfAlpha = terrain.terrainData.GetAlphamaps(0, 0, 1, resolution);

                TerrainLayer[] layers = terrain.terrainData.terrainLayers;
                for (int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
                {
                    int neighborLayerIndex = leftNeighbor.terrainData.GetLayerIndex(layers[layerIndex]);
                    if (neighborLayerIndex < 0)
                        continue;
                    for (int i = 0; i < resolution; ++i)
                    {
                        float avg = (neighborAlpha[i, 0, neighborLayerIndex] + selfAlpha[i, 0, layerIndex]) * 0.5f;
                        neighborAlpha[i, 0, neighborLayerIndex] = avg;
                        selfAlpha[i, 0, layerIndex] = avg;
                    }
                }
                terrain.terrainData.SetAlphamaps(0, 0, selfAlpha);
                leftNeighbor.terrainData.SetAlphamaps(resolution - 1, 0, neighborAlpha);
            }
        }

        private void MatchSeamTop()
        {
            Terrain topNeighbor = terrain.topNeighbor;
            if (topNeighbor == null || topNeighbor.terrainData == null)
                return;
            if (topNeighbor.terrainData.heightmapResolution == terrain.terrainData.heightmapResolution)
            {
                int resolution = terrain.terrainData.heightmapResolution;
                float[,] neighborHeights = topNeighbor.terrainData.GetHeights(0, 0, resolution, 1);
                float[,] selfHeights = terrain.terrainData.GetHeights(0, resolution - 1, resolution, 1);
                float[,] avgHeights = new float[1, resolution];
                for (int i = 0; i < resolution; ++i)
                {
                    avgHeights[0, i] = (selfHeights[0, i] + neighborHeights[0, i]) * 0.5f;
                }
                terrain.terrainData.SetHeightsDelayLOD(0, resolution - 1, avgHeights);
                topNeighbor.terrainData.SetHeights(0, 0, avgHeights);
            }

            if (topNeighbor.terrainData.alphamapResolution == terrain.terrainData.alphamapResolution)
            {
                int resolution = terrain.terrainData.alphamapResolution;
                float[,,] neighborAlpha = topNeighbor.terrainData.GetAlphamaps(0, 0, resolution, 1);
                float[,,] selfAlpha = terrain.terrainData.GetAlphamaps(0, resolution - 1, resolution, 1);

                TerrainLayer[] layers = terrain.terrainData.terrainLayers;
                for (int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
                {
                    int neighborLayerIndex = topNeighbor.terrainData.GetLayerIndex(layers[layerIndex]);
                    if (neighborLayerIndex < 0)
                        continue;
                    for (int i = 0; i < resolution; ++i)
                    {
                        float avg = (neighborAlpha[0, i, neighborLayerIndex] + selfAlpha[0, i, layerIndex]) * 0.5f;
                        neighborAlpha[0, i, neighborLayerIndex] = avg;
                        selfAlpha[0, i, layerIndex] = avg;
                    }
                }
                terrain.terrainData.SetAlphamaps(0, resolution - 1, selfAlpha);
                topNeighbor.terrainData.SetAlphamaps(0, 0, neighborAlpha);
            }
        }

        private void MatchSeamRight()
        {
            Terrain rightNeighbor = terrain.rightNeighbor;
            if (rightNeighbor == null || rightNeighbor.terrainData == null)
                return;
            if (rightNeighbor.terrainData.heightmapResolution == terrain.terrainData.heightmapResolution)
            {
                int resolution = terrain.terrainData.heightmapResolution;
                float[,] neighborHeights = rightNeighbor.terrainData.GetHeights(0, 0, 1, resolution);
                float[,] selfHeights = terrain.terrainData.GetHeights(resolution - 1, 0, 1, resolution);
                float[,] avgHeights = new float[resolution, 1];
                for (int i = 0; i < resolution; ++i)
                {
                    avgHeights[i, 0] = (selfHeights[i, 0] + neighborHeights[i, 0]) * 0.5f;
                }
                terrain.terrainData.SetHeightsDelayLOD(resolution - 1, 0, avgHeights);
                rightNeighbor.terrainData.SetHeights(0, 0, avgHeights);
            }

            if (rightNeighbor.terrainData.alphamapResolution == terrain.terrainData.alphamapResolution)
            {
                int resolution = terrain.terrainData.alphamapResolution;
                float[,,] neighborAlpha = rightNeighbor.terrainData.GetAlphamaps(0, 0, 1, resolution);
                float[,,] selfAlpha = terrain.terrainData.GetAlphamaps(resolution - 1, 0, 1, resolution);

                TerrainLayer[] layers = terrain.terrainData.terrainLayers;
                for (int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
                {
                    int neighborLayerIndex = rightNeighbor.terrainData.GetLayerIndex(layers[layerIndex]);
                    if (neighborLayerIndex < 0)
                        continue;
                    for (int i = 0; i < resolution; ++i)
                    {
                        float avg = (neighborAlpha[i, 0, neighborLayerIndex] + selfAlpha[i, 0, layerIndex]) * 0.5f;
                        neighborAlpha[i, 0, neighborLayerIndex] = avg;
                        selfAlpha[i, 0, layerIndex] = avg;
                    }
                }
                terrain.terrainData.SetAlphamaps(resolution - 1, 0, selfAlpha);
                rightNeighbor.terrainData.SetAlphamaps(0, 0, neighborAlpha);
            }
        }

        private void MatchSeamBottom()
        {
            Terrain bottomNeighbor = terrain.bottomNeighbor;
            if (bottomNeighbor == null || bottomNeighbor.terrainData == null)
                return;
            if (bottomNeighbor.terrainData.heightmapResolution == terrain.terrainData.heightmapResolution)
            {
                int resolution = terrain.terrainData.heightmapResolution;
                float[,] neighborHeights = bottomNeighbor.terrainData.GetHeights(0, resolution - 1, resolution, 1);
                float[,] selfHeights = terrain.terrainData.GetHeights(0, 0, resolution, 1);
                float[,] avgHeights = new float[1, resolution];
                for (int i = 0; i < resolution; ++i)
                {
                    avgHeights[0, i] = (selfHeights[0, i] + neighborHeights[0, i]) * 0.5f;
                }
                terrain.terrainData.SetHeightsDelayLOD(0, 0, selfHeights);
                bottomNeighbor.terrainData.SetHeights(0, resolution - 1, avgHeights);
            }

            if (bottomNeighbor.terrainData.alphamapResolution == terrain.terrainData.alphamapResolution)
            {
                int resolution = terrain.terrainData.alphamapResolution;
                float[,,] neighborAlpha = bottomNeighbor.terrainData.GetAlphamaps(0, resolution - 1, resolution, 1);
                float[,,] selfAlpha = terrain.terrainData.GetAlphamaps(0, 0, resolution, 1);

                TerrainLayer[] layers = terrain.terrainData.terrainLayers;
                for (int layerIndex = 0; layerIndex < layers.Length; ++layerIndex)
                {
                    int neighborLayerIndex = bottomNeighbor.terrainData.GetLayerIndex(layers[layerIndex]);
                    if (neighborLayerIndex < 0)
                        continue;
                    for (int i = 0; i < resolution; ++i)
                    {
                        float avg = (neighborAlpha[0, i, neighborLayerIndex] + selfAlpha[0, i, layerIndex]) * 0.5f;
                        neighborAlpha[0, i, neighborLayerIndex] = avg;
                        selfAlpha[0, i, layerIndex] = avg;
                    }
                }
                terrain.terrainData.SetAlphamaps(0, 0, selfAlpha);
                bottomNeighbor.terrainData.SetAlphamaps(0, resolution - 1, neighborAlpha);
            }
        }

        public void PopulateTrees(List<TreeTemplate> templates, List<ComputeBuffer> buffers)
        {
            List<TreeTemplate> distinctTemplates = templates.Distinct().ToList();
            int[] templateIndices = new int[templates.Count];
            for (int i = 0; i < templates.Count; ++i)
            {
                templateIndices[i] = distinctTemplates.IndexOf(templates[i]);
            }

            List<TreePrototype> prototypes = new List<TreePrototype>();
            int[] minProtoIndices = new int[distinctTemplates.Count];
            int[] maxProtoIndices = new int[distinctTemplates.Count];
            int baseProtoIndex = 0;
            for (int i = 0; i < distinctTemplates.Count; ++i)
            {
                TreeTemplate template = distinctTemplates[i];
                List<TreePrototype> prototypesFromTemplates = CreateTreePrototypesFromTemplate(template);
                prototypes.AddRange(prototypesFromTemplates);

                int minProtoIndex = baseProtoIndex;
                int maxProtoIndex = baseProtoIndex + prototypesFromTemplates.Count - 1;
                minProtoIndices[i] = minProtoIndex;
                maxProtoIndices[i] = maxProtoIndex;

                baseProtoIndex += prototypesFromTemplates.Count;
            }

            List<TreeInstance> instances = new List<TreeInstance>();
            for (int i = 0; i < buffers.Count; ++i)
            {
                ComputeBuffer buffer = buffers[i];
                int tIndex = templateIndices[i];
                int minProtoIndex = minProtoIndices[tIndex];
                int maxProtoIndex = maxProtoIndices[tIndex];
                ParseTreeInstances(instances, buffer, minProtoIndex, maxProtoIndex);
            }

            terrain.terrainData.treeInstances = new TreeInstance[0];
            terrain.terrainData.treePrototypes = prototypes.ToArray();
            terrain.terrainData.SetTreeInstances(instances.ToArray(), true);
        }

        private List<TreePrototype> CreateTreePrototypesFromTemplate(TreeTemplate template)
        {
            List<TreePrototype> prototypes = new List<TreePrototype>();
            if (!template.IsValid())
            {
                return prototypes;
            }

            TreePrototype p0 = new TreePrototype();
            p0.prefab = template.prefab;
            p0.bendFactor = template.bendFactor;
            p0.navMeshLod = template.navMeshLod;
            prototypes.Add(p0);

            GameObject[] variants = template.prefabVariants;
            foreach (GameObject v in variants)
            {
                if (v == null)
                    continue;
                TreePrototype p = new TreePrototype(p0);
                p.prefab = v;
                prototypes.Add(p);
            }
            return prototypes;
        }

        private void ParseTreeInstances(List<TreeInstance> instances, ComputeBuffer buffer, int minProtoIndex, int maxProtoIndex)
        {
            if (buffer.count % InstanceSample.SIZE != 0)
            {
                Debug.LogError("Cannot parse instance sample buffer");
                return;
            }

            InstanceSample[] data = new InstanceSample[buffer.count / InstanceSample.SIZE];
            buffer.GetData(data);

            foreach (InstanceSample t in data)
            {
                if (t.isValid != 1)
                    continue;
                TreeInstance tree = new TreeInstance();
                tree.position = t.position;
                tree.rotation = t.rotationY;
                tree.heightScale = t.verticalScale;
                tree.widthScale = t.horizontalScale;
                if (minProtoIndex == maxProtoIndex)
                {
                    tree.prototypeIndex = minProtoIndex;
                }
                else
                {
                    tree.prototypeIndex = Random.Range(minProtoIndex, maxProtoIndex + 1);
                }

                instances.Add(tree);
            }
        }

        public ProgressiveTask PopulateDetailDensity(List<DetailTemplate> templates, List<RenderTexture> densityMaps)
        {
            ProgressiveTask task = new ProgressiveTask();
            CoroutineUtility.StartCoroutine(PopulateDetailDensityProgressive(task, templates, densityMaps));
            return task;
        }

        private IEnumerator PopulateDetailDensityProgressive(ProgressiveTask task, List<DetailTemplate> templates, List<RenderTexture> densityMaps)
        {
            //Merge duplicated templates
            List<DetailTemplate> distinctTemplates = templates.Distinct().ToList();
            int[] templateIndices = new int[templates.Count];
            for (int i = 0; i < templates.Count; ++i)
            {
                templateIndices[i] = distinctTemplates.IndexOf(templates[i]);
            }

            int resolution = terrain.terrainData.detailResolution;
            List<RenderTexture> scaledDensityMaps = new List<RenderTexture>();
            for (int i = 0; i < distinctTemplates.Count; ++i)
            {
                RenderTexture dm = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
                scaledDensityMaps.Add(dm);
            }

            for (int i = 0; i < densityMaps.Count; ++i)
            {
                int tIndex = templateIndices[i];
                RenderTexture targetRt = scaledDensityMaps[tIndex];
                Drawing.BlitAdd(densityMaps[i], targetRt);
            }

            //Create detail prototypes
            List<DetailPrototype> prototypes = new List<DetailPrototype>();
            List<Texture2D> densityMapByProto = new List<Texture2D>();
            List<float> baseDensityByProto = new List<float>();
            for (int i = 0; i < distinctTemplates.Count; ++i)
            {
                DetailTemplate template = distinctTemplates[i];
                List<DetailPrototype> prototypesFromTemplates = CreateDetailPrototypesFromTemplate(template);
                prototypes.AddRange(prototypesFromTemplates);

                float baseDensity = template.density * 1.0f / prototypesFromTemplates.Count;
                for (int j = 0; j < prototypesFromTemplates.Count; ++j)
                {
                    //A hack for MacOS, some how the scaledDensityMaps get destroy before those coroutines to complete, cause them to read pixel from null RT
                    //Instead of copy RT data back to the CPU inside the coroutines, we do it here, then the 'dm' will be destroyed at the end of each coroutine
                    //Performance of 2 version is the same.
                    Texture2D dm = new Texture2D(resolution, resolution, TextureFormat.RFloat, false);
                    RenderTexture.active = scaledDensityMaps[i];
                    dm.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                    dm.Apply();
                    RenderTexture.active = null;

                    densityMapByProto.Add(dm);
                    baseDensityByProto.Add(baseDensity);
                }
            }

            //Populate
            terrain.terrainData.detailPrototypes = prototypes.ToArray();
            CoroutineHandle[] coroutines = new CoroutineHandle[prototypes.Count];
            int layerIndex = 0;
            for (int i = 0; i < prototypes.Count; ++i)
            {
                Texture2D dm = densityMapByProto[i];
                float baseDensity = baseDensityByProto[i];
                CoroutineHandle c = CoroutineUtility.StartCoroutine(PopulateLayerDensityProgressive(layerIndex, dm, baseDensity));
                coroutines[i] = c;
                layerIndex += 1;
            }

            foreach (CoroutineHandle c in coroutines)
            {
                yield return c;
            }

            for (int i = 0; i < scaledDensityMaps.Count; ++i)
            {
                RenderTexture.ReleaseTemporary(scaledDensityMaps[i]);
            }

            task.Complete();
            yield break;
        }

        private IEnumerator PopulateLayerDensityProgressive(int layerIndex, Texture2D densityMap, float density)
        {
            if (densityMap == null)
            {
                yield break;
            }

            int resolution = terrain.terrainData.detailResolution;
            int resolutionPerPatch = terrain.terrainData.detailResolutionPerPatch;

            int stepCount = resolution / resolutionPerPatch;
            int[,] densityArray = new int[resolutionPerPatch, resolution];
            for (int i = 0; i < stepCount; ++i)
            {
                int baseX = 0;
                int baseY = i * resolutionPerPatch;
                int blockWidth = resolution;
                int blockHeight = resolutionPerPatch;

                Color[] data = densityMap.GetPixels(baseX, baseY, blockWidth, blockHeight, 0);
                FillDensityArray(densityArray, blockWidth, blockHeight, data, density, layerIndex);
                terrain.terrainData.SetDetailLayer(baseX, baseY, layerIndex, densityArray);
                yield return null;
            }

            Object.DestroyImmediate(densityMap);
            yield break;
        }

        private List<DetailPrototype> CreateDetailPrototypesFromTemplate(DetailTemplate t)
        {
            List<DetailPrototype> prototypes = new List<DetailPrototype>();
            if (!t.IsValid())
                return prototypes;

            DetailPrototype p = new DetailPrototype();
            p.renderMode = t.renderMode;
            p.healthyColor = t.primaryColor;
            p.dryColor = t.secondaryColor;
            p.minWidth = t.minWidth;
            p.minHeight = t.minHeight;
            p.maxWidth = t.maxWidth;
            p.maxHeight = t.maxHeight;
            p.noiseSpread = t.noiseSpread;
            p.holeEdgePadding = t.holeEdgePadding;
            p.usePrototypeMesh = t.renderMode == DetailRenderMode.VertexLit;
#if UNITY_2021_2_OR_NEWER
            if (t.renderMode == DetailRenderMode.VertexLit)
            {
                p.useInstancing = t.useInstancing;
            }
            else
            {
                p.useInstancing = false;
            }
#endif

            if (t.renderMode == DetailRenderMode.VertexLit)
            {
                p.prototype = t.prefab;
            }
            else
            {
                p.prototypeTexture = t.texture;
            }
            prototypes.Add(p);

            if (t.renderMode == DetailRenderMode.VertexLit)
            {
                GameObject[] variants = t.prefabVariants;
                foreach (GameObject v in variants)
                {
                    if (v == null)
                        continue;
                    DetailPrototype p0 = new DetailPrototype(p);
                    p0.prototype = v;
                    prototypes.Add(p0);
                }
            }
            else
            {
                Texture2D[] variants = t.textureVariants;

                foreach (Texture2D v in variants)
                {
                    if (v == null)
                        continue;
                    DetailPrototype p0 = new DetailPrototype(p);
                    p0.prototypeTexture = v;
                    prototypes.Add(p0);
                }
            }

            return prototypes;
        }

        private void FillDensityArray(int[,] array, int width, int height, Color[] data, float baseDensity, int randomSeed)
        {
            Random.InitState(randomSeed * 12345);
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    float densityFloat = (data[y * width + x].r * baseDensity);
                    int densityInt = (int)densityFloat;
                    float remainder = densityFloat - densityInt;
                    if (remainder > Random.value)
                    {
                        densityInt += 1;
                    }
                    array[y, x] = densityInt;
                }
            }
        }

        public ProgressiveTask PopulateObject(List<ObjectTemplate> templates, List<ComputeBuffer> sampleBuffers, VistaManager.ObjectPopulateArgs objectPopulateArgs)
        {
            ProgressiveTask task = new ProgressiveTask();
            CoroutineUtility.StartCoroutine(PopulateObjectProgressive(task, templates, sampleBuffers, objectPopulateArgs));
            return task;
        }

        private IEnumerator PopulateObjectProgressive(ProgressiveTask task, List<ObjectTemplate> templates, List<ComputeBuffer> sampleBuffers, VistaManager.ObjectPopulateArgs objectPopulateArgs)
        {
            string rootName = SpawnUtilities.ROOT_NAME;
            Transform existingRoot = terrain.transform.Find(rootName);
            if (existingRoot != null)
            {
                DestroyImmediate(existingRoot.gameObject);
            }

            Transform mainRoot = new GameObject(rootName).transform;
            mainRoot.parent = terrain.transform;
            mainRoot.localPosition = Vector3.zero;
            mainRoot.localRotation = Quaternion.identity;
            mainRoot.localScale = Vector3.one;

            List<ObjectTemplate> distinctTemplates = templates.Distinct().ToList();
            int[] templateIndices = new int[templates.Count];
            for (int i = 0; i < templates.Count; ++i)
            {
                templateIndices[i] = distinctTemplates.IndexOf(templates[i]);
            }

            CoroutineHandle[] coroutines = new CoroutineHandle[templates.Count];
            for (int i = 0; i < sampleBuffers.Count; ++i)
            {
                int tIndex = templateIndices[i];
                ObjectTemplate template = distinctTemplates[tIndex];
                CoroutineHandle c = CoroutineUtility.StartCoroutine(PopulateObjectProgressive(template, sampleBuffers[i], mainRoot, objectPopulateArgs));
                coroutines[i] = c;
            }

            foreach (CoroutineHandle c in coroutines)
            {
                yield return c;
            }

            task.Complete();
            yield break;
        }

        private IEnumerator PopulateObjectProgressive(ObjectTemplate template, ComputeBuffer buffer, Transform mainRoot, VistaManager.ObjectPopulateArgs objectPopulateArgs)
        {
            if (buffer.count % InstanceSample.SIZE != 0)
            {
                Debug.LogError("Cannot parse tree sample buffer");
                yield break;
            }
            if (mainRoot == null)
            {
                yield break;
            }

            int instanceCount = buffer.count / InstanceSample.SIZE;
            InstanceSample[] samples = new InstanceSample[buffer.count / InstanceSample.SIZE];
            buffer.GetData(samples);

            string prefabRootName = $"~{template.name}";
            Transform prefabRoot = mainRoot.Find(prefabRootName);
            if (prefabRoot == null)
            {
                prefabRoot = new GameObject(prefabRootName).transform;
                prefabRoot.parent = mainRoot;
                prefabRoot.localPosition = Vector3.zero;
                prefabRoot.localRotation = Quaternion.identity;
                prefabRoot.localScale = Vector3.one;
            }

            List<GameObject> prefabs = new List<GameObject>();
            prefabs.Add(template.prefab);
            foreach (GameObject g in template.prefabVariants)
            {
                if (g != null)
                {
                    prefabs.Add(g);
                }
            }
            int prefabCount = prefabs.Count;

            Vector3 terrainSize = terrain.terrainData.size;
            for (int i = 0; i < instanceCount; ++i)
            {
                if (mainRoot == null || prefabRoot == null)
                {
                    yield break;
                }

                InstanceSample sample = samples[i];
                if (sample.isValid == 0)
                    continue;
                GameObject prefab;
                if (prefabCount == 1)
                {
                    prefab = prefabs[0];
                }
                else
                {
                    prefab = prefabs[Random.Range(0, prefabs.Count)];
                }

                Vector3 localPosition = new Vector3(
                    Mathf.Lerp(0, terrainSize.x, sample.position.x),
                    terrain.terrainData.GetInterpolatedHeight(sample.position.x, sample.position.z),
                    Mathf.Lerp(0, terrainSize.z, sample.position.z));
                Quaternion localRotation = Quaternion.Euler(0, sample.rotationY * Mathf.Rad2Deg, 0);
                Vector3 baseScale = prefab.transform.localScale;
                Vector3 localScale = new Vector3(sample.horizontalScale, sample.verticalScale, sample.horizontalScale);
                localScale.Scale(baseScale);

                if (template.alignToNormal)
                {
                    Vector3 normalVector = terrain.terrainData.GetInterpolatedNormal(sample.position.x, sample.position.z);
                    float errorFactor = Random.Range(1 - template.normalAlignmentError, 1 + template.normalAlignmentError);
                    normalVector = Vector3.LerpUnclamped(Vector3.up, normalVector, errorFactor);
                    Quaternion alignmentRotation = Quaternion.FromToRotation(Vector3.up, normalVector);
                    localRotation *= alignmentRotation;
                }

                GameObject instance = SpawnUtilities.Spawn(prefab);
                instance.transform.parent = prefabRoot;
                instance.transform.localPosition = localPosition;
                instance.transform.localRotation = localRotation;
                instance.transform.localScale = localScale;
                populatePrefabInstanceCallback?.Invoke(this, instance);

                if (i % objectPopulateArgs.objectsPerFrame == 0)
                {
                    yield return null;
                }
            }

            yield break;
        }

        public void PopulateGenericTextures(List<string> labels, List<RenderTexture> textures)
        {
            populateGenericTexturesCallback?.Invoke(labels, textures);
        }

        public void PopulateGenericBuffers(List<string> labels, List<ComputeBuffer> buffers)
        {
            populateGenericBuffersCallback?.Invoke(labels, buffers);
        }

        public void OnBeforeApplyingData()
        {
        }

        public void OnAfterApplyingData()
        {
        }

        public void OnCollectSceneHeight(RenderTexture targetRt, Rect requestedWorldRect)
        {
            Bounds selfWorldBounds = worldBounds;
            Rect selfRect = new Rect(selfWorldBounds.min.x, selfWorldBounds.min.z, selfWorldBounds.size.x, selfWorldBounds.size.z);
            float minX = Utilities.InverseLerpUnclamped(requestedWorldRect.min.x, requestedWorldRect.max.x, selfRect.min.x);
            float maxX = Utilities.InverseLerpUnclamped(requestedWorldRect.min.x, requestedWorldRect.max.x, selfRect.max.x) + targetRt.texelSize.x;
            float minY = Utilities.InverseLerpUnclamped(requestedWorldRect.min.y, requestedWorldRect.max.y, selfRect.min.y);
            float maxY = Utilities.InverseLerpUnclamped(requestedWorldRect.min.y, requestedWorldRect.max.y, selfRect.max.y) + targetRt.texelSize.y;

            Vector2[] uvCorner = new Vector2[]
            {
                new Vector2(minX, minY),
                new Vector2(minX, maxY),
                new Vector2(maxX, maxY),
                new Vector2(maxX, minY)
            };

            RenderTexture terrainHeightMap = terrain.terrainData.heightmapTexture;
            TerrainTileUtilities.DecodeAndDrawHeightMap(targetRt, terrainHeightMap, uvCorner);
        }
    }
}
#endif
