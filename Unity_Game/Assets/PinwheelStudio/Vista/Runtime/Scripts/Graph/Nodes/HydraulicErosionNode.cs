#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Hydraulic Erosion",
        path = "Nature/Hydraulic Erosion",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.yof3oeuykzqe",
        keywords = "hydraulic, erosion, water, flow, rain, weather",
        description = "Adding realism to the terrain by applying erosion caused by water flow.\nBest practice: Don't use just one node for this effect, instead chain many of them with Detail Level from low to high to have erosion effect at different scales.\nLow Detail Level may introduce some pixel artifacts but it can be hidden with later nodes.")]
    public class HydraulicErosionNode : ImageNodeBase
    {
        public readonly MaskSlot inputHeightSlot = new MaskSlot("Height", SlotDirection.Input, 0);
        public readonly MaskSlot waterMaskSlot = new MaskSlot("Water Mask", SlotDirection.Input, 1);
        public readonly MaskSlot hardnessSlot = new MaskSlot("Hardness", SlotDirection.Input, 2);
        public readonly MaskSlot detailHeightSlot = new MaskSlot("Detail Height", SlotDirection.Input, 3);
        public readonly MaskSlot bedRockDepthSlot = new MaskSlot("Bedrock Depth", SlotDirection.Input, 4);

        public readonly MaskSlot outputHeightSlot = new MaskSlot("Height", SlotDirection.Output, 100);
        public readonly MaskSlot erosionSlot = new MaskSlot("Erosion", SlotDirection.Output, 101);
        public readonly MaskSlot depositionSlot = new MaskSlot("Deposition", SlotDirection.Output, 102);
        public readonly MaskSlot waterSlot = new MaskSlot("Water", SlotDirection.Output, 103);

        [SerializeField]
        private float m_detailLevel;
        public float detailLevel
        {
            get
            {
                return m_detailLevel;
            }
            set
            {
                m_detailLevel = Mathf.Clamp(value, 0.1f, 1f);
            }
        }

        [SerializeField]
        private int m_iterationCount;
        public int iterationCount
        {
            get
            {
                return m_iterationCount;
            }
            set
            {
                m_iterationCount = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private int m_iterationPerFrame;
        public int iterationPerFrame
        {
            get
            {
                return Mathf.Max(1, m_iterationPerFrame);
            }
            set
            {
                m_iterationPerFrame = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private bool m_highQualityMode;
        public bool highQualityMode
        {
            get
            {
                return m_highQualityMode;
            }
            set
            {
                m_highQualityMode = value;
            }
        }

        [SerializeField]
        private float m_waterSourceAmount;
        public float waterSourceAmount
        {
            get
            {
                return m_waterSourceAmount;
            }
            set
            {
                m_waterSourceAmount = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_waterSourceMultiplier;
        public float waterSourceMultiplier
        {
            get
            {
                return m_waterSourceMultiplier;
            }
            set
            {
                m_waterSourceMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_flowRate;
        public float flowRate
        {
            get
            {
                return m_flowRate;
            }
            set
            {
                m_flowRate = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_flowMultiplier;
        public float flowMultiplier
        {
            get
            {
                return m_flowMultiplier;
            }
            set
            {
                m_flowMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_sedimentCapacity;
        public float sedimentCapacity
        {
            get
            {
                return m_sedimentCapacity;
            }
            set
            {
                m_sedimentCapacity = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_sedimentCapacityMultiplier;
        public float sedimentCapacityMultiplier
        {
            get
            {
                return m_sedimentCapacityMultiplier;
            }
            set
            {
                m_sedimentCapacityMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_erosionRate;
        public float erosionRate
        {
            get
            {
                return m_erosionRate;
            }
            set
            {
                m_erosionRate = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_erosionMultiplier;
        public float erosionMultiplier
        {
            get
            {
                return m_erosionMultiplier;
            }
            set
            {
                m_erosionMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_depositionRate;
        public float depositionRate
        {
            get
            {
                return m_depositionRate;
            }
            set
            {
                m_depositionRate = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_depositionMultiplier;
        public float depositionMultiplier
        {
            get
            {
                return m_depositionMultiplier;
            }
            set
            {
                m_depositionMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_depositionSlopeFactor;
        public float depositionSlopeFactor
        {
            get
            {
                return m_depositionSlopeFactor;
            }
            set
            {
                m_depositionSlopeFactor = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_evaporationRate;
        public float evaporationRate
        {
            get
            {
                return m_evaporationRate;
            }
            set
            {
                m_evaporationRate = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_evaporationMultiplier;
        public float evaporationMultiplier
        {
            get
            {
                return m_evaporationMultiplier;
            }
            set
            {
                m_evaporationMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_bedRockDepth;
        public float bedRockDepth
        {
            get
            {
                return m_bedRockDepth;
            }
            set
            {
                m_bedRockDepth = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_sharpness;
        public float sharpness
        {
            get
            {
                return m_sharpness;
            }
            set
            {
                m_sharpness = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float m_heightScale;
        public float heightScale
        {
            get
            {
                return m_heightScale;
            }
            set
            {
                m_heightScale = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_detailHeightScale;
        public float detailHeightScale
        {
            get
            {
                return m_detailHeightScale;
            }
            set
            {
                m_detailHeightScale = value;
            }
        }

        [SerializeField]
        private float m_erosionBoost;
        public float erosionBoost
        {
            get
            {
                return m_erosionBoost;
            }
            set
            {
                m_erosionBoost = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float m_depositionBoost;
        public float depositionBoost
        {
            get
            {
                return m_depositionBoost;
            }
            set
            {
                m_depositionBoost = Mathf.Max(0, value);
            }
        }

        private Material m_materialHelper;
        private static readonly string HELPER_SHADER_NAME = "Hidden/Vista/Graph/ErosionHelper";
        private static readonly int HEIGHT_MAP = Shader.PropertyToID("_HeightMap");
        private static readonly int WATER_SOURCE_MAP = Shader.PropertyToID("_WaterSourceMap");
        private static readonly int HARDNESS_MAP = Shader.PropertyToID("_HardnessMap");
        private static readonly int DETAIL_HEIGHT_MAP = Shader.PropertyToID("_DetailHeightMap");
        private static readonly int BED_ROCK_DEPTH_MAP = Shader.PropertyToID("_BedRockDepthMap");

        private static readonly int BOUNDS = Shader.PropertyToID("_Bounds");
        private static readonly int TERRAIN_POS = Shader.PropertyToID("_TerrainPos");
        private static readonly int TEXTURE_SIZE = Shader.PropertyToID("_TextureSize");
        private static readonly int HEIGHT_SCALE = Shader.PropertyToID("_HeightScale");
        private static readonly int DETAIL_HEIGHT_SCALE = Shader.PropertyToID("_DetailHeightScale");
        private static readonly int EROSION_BOOST = Shader.PropertyToID("_ErosionBoost");
        private static readonly int DEPOSITION_BOOST = Shader.PropertyToID("_DepositionBoost");

        private static readonly int PASS_INIT_WORLD_DATA = 0;
        private static readonly int PASS_INIT_MASK = 1;
        private static readonly int PASS_OUTPUT_HEIGHT = 2;
        private static readonly int PASS_OUTPUT_EROSION = 3;
        private static readonly int PASS_OUTPUT_DEPOSITION = 4;
        private static readonly int PASS_OUTPUT_WATER = 5;

        private ComputeShader m_simulationShader;
        private static readonly string SIM_SHADER_NAME = "Vista/Shaders/Graph/HydraulicErosion";
        private static readonly int MASK_MAP = Shader.PropertyToID("_MaskMap");
        private static readonly int WATER_SOURCE_AMOUNT = Shader.PropertyToID("_WaterSourceAmount");
        private static readonly int FLOW_RATE = Shader.PropertyToID("_FlowRate");
        private static readonly int SEDIMENT_CAPACITY = Shader.PropertyToID("_SedimentCapacity");
        private static readonly int EROSION_RATE = Shader.PropertyToID("_ErosionRate");
        private static readonly int DEPOSITION_RATE = Shader.PropertyToID("_DepositionRate");
        private static readonly int DEPOSITION_SLOPE_FACTOR = Shader.PropertyToID("_DepositionSlopeFactor");
        private static readonly int EVAPORATION_RATE = Shader.PropertyToID("_EvaporationRate");
        private static readonly int BED_ROCK_DEPTH = Shader.PropertyToID("_BedRockDepth");
        private static readonly int SHARPNESS = Shader.PropertyToID("_Sharpness");

        private static readonly int SIM_DATA_RESOLUTION = Shader.PropertyToID("_SimDataResolution");
        private static readonly int WORLD_DATA = Shader.PropertyToID("_WorldData");
        private static readonly int HEIGHT_CHANGE_DATA = Shader.PropertyToID("_HeightChangeData");
        private static readonly int OUTFLOW_VH_DATA = Shader.PropertyToID("_OutflowVHData");
        private static readonly int OUTFLOW_DIAG_DATA = Shader.PropertyToID("_OutflowDiagData");
        private static readonly int VELOCITY_DATA = Shader.PropertyToID("_VelocityData");
        private static readonly int KERNEL_INDEX_SIMULATE = 0;
        private static readonly int KERNEL_INDEX_POST_PROCESS = 1;

        private static readonly string TEX_NAME_WORLD_DATA = "WorldData";
        private static readonly string TEX_NAME_MASK = "Mask";
        private static readonly string TEX_NAME_OUTFLOW_VH = "OutflowVH";
        private static readonly string TEX_NAME_OUTFLOW_DIAG = "OutflowDiag";
        private static readonly string TEX_NAME_VELOCITY = "Velocity";
        private static readonly string TEX_NAME_HEIGHT_CHANGE = "HeightChange";

        private static readonly string KW_HIGH_QUALITY = "HIGH_QUALITY";
        private static readonly string KW_HAS_MASK = "HAS_MASK";

        private struct SimulationTextures
        {
            public int resolution { get; set; }
            public Texture inputHeightTexture { get; set; }
            public RenderTexture worldData { get; set; }
            public RenderTexture maskData { get; set; }
            public RenderTexture outflowVHData { get; set; }
            public RenderTexture outflowDiagData { get; set; }
            public RenderTexture velocityData { get; set; }
            public RenderTexture heightChangeData { get; set; }

            public void ClearReferences()
            {
                inputHeightTexture = null;
                worldData = null;
                maskData = null;
                outflowVHData = null;
                outflowDiagData = null;
                velocityData = null;
                heightChangeData = null;
            }
        }

        public HydraulicErosionNode() : base()
        {
            m_shouldSplitExecution = true;

            m_highQualityMode = true;
            m_detailLevel = 0.25f;
            m_iterationCount = 30;
            m_iterationPerFrame = 1;

            m_waterSourceAmount = 0.5f;
            m_waterSourceMultiplier = 1f;

            m_flowRate = 1f;
            m_flowMultiplier = 1f;

            m_sedimentCapacity = 5f;
            m_sedimentCapacityMultiplier = 1f;

            m_erosionRate = 1f;
            m_erosionMultiplier = 1;

            m_depositionRate = 1f;
            m_depositionMultiplier = 1f;
            m_depositionSlopeFactor = 1f;

            m_evaporationRate = 0.1f;
            m_evaporationMultiplier = 1f;

            m_bedRockDepth = 10f;
            m_sharpness = 0.5f;

            m_heightScale = 1f;
            m_detailHeightScale = 1f;
            m_erosionBoost = 1f;
            m_depositionBoost = 1f;
        }

        private SimulationTextures CreateSimulationTextures(GraphContext context, int simPass, float detailLevelMultiplier)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;

            SlotRef inputHeightRefLink = context.GetInputLink(m_id, inputHeightSlot.id);
            Texture inputHeightTexture = context.GetTexture(inputHeightRefLink);
            int inputResolution;
            if (inputHeightTexture == null)
            {
                inputHeightTexture = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = inputHeightTexture.width;
            }

            SlotRef waterSourceRefLink = context.GetInputLink(m_id, waterMaskSlot.id);
            Texture waterSourceTexture = context.GetTexture(waterSourceRefLink);

            SlotRef hardnessRefLink = context.GetInputLink(m_id, hardnessSlot.id);
            Texture hardnessTexture = context.GetTexture(hardnessRefLink);

            SlotRef detailHeightRefLink = context.GetInputLink(m_id, detailHeightSlot.id);
            Texture detailHeightTexture = context.GetTexture(detailHeightRefLink);
            if (detailHeightTexture == null)
            {
                detailHeightTexture = Texture2D.blackTexture;
            }

            SlotRef bedRockDepthRefLink = context.GetInputLink(m_id, bedRockDepthSlot.id);
            Texture bedRockDepthTexture = context.GetTexture(bedRockDepthRefLink);
            if (bedRockDepthTexture == null)
            {
                bedRockDepthTexture = Texture2D.whiteTexture;
            }

            if (m_materialHelper == null)
            {
                m_materialHelper = new Material(ShaderUtilities.Find(HELPER_SHADER_NAME));
            }
            m_materialHelper.SetTexture(HEIGHT_MAP, inputHeightTexture);

            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            float terrainHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
            Vector3 bounds = new Vector3(worldBounds.z, terrainHeight, worldBounds.w);
            m_materialHelper.SetVector(BOUNDS, bounds);
            m_materialHelper.SetFloat(HEIGHT_SCALE, m_heightScale);
            m_materialHelper.SetFloat(EROSION_BOOST, m_erosionBoost);
            m_materialHelper.SetFloat(DEPOSITION_BOOST, m_depositionBoost);

            SimulationTextures textures = new SimulationTextures();
            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            textures.resolution = resolution;
            textures.inputHeightTexture = inputHeightTexture;

            float areaSize = bounds.x;
            int simDataResolution = Utilities.MultipleOf8(Mathf.CeilToInt(m_detailLevel * detailLevelMultiplier * areaSize));
            simDataResolution = Mathf.Min(baseResolution, simDataResolution);
            m_materialHelper.SetVector(TEXTURE_SIZE, new Vector2(simDataResolution, simDataResolution));
            m_materialHelper.SetTexture(DETAIL_HEIGHT_MAP, detailHeightTexture);
            m_materialHelper.SetFloat(DETAIL_HEIGHT_SCALE, m_detailHeightScale);
            m_materialHelper.SetTexture(BED_ROCK_DEPTH_MAP, bedRockDepthTexture);
            m_materialHelper.SetFloat(BED_ROCK_DEPTH, m_bedRockDepth);
            DataPool.RtDescriptor worldDataDesc = DataPool.RtDescriptor.Create(simDataResolution, simDataResolution, RenderTextureFormat.ARGBFloat);
            RenderTexture worldData = context.CreateTemporaryRT(worldDataDesc, TEX_NAME_WORLD_DATA + simPass + m_id);
            Drawing.DrawQuad(worldData, m_materialHelper, PASS_INIT_WORLD_DATA);
            textures.worldData = worldData;

            if (waterSourceTexture != null || hardnessTexture != null)
            {
                if (waterSourceTexture == null)
                    waterSourceTexture = Texture2D.whiteTexture;
                if (hardnessTexture == null)
                    hardnessTexture = Texture2D.blackTexture;
                m_materialHelper.SetTexture(WATER_SOURCE_MAP, waterSourceTexture);
                m_materialHelper.SetTexture(HARDNESS_MAP, hardnessTexture);

                DataPool.RtDescriptor maskDesc = DataPool.RtDescriptor.Create(simDataResolution, simDataResolution, RenderTextureFormat.RGFloat);
                RenderTexture maskData = context.CreateTemporaryRT(maskDesc, TEX_NAME_MASK + simPass + m_id);
                Drawing.DrawQuad(maskData, m_materialHelper, PASS_INIT_MASK);
                textures.maskData = maskData;
            }

            DataPool.RtDescriptor outflowVHDesc = DataPool.RtDescriptor.Create(simDataResolution, simDataResolution, RenderTextureFormat.ARGBFloat);
            RenderTexture outflowVHData = context.CreateTemporaryRT(outflowVHDesc, TEX_NAME_OUTFLOW_VH + simPass + m_id);
            textures.outflowVHData = outflowVHData;

            if (highQualityMode)
            {
                DataPool.RtDescriptor outflowDiagDesc = DataPool.RtDescriptor.Create(simDataResolution, simDataResolution, RenderTextureFormat.ARGBFloat);
                RenderTexture outflowDiagData = context.CreateTemporaryRT(outflowDiagDesc, TEX_NAME_OUTFLOW_DIAG + simPass + m_id);
                textures.outflowDiagData = outflowDiagData;
            }

            DataPool.RtDescriptor velocityDesc = DataPool.RtDescriptor.Create(simDataResolution, simDataResolution, RenderTextureFormat.RGFloat);
            RenderTexture velocityData = context.CreateTemporaryRT(velocityDesc, TEX_NAME_VELOCITY + simPass + m_id);
            textures.velocityData = velocityData;

            DataPool.RtDescriptor heightChangeDesc = DataPool.RtDescriptor.Create(simDataResolution, simDataResolution, RenderTextureFormat.RGFloat);
            RenderTexture heightChangeData = context.CreateTemporaryRT(heightChangeDesc, TEX_NAME_HEIGHT_CHANGE + simPass + m_id);
            Drawing.Blit(Texture2D.blackTexture, heightChangeData);
            textures.heightChangeData = heightChangeData;

            return textures;
        }

        private void InitSimulationShader(GraphContext context, SimulationTextures textures)
        {
            if (m_simulationShader == null)
            {
                m_simulationShader = Resources.Load<ComputeShader>(SIM_SHADER_NAME);
            }
            m_simulationShader.SetVector(SIM_DATA_RESOLUTION, Vector2.one * textures.worldData.width);
            m_simulationShader.SetTexture(KERNEL_INDEX_SIMULATE, WORLD_DATA, textures.worldData);
            m_simulationShader.SetTexture(KERNEL_INDEX_SIMULATE, HEIGHT_CHANGE_DATA, textures.heightChangeData);
            m_simulationShader.SetTexture(KERNEL_INDEX_SIMULATE, OUTFLOW_VH_DATA, textures.outflowVHData);
            if (highQualityMode)
            {
                m_simulationShader.SetTexture(KERNEL_INDEX_SIMULATE, OUTFLOW_DIAG_DATA, textures.outflowDiagData);
                m_simulationShader.EnableKeyword(KW_HIGH_QUALITY);
            }
            else
            {
                m_simulationShader.DisableKeyword(KW_HIGH_QUALITY);
            }
            m_simulationShader.SetTexture(KERNEL_INDEX_SIMULATE, VELOCITY_DATA, textures.velocityData);
            if (textures.maskData != null)
            {
                m_simulationShader.SetTexture(KERNEL_INDEX_SIMULATE, MASK_MAP, textures.maskData);
                m_simulationShader.EnableKeyword(KW_HAS_MASK);
            }
            else
            {
                m_simulationShader.DisableKeyword(KW_HAS_MASK);
            }

            m_simulationShader.SetTexture(KERNEL_INDEX_POST_PROCESS, WORLD_DATA, textures.worldData);
            m_simulationShader.SetTexture(KERNEL_INDEX_POST_PROCESS, HEIGHT_CHANGE_DATA, textures.heightChangeData);

            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            float terrainHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
            Vector3 bounds = new Vector3(worldBounds.z, terrainHeight, worldBounds.w);
            m_simulationShader.SetVector(BOUNDS, bounds);
            m_simulationShader.SetVector(TERRAIN_POS, new Vector4(worldBounds.x, 0, worldBounds.y, 0));

            float wt = m_waterSourceAmount * m_waterSourceMultiplier;
            float fr = m_flowRate * m_flowMultiplier;
            float sc = m_sedimentCapacity * m_sedimentCapacityMultiplier;
            float er = m_erosionRate * m_erosionMultiplier;
            float dr = m_depositionRate * m_depositionMultiplier;
            float ev = m_evaporationRate * m_evaporationMultiplier;

            m_simulationShader.SetFloat(WATER_SOURCE_AMOUNT, wt);
            m_simulationShader.SetFloat(FLOW_RATE, fr);
            m_simulationShader.SetFloat(SEDIMENT_CAPACITY, sc);
            m_simulationShader.SetFloat(EROSION_RATE, er);
            m_simulationShader.SetFloat(DEPOSITION_RATE, dr);
            m_simulationShader.SetFloat(DEPOSITION_SLOPE_FACTOR, m_depositionSlopeFactor);
            m_simulationShader.SetFloat(EVAPORATION_RATE, ev);
            m_simulationShader.SetFloat(SHARPNESS, m_sharpness);
        }

        private void Dispatch(int kernel, int dimX, int dimZ)
        {
            int threadGroupX = (dimX + 7) / 8;
            int threadGroupY = 1;
            int threadGroupZ = (dimZ + 7) / 8;

            m_simulationShader.Dispatch(kernel, threadGroupX, threadGroupY, threadGroupZ);
        }

        private void Output(GraphContext context, SimulationTextures textures)
        {
            Vector4 worldBounds = context.GetArg(Args.WORLD_BOUNDS).vectorValue;
            float terrainHeight = context.GetArg(Args.TERRAIN_HEIGHT).floatValue;
            Vector3 bounds = new Vector3(worldBounds.z, terrainHeight, worldBounds.w);
            SlotRef outputHeightRef = new SlotRef(m_id, outputHeightSlot.id);
            if (context.GetReferenceCount(outputHeightRef) > 0 || context.IsTargetNode(m_id))
            {
                DataPool.RtDescriptor outputHeightDesc = DataPool.RtDescriptor.Create(textures.resolution, textures.resolution, RenderTextureFormat.RFloat);
                RenderTexture outputHeightRt = context.CreateRenderTarget(outputHeightDesc, outputHeightRef);
                m_materialHelper.SetTexture(HEIGHT_MAP, textures.inputHeightTexture);
                m_materialHelper.SetTexture(HEIGHT_CHANGE_DATA, textures.heightChangeData);
                m_materialHelper.SetVector(BOUNDS, bounds);
                Drawing.DrawQuad(outputHeightRt, m_materialHelper, PASS_OUTPUT_HEIGHT);
            }

            SlotRef outputErosionRef = new SlotRef(m_id, erosionSlot.id);
            if (context.GetReferenceCount(outputErosionRef) > 0)
            {
                DataPool.RtDescriptor outputErosionDesc = DataPool.RtDescriptor.Create(textures.resolution, textures.resolution, RenderTextureFormat.RFloat);
                RenderTexture outputErosionRt = context.CreateRenderTarget(outputErosionDesc, outputErosionRef);
                m_materialHelper.SetTexture(HEIGHT_CHANGE_DATA, textures.heightChangeData);
                m_materialHelper.SetVector(BOUNDS, bounds);
                Drawing.DrawQuad(outputErosionRt, m_materialHelper, PASS_OUTPUT_EROSION);
            }

            SlotRef outputDepositionRef = new SlotRef(m_id, depositionSlot.id);
            if (context.GetReferenceCount(outputDepositionRef) > 0)
            {
                DataPool.RtDescriptor outputDepositionDesc = DataPool.RtDescriptor.Create(textures.resolution, textures.resolution, RenderTextureFormat.RFloat);
                RenderTexture outputDepositionRt = context.CreateRenderTarget(outputDepositionDesc, outputDepositionRef);
                m_materialHelper.SetTexture(HEIGHT_CHANGE_DATA, textures.heightChangeData);
                m_materialHelper.SetVector(BOUNDS, bounds);
                Drawing.DrawQuad(outputDepositionRt, m_materialHelper, PASS_OUTPUT_DEPOSITION);
            }

            SlotRef outputWaterRef = new SlotRef(m_id, waterSlot.id);
            if (context.GetReferenceCount(outputWaterRef) > 0)
            {
                DataPool.RtDescriptor outputWaterDesc = DataPool.RtDescriptor.Create(textures.resolution, textures.resolution, RenderTextureFormat.RFloat);
                RenderTexture outputWaterRt = context.CreateRenderTarget(outputWaterDesc, outputWaterRef);
                m_materialHelper.SetTexture(WORLD_DATA, textures.worldData);
                m_materialHelper.SetVector(BOUNDS, bounds);
                Drawing.DrawQuad(outputWaterRt, m_materialHelper, PASS_OUTPUT_WATER);
            }
        }

        private void CopySimulationData(SimulationTextures src, SimulationTextures dest)
        {
            Drawing.Blit(src.worldData, dest.worldData);
            Drawing.Blit(src.outflowVHData, dest.outflowVHData);
            Drawing.Blit(src.velocityData, dest.velocityData);
            Drawing.Blit(src.heightChangeData, dest.heightChangeData);

            if (src.outflowDiagData != null && dest.outflowDiagData != null)
            {
                Drawing.Blit(src.outflowDiagData, dest.outflowDiagData);
            }
        }

        private void CleanUp(GraphContext context)
        {
            SlotRef inputHeightRefLink = context.GetInputLink(m_id, inputHeightSlot.id);
            context.ReleaseReference(inputHeightRefLink);

            SlotRef waterSourceRefLink = context.GetInputLink(m_id, waterMaskSlot.id);
            context.ReleaseReference(waterSourceRefLink);

            SlotRef hardnessRefLink = context.GetInputLink(m_id, hardnessSlot.id);
            context.ReleaseReference(hardnessRefLink);

            SlotRef detailHeightRefLink = context.GetInputLink(m_id, detailHeightSlot.id);
            context.ReleaseReference(detailHeightRefLink);

            SlotRef bedRockDepthRefLink = context.GetInputLink(m_id, bedRockDepthSlot.id);
            context.ReleaseReference(bedRockDepthRefLink);

            CleanUpSimulationTextures(context, 0);
            CleanUpSimulationTextures(context, 1);
            CleanUpSimulationTextures(context, 2);

            Object.DestroyImmediate(m_materialHelper);
            Resources.UnloadAsset(m_simulationShader);
        }

        private void CleanUpSimulationTextures(GraphContext context, int simPass)
        {
            context.ReleaseTemporary(TEX_NAME_WORLD_DATA + simPass + m_id);
            context.ReleaseTemporary(TEX_NAME_MASK + simPass + m_id);
            context.ReleaseTemporary(TEX_NAME_OUTFLOW_VH + simPass + m_id);
            context.ReleaseTemporary(TEX_NAME_OUTFLOW_DIAG + simPass + m_id);
            context.ReleaseTemporary(TEX_NAME_VELOCITY + simPass + m_id);
            context.ReleaseTemporary(TEX_NAME_HEIGHT_CHANGE + simPass + m_id);
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int firstPassIterationCount = Mathf.Max(1, m_iterationCount / 3);
            int secondPassIterationCount = Mathf.Max(0, Mathf.Min(firstPassIterationCount, m_iterationCount - firstPassIterationCount));
            int thirdPassIterationCount = Mathf.Max(0, m_iterationCount - secondPassIterationCount - firstPassIterationCount);

            SimulationTextures outputTextures;
            SimulationTextures textures0 = CreateSimulationTextures(context, 0, 0.35f);
            outputTextures = textures0;

            InitSimulationShader(context, textures0);
            int dimX = textures0.worldData.width;
            int dimZ = textures0.worldData.height;
            for (int i = 0; i < firstPassIterationCount; ++i)
            {
                Dispatch(KERNEL_INDEX_SIMULATE, dimX, dimZ);
                if (i % 5 == 0)
                {
                    Dispatch(KERNEL_INDEX_POST_PROCESS, dimX, dimZ);
                }
            }

            if (secondPassIterationCount > 0)
            {
                SimulationTextures textures1 = CreateSimulationTextures(context, 1, 0.65f);
                CopySimulationData(textures0, textures1);
                outputTextures = textures1;

                InitSimulationShader(context, textures1);
                dimX = textures1.worldData.width;
                dimZ = textures1.worldData.height;
                for (int i = 0; i < secondPassIterationCount; ++i)
                {
                    Dispatch(KERNEL_INDEX_SIMULATE, dimX, dimZ);
                    if (i % 5 == 0)
                    {
                        Dispatch(KERNEL_INDEX_POST_PROCESS, dimX, dimZ);
                    }
                }

                if (thirdPassIterationCount > 0)
                {
                    SimulationTextures textures2 = CreateSimulationTextures(context, 2, 1f);
                    CopySimulationData(textures1, textures2);
                    outputTextures = textures2;

                    InitSimulationShader(context, textures2);
                    dimX = textures2.worldData.width;
                    dimZ = textures2.worldData.height;
                    for (int i = 0; i < thirdPassIterationCount; ++i)
                    {
                        Dispatch(KERNEL_INDEX_SIMULATE, dimX, dimZ);
                        if (i % 5 == 0)
                        {
                            Dispatch(KERNEL_INDEX_POST_PROCESS, dimX, dimZ);
                        }
                    }
                }
            }

            Output(context, outputTextures);
            CleanUp(context);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            int firstPassIterationCount = Mathf.Max(1, m_iterationCount / 3);
            int secondPassIterationCount = Mathf.Max(0, Mathf.Min(firstPassIterationCount, m_iterationCount - firstPassIterationCount));
            int thirdPassIterationCount = Mathf.Max(0, m_iterationCount - secondPassIterationCount - firstPassIterationCount);
            int currentIteration = 0;

            SimulationTextures outputTextures;
            SimulationTextures textures0 = CreateSimulationTextures(context, 0, 0.35f);
            outputTextures = textures0;

            InitSimulationShader(context, textures0);
            int dimX = textures0.worldData.width;
            int dimZ = textures0.worldData.height;
            for (int i = 0; i < firstPassIterationCount; ++i)
            {
                Dispatch(KERNEL_INDEX_SIMULATE, dimX, dimZ);
                if (i % 5 == 0)
                {
                    Dispatch(KERNEL_INDEX_POST_PROCESS, dimX, dimZ);
                }
                if (i % iterationPerFrame == 0 && shouldSplitExecution)
                {
                    context.SetCurrentProgress(currentIteration * 1.0f / m_iterationCount);
                    yield return null;
                }
                currentIteration += 1;
            }

            if (secondPassIterationCount > 0)
            {
                SimulationTextures textures1 = CreateSimulationTextures(context, 1, 0.65f);
                CopySimulationData(textures0, textures1);
                outputTextures = textures1;

                InitSimulationShader(context, textures1);
                dimX = textures1.worldData.width;
                dimZ = textures1.worldData.height;
                for (int i = 0; i < secondPassIterationCount; ++i)
                {
                    Dispatch(KERNEL_INDEX_SIMULATE, dimX, dimZ);
                    if (i % 5 == 0)
                    {
                        Dispatch(KERNEL_INDEX_POST_PROCESS, dimX, dimZ);
                    }
                    if (i % iterationPerFrame == 0 && shouldSplitExecution)
                    {
                        context.SetCurrentProgress(currentIteration * 1.0f / m_iterationCount);
                        yield return null;
                    }
                    currentIteration += 1;
                }

                if (thirdPassIterationCount > 0)
                {
                    SimulationTextures textures2 = CreateSimulationTextures(context, 2, 1f);
                    CopySimulationData(textures1, textures2);
                    outputTextures = textures2;

                    InitSimulationShader(context, textures2);
                    dimX = textures2.worldData.width;
                    dimZ = textures2.worldData.height;
                    for (int i = 0; i < thirdPassIterationCount; ++i)
                    {
                        Dispatch(KERNEL_INDEX_SIMULATE, dimX, dimZ);
                        if (i % 5 == 0)
                        {
                            Dispatch(KERNEL_INDEX_POST_PROCESS, dimX, dimZ);
                        }
                        if (i % iterationPerFrame == 0 && shouldSplitExecution)
                        {
                            context.SetCurrentProgress(currentIteration * 1.0f / m_iterationCount);
                            yield return null;
                        }
                        currentIteration += 1;
                    }
                }
            }

            Output(context, outputTextures);
            CleanUp(context);

            yield return null;
        }
    }
}
#endif
