#if VISTA
#if GRIFFIN
using Pinwheel.Griffin;
using Pinwheel.Vista.Graphics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pinwheel.Vista.PolarisTerrain
{
    [RequireComponent(typeof(GStylizedTerrain))]
    [ExecuteInEditMode]
    [AddComponentMenu("Vista/Polaris Tile")]
    public class PolarisTile : MonoBehaviour, ITile, IGeometryPopulator, IAlbedoMapPopulator, IMetallicMapPopulator, ILayerWeightsPopulator, ITreePopulator, IDetailInstancePopulator, IObjectPopulator, IGenericTexturePopulator, IGenericBufferPopulator, ISceneHeightProvider, ISerializationCallbackReceiver
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

        public GStylizedTerrain terrain { get; private set; }

        public Bounds worldBounds
        {
            get
            {
                return terrain.Bounds;
            }
        }

        public float maxHeight
        {
            get
            {
                if (terrain != null && terrain.TerrainData != null)
                {
                    return terrain.TerrainData.Geometry.Height;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (terrain != null && terrain.TerrainData != null)
                {
                    terrain.TerrainData.Geometry.Height = value;
                }
            }
        }

        public int heightMapResolution
        {
            get
            {
                if (terrain != null && terrain.TerrainData != null)
                {
                    return terrain.TerrainData.Geometry.HeightMapResolution;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (terrain != null && terrain.TerrainData != null)
                {
                    terrain.TerrainData.Geometry.HeightMapResolution = value;
                }
            }
        }

        public int textureResolution
        {
            get
            {
                if (terrain != null && terrain.TerrainData != null)
                {
                    return terrain.TerrainData.Shading.SplatControlResolution;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (terrain != null && terrain.TerrainData != null)
                {
                    terrain.TerrainData.Shading.SplatControlResolution = value;
                    terrain.TerrainData.Shading.AlbedoMapResolution = value;
                    terrain.TerrainData.Shading.MetallicMapResolution = value;
                }
            }
        }

        public int detailDensityMapResolution
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        [SerializeField]
        private List<GSplatPrototype> m_splatPrototypesSerialized;
        [SerializeField]
        private List<GTreePrototype> m_treePrototypesSerialized;
        [SerializeField]
        private List<GGrassPrototype> m_grassPrototypesSerialized;

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
            terrain = GetComponent<GStylizedTerrain>();
            VistaManager.collectTiles += OnCollectTiles;
            DeserializePrototypes();
        }

        private void OnDisable()
        {
            VistaManager.collectTiles -= OnCollectTiles;
        }

        private void OnCollectTiles(VistaManager manager, Collector<ITile> tiles)
        {
            if (string.Equals(manager.id, m_managerId) && terrain != null && terrain.TerrainData != null)
            {
                tiles.Add(this);
            }
        }

        public void OnBeforeApplyingData()
        {
        }

        public void OnAfterApplyingData()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(terrain.TerrainData);
#endif
        }

        public void PopulateHeightMap(RenderTexture heightMap)
        {
            int resolution = terrain.TerrainData.Geometry.HeightMapResolution;
            RenderTexture destHeightMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            PolarisTileUtilities.SetHeightMap(terrain.TerrainData.Geometry.HeightMap, heightMap, destHeightMap);

            RenderTexture.active = destHeightMap;
            terrain.TerrainData.Geometry.HeightMap.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            terrain.TerrainData.Geometry.HeightMap.Apply();
            RenderTexture.active = null;
            destHeightMap.Release();
            Object.DestroyImmediate(destHeightMap);
        }

        public void PopulateHoleMap(RenderTexture holeMap)
        {
            int resolution = terrain.TerrainData.Geometry.HeightMapResolution;
            RenderTexture destHeightMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            PolarisTileUtilities.SetHoleMap(terrain.TerrainData.Geometry.HeightMap, holeMap, destHeightMap);

            RenderTexture.active = destHeightMap;
            terrain.TerrainData.Geometry.HeightMap.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            terrain.TerrainData.Geometry.HeightMap.Apply();
            RenderTexture.active = null;
            destHeightMap.Release();
            Object.DestroyImmediate(destHeightMap);
        }

        public void PopulateMeshDensityMap(RenderTexture meshDensityMap)
        {
            int resolution = terrain.TerrainData.Geometry.HeightMapResolution;
            RenderTexture destHeightMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            PolarisTileUtilities.SetMeshDensityMap(terrain.TerrainData.Geometry.HeightMap, meshDensityMap, destHeightMap);

            RenderTexture.active = destHeightMap;
            terrain.TerrainData.Geometry.HeightMap.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            terrain.TerrainData.Geometry.HeightMap.Apply();
            RenderTexture.active = null;
            destHeightMap.Release();
            Object.DestroyImmediate(destHeightMap);
        }

        public void UpdateGeometry()
        {
            terrain.TerrainData.Geometry.SetRegionDirty(GCommon.UnitRect);
            terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
        }

        public void MatchSeams()
        {
            terrain.MatchEdges();
        }

        public void PopulateAlbedoMap(RenderTexture albedoMap)
        {
            int resolution = terrain.TerrainData.Shading.AlbedoMapResolution;
            if (resolution == albedoMap.width && resolution == albedoMap.height)
            {
                RenderTexture.active = albedoMap;
                terrain.TerrainData.Shading.AlbedoMap.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                terrain.TerrainData.Shading.AlbedoMap.Apply();
                RenderTexture.active = null;
            }
            else
            {
                RenderTexture scaledAlbedo = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Drawing.Blit(albedoMap, scaledAlbedo);

                RenderTexture.active = scaledAlbedo;
                terrain.TerrainData.Shading.AlbedoMap.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                terrain.TerrainData.Shading.AlbedoMap.Apply();
                RenderTexture.active = null;

                scaledAlbedo.Release();
                Object.DestroyImmediate(scaledAlbedo);
            }

            terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
        }

        public void PopulateMetallicMap(RenderTexture metallicMap)
        {
            int resolution = terrain.TerrainData.Shading.MetallicMapResolution;
            if (resolution == metallicMap.width && resolution == metallicMap.height)
            {
                RenderTexture.active = metallicMap;
                terrain.TerrainData.Shading.MetallicMap.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                terrain.TerrainData.Shading.MetallicMap.Apply();
                RenderTexture.active = null;
            }
            else
            {
                RenderTexture scaledMetallicMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Drawing.Blit(metallicMap, scaledMetallicMap);

                RenderTexture.active = scaledMetallicMap;
                terrain.TerrainData.Shading.MetallicMap.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                terrain.TerrainData.Shading.MetallicMap.Apply();
                RenderTexture.active = null;

                scaledMetallicMap.Release();
                Object.DestroyImmediate(scaledMetallicMap);
            }
            terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
        }

        public void PopulateLayerWeights(List<TerrainLayer> layers, List<RenderTexture> weights)
        {
            List<TerrainLayer> distinctLayers;
            List<RenderTexture> alphaMaps;
            int resolution = textureResolution;

            AlphaMapsCombiner combiner = new AlphaMapsCombiner();
            combiner.CombineAndMerge(layers, weights, resolution, out distinctLayers, out alphaMaps);

            m_terrainLayers = distinctLayers.ToArray();

            List<GSplatPrototype> prototypes = new List<GSplatPrototype>();
            foreach (TerrainLayer l in m_terrainLayers)
            {
                prototypes.Add((GSplatPrototype)l);
            }
            GSplatPrototypeGroup splatGroup = CreateSplatGroup(prototypes);
            terrain.TerrainData.Shading.Splats = splatGroup;

#if __MICROSPLAT_POLARIS__ 
            JBooth.MicroSplat.TextureArrayConfig cfg = terrain.TerrainData.Shading.MicroSplatTextureArrayConfig;
            while (cfg!=null && cfg.sourceTextures.Count < m_terrainLayers.Length)
            {
                cfg.sourceTextures.Add(new JBooth.MicroSplat.TextureArrayConfig.TextureEntry());
            }
#endif

            for (int i = 0; i < alphaMaps.Count; ++i)
            {
                Texture2D alphaMap = terrain.TerrainData.Shading.GetSplatControl(i);
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

        private GSplatPrototypeGroup CreateSplatGroup(List<GSplatPrototype> prototypes)
        {
            GSplatPrototypeGroup splatGroup = ScriptableObject.CreateInstance<GSplatPrototypeGroup>();
            splatGroup.name = "~GeneratedSplatGroup";
            splatGroup.Prototypes = prototypes;
            return splatGroup;
        }

        public void PopulateTrees(List<TreeTemplate> templates, List<ComputeBuffer> buffers)
        {
            List<TreeTemplate> distinctTemplates = templates.Distinct().ToList();
            int[] templateIndices = new int[templates.Count];
            for (int i = 0; i < templates.Count; ++i)
            {
                templateIndices[i] = distinctTemplates.IndexOf(templates[i]);
            }

            List<GTreePrototype> prototypes = new List<GTreePrototype>();
            int[] minProtoIndices = new int[distinctTemplates.Count];
            int[] maxProtoIndices = new int[distinctTemplates.Count];
            int baseProtoIndex = 0;
            for (int i = 0; i < distinctTemplates.Count; ++i)
            {
                TreeTemplate template = distinctTemplates[i];
                List<GTreePrototype> prototypesFromTemplates = CreateTreePrototypesFromTemplate(template);
                prototypes.AddRange(prototypesFromTemplates);

                int minProtoIndex = baseProtoIndex;
                int maxProtoIndex = baseProtoIndex + prototypesFromTemplates.Count - 1;
                minProtoIndices[i] = minProtoIndex;
                maxProtoIndices[i] = maxProtoIndex;

                baseProtoIndex += prototypesFromTemplates.Count;
            }

            List<GTreeInstance> instances = new List<GTreeInstance>();
            for (int i = 0; i < buffers.Count; ++i)
            {
                ComputeBuffer buffer = buffers[i];
                int tIndex = templateIndices[i];
                int minProtoIndex = minProtoIndices[tIndex];
                int maxProtoIndex = maxProtoIndices[tIndex];
                ParseTreeInstances(instances, buffer, minProtoIndex, maxProtoIndex);
            }

            GTreePrototypeGroup treeGroup = CreateTreeGroup(prototypes);
            terrain.TerrainData.Foliage.Trees = treeGroup;
            terrain.TerrainData.Foliage.TreeInstances = instances;
            terrain.TerrainData.Foliage.SetTreeRegionDirty(new Rect(0, 0, 1, 1));
            terrain.UpdateTreesPosition();
            terrain.TerrainData.Foliage.ClearTreeDirtyRegions();
        }

        private List<GTreePrototype> CreateTreePrototypesFromTemplate(TreeTemplate template)
        {
            List<GTreePrototype> prototypes = new List<GTreePrototype>();
            if (template.prefab == null)
            {
                return prototypes;
            }

            List<GameObject> prefabs = new List<GameObject>();
            prefabs.Add(template.prefab);
            GameObject[] variants = template.prefabVariants;
            foreach (GameObject v in variants)
            {
                if (v != null)
                {
                    prefabs.Add(v);
                }
            }

            foreach (GameObject p in prefabs)
            {
                GTreePrototype proto = new GTreePrototype();
                proto.Prefab = p;
                proto.BaseScale = template.baseScale;
                proto.BaseRotation = template.baseRotation;

                proto.ShadowCastingMode = template.shadowCastingMode;
                proto.ReceiveShadow = template.receiveShadow;

                proto.Billboard = template.billboard;
                proto.BillboardShadowCastingMode = template.billboardShadowCastingMode;
                proto.BillboardReceiveShadow = template.billboardReceiveShadow;

                proto.KeepPrefabLayer = template.keepPrefabLayer;
                proto.Layer = template.layer;
                proto.PivotOffset = template.pivotOffset;

                prototypes.Add(proto);
            }

            return prototypes;
        }

        private void ParseTreeInstances(List<GTreeInstance> instances, ComputeBuffer buffer, int minProtoIndex, int maxProtoIndex)
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
                if (t.isValid <= 0)
                    continue;
                GTreeInstance tree = new GTreeInstance();
                tree.Position = t.position;
                tree.Rotation = Quaternion.Euler(0, t.rotationY, 0);
                tree.Scale = new Vector3(t.horizontalScale, t.verticalScale, t.horizontalScale);
                if (minProtoIndex == maxProtoIndex)
                {
                    tree.PrototypeIndex = minProtoIndex;
                }
                else
                {
                    tree.PrototypeIndex = Random.Range(minProtoIndex, maxProtoIndex + 1);
                }

                instances.Add(tree);
            }
        }

        private GTreePrototypeGroup CreateTreeGroup(List<GTreePrototype> prototypes)
        {
            GTreePrototypeGroup treeGroup = ScriptableObject.CreateInstance<GTreePrototypeGroup>();
            treeGroup.name = "~GeneratedTreeGroup";
            treeGroup.Prototypes = prototypes;
            return treeGroup;
        }

        public void PopulateDetailInstance(List<DetailTemplate> templates, List<ComputeBuffer> buffers)
        {
            List<DetailTemplate> distinctTemplates = templates.Distinct().ToList();
            int[] templateIndices = new int[templates.Count];
            for (int i = 0; i < templates.Count; ++i)
            {
                templateIndices[i] = distinctTemplates.IndexOf(templates[i]);
            }

            List<GGrassPrototype> prototypes = new List<GGrassPrototype>();
            int[] minProtoIndices = new int[distinctTemplates.Count];
            int[] maxProtoIndices = new int[distinctTemplates.Count];
            int baseProtoIndex = 0;
            for (int i = 0; i < distinctTemplates.Count; ++i)
            {
                DetailTemplate template = distinctTemplates[i];
                List<GGrassPrototype> prototypesFromTemplates = CreateGrassPrototypesFromTemplate(template);
                prototypes.AddRange(prototypesFromTemplates);

                int minProtoIndex = baseProtoIndex;
                int maxProtoIndex = baseProtoIndex + prototypesFromTemplates.Count - 1;
                minProtoIndices[i] = minProtoIndex;
                maxProtoIndices[i] = maxProtoIndex;

                baseProtoIndex += prototypesFromTemplates.Count;
            }

            List<GGrassInstance> instances = new List<GGrassInstance>();
            for (int i = 0; i < buffers.Count; ++i)
            {
                ComputeBuffer buffer = buffers[i];
                int tIndex = templateIndices[i];
                int minProtoIndex = minProtoIndices[tIndex];
                int maxProtoIndex = maxProtoIndices[tIndex];
                ParseGrassInstances(instances, buffer, minProtoIndex, maxProtoIndex);
            }

            GGrassPrototypeGroup grassGroup = CreateGrassGroup(prototypes);
            terrain.TerrainData.Foliage.ClearGrassInstances();
            terrain.TerrainData.Foliage.Grasses = grassGroup;
            terrain.TerrainData.Foliage.AddGrassInstances(instances);
            terrain.TerrainData.Foliage.SetGrassRegionDirty(new Rect(0, 0, 1, 1));
            terrain.UpdateGrassPatches();
            terrain.TerrainData.Foliage.ClearGrassDirtyRegions();
        }

        private List<GGrassPrototype> CreateGrassPrototypesFromTemplate(DetailTemplate template)
        {
            List<GGrassPrototype> prototypes = new List<GGrassPrototype>();
            if (!template.IsValid())
                return prototypes;

            if (template.renderMode == DetailRenderMode.VertexLit)
            {
                List<GameObject> prefabs = new List<GameObject>();
                prefabs.Add(template.prefab);
                GameObject[] variants = template.prefabVariants;
                foreach (GameObject g in variants)
                {
                    if (g == null)
                        continue;
                    prefabs.Add(g);
                }

                foreach (GameObject g in prefabs)
                {
                    GGrassPrototype proto = new GGrassPrototype();
                    proto.Shape = GGrassShape.DetailObject;
                    proto.Prefab = g;
                    prototypes.Add(proto);
                }
            }
            else
            {
                List<Texture2D> textures = new List<Texture2D>();
                textures.Add(template.texture);
                Texture2D[] variants = template.textureVariants;
                foreach (Texture2D t in variants)
                {
                    if (t == null)
                        continue;
                    textures.Add(t);
                }

                foreach (Texture2D t in textures)
                {
                    GGrassPrototype proto = new GGrassPrototype();
                    proto.Shape = GGrassShape.Clump;
                    proto.Texture = t;
                    prototypes.Add(proto);
                }
            }

            foreach (GGrassPrototype proto in prototypes)
            {
                proto.Color = template.primaryColor;
                proto.Size = new Vector3(template.minWidth, template.minHeight, template.minWidth);
                proto.PivotOffset = template.pivotOffset;
                proto.BendFactor = template.bendFactor;
                proto.Layer = template.layer;
                proto.AlignToSurface = template.alignToSurface;
                proto.ShadowCastingMode = template.castShadow;
                proto.ReceiveShadow = template.receiveShadow;
            }

            return prototypes;
        }

        private GGrassPrototypeGroup CreateGrassGroup(List<GGrassPrototype> prototypes)
        {
            GGrassPrototypeGroup grassGroup = ScriptableObject.CreateInstance<GGrassPrototypeGroup>();
            grassGroup.name = "~GeneratedGrassGroup";
            grassGroup.Prototypes = prototypes;
            return grassGroup;
        }

        private void ParseGrassInstances(List<GGrassInstance> instances, ComputeBuffer buffer, int minProtoIndex, int maxProtoIndex)
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
                if (t.isValid <= 0)
                    continue;
                GGrassInstance tree = new GGrassInstance();
                tree.Position = t.position;
                tree.Rotation = Quaternion.Euler(0, t.rotationY, 0);
                tree.Scale = new Vector3(t.horizontalScale, t.verticalScale, t.horizontalScale);
                if (minProtoIndex == maxProtoIndex)
                {
                    tree.PrototypeIndex = minProtoIndex;
                }
                else
                {
                    tree.PrototypeIndex = Random.Range(minProtoIndex, maxProtoIndex + 1);
                }

                instances.Add(tree);
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

            Vector3 terrainSize = terrain.TerrainData.Geometry.Size;
            for (int i = 0; i < instanceCount; ++i)
            {
                InstanceSample sample = samples[i];
                if (sample.isValid == 0)
                    continue;
                RaycastHit hit;
                Vector3 normalizedPoint = new Vector3(sample.position.x, 0, sample.position.z);
                Vector3 worldPosition;
                if (terrain.Raycast(normalizedPoint, out hit))
                {
                    worldPosition = hit.point;
                }
                else
                {
                    worldPosition = terrain.transform.TransformPoint(new Vector3(terrainSize.x * sample.position.x, 0, terrainSize.z * sample.position.z));
                }

                GameObject prefab;
                if (prefabCount == 1)
                {
                    prefab = prefabs[0];
                }
                else
                {
                    prefab = prefabs[Random.Range(0, prefabs.Count)];
                }

                Quaternion localRotation = Quaternion.Euler(0, sample.rotationY * Mathf.Rad2Deg, 0);
                Vector3 baseScale = prefab.transform.localScale;
                Vector3 localScale = new Vector3(sample.horizontalScale, sample.verticalScale, sample.horizontalScale);
                localScale.Scale(baseScale);

                if (template.alignToNormal)
                {
                    Vector3 normalVector = hit.normal;
                    float errorFactor = Random.Range(1 - template.normalAlignmentError, 1 + template.normalAlignmentError);
                    normalVector = Vector3.LerpUnclamped(Vector3.up, normalVector, errorFactor);
                    Quaternion alignmentRotation = Quaternion.FromToRotation(Vector3.up, normalVector);
                    localRotation *= alignmentRotation;
                }

                GameObject instance = SpawnUtilities.Spawn(prefab);
                instance.transform.parent = prefabRoot;
                instance.transform.position = worldPosition;
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

        private void SerializePrototypes()
        {
            if (terrain != null && terrain.TerrainData != null)
            {
                if (terrain.TerrainData.Shading.Splats != null)
                {
                    m_splatPrototypesSerialized = terrain.TerrainData.Shading.Splats.Prototypes;
                }
                if (terrain.TerrainData.Foliage.Trees != null)
                {
                    m_treePrototypesSerialized = terrain.TerrainData.Foliage.Trees.Prototypes;
                }
                if (terrain.TerrainData.Foliage.Grasses != null)
                {
                    m_grassPrototypesSerialized = terrain.TerrainData.Foliage.Grasses.Prototypes;
                }
            }
        }

        private void DeserializePrototypes()
        {
            if (terrain != null && terrain.TerrainData != null)
            {
                if (m_splatPrototypesSerialized != null)
                {
                    GSplatPrototypeGroup splatGroup = CreateSplatGroup(m_splatPrototypesSerialized);
                    terrain.TerrainData.Shading.Splats = splatGroup;
                }
                if (m_treePrototypesSerialized != null)
                {
                    GTreePrototypeGroup treeGroup = CreateTreeGroup(m_treePrototypesSerialized);
                    terrain.TerrainData.Foliage.Trees = treeGroup;
                }
                if (m_grassPrototypesSerialized != null)
                {
                    GGrassPrototypeGroup grassGroup = CreateGrassGroup(m_grassPrototypesSerialized);
                    terrain.TerrainData.Foliage.Grasses = grassGroup;
                }
            }
        }

        public void OnBeforeSerialize()
        {
            SerializePrototypes();
        }

        public void OnAfterDeserialize()
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

            Texture terrainHeightMap = terrain.TerrainData.Geometry.HeightMap;
            PolarisTileUtilities.DecodeAndDrawHeightMap(targetRt, terrainHeightMap, uvCorner);
        }
    }
}
#endif
#endif