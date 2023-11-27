#if VISTA
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public abstract class InstanceOutputNodeBase : ExecutableNodeBase, IOutputNode, IHasSeed
    {
        public readonly BufferSlot positionInputSlot = new BufferSlot("Positions", SlotDirection.Input, 0);
        public readonly MaskSlot densityInputSlot = new MaskSlot("Density", SlotDirection.Input, 1);
        public readonly MaskSlot verticalScaleInputSlot = new MaskSlot("Vertical Scale", SlotDirection.Input, 2);
        public readonly MaskSlot horizontalScaleInputSlot = new MaskSlot("Horizontal Scale", SlotDirection.Input, 3);
        public readonly MaskSlot rotationInputSlot = new MaskSlot("Rotation Y", SlotDirection.Input, 4);

        public readonly BufferSlot outputSlot = new BufferSlot("Output", SlotDirection.Output, 100);

        public SlotRef mainOutputSlot
        {
            get
            {
                return new SlotRef(m_id, outputSlot.id);
            }
        }

        [SerializeField]
        protected float m_densityMultiplier;
        public float densityMultiplier
        {
            get
            {
                return m_densityMultiplier;
            }
            set
            {
                m_densityMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected float m_verticalScaleMultiplier;
        public float verticalScaleMultiplier
        {
            get
            {
                return m_verticalScaleMultiplier;
            }
            set
            {
                m_verticalScaleMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected float m_horizontalScaleMultiplier;
        public float horizontalScaleMultiplier
        {
            get
            {
                return m_horizontalScaleMultiplier;
            }
            set
            {
                m_horizontalScaleMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        protected float m_minRotation;
        public float minRotation
        {
            get
            {
                return m_minRotation;
            }
            set
            {
                m_minRotation = Mathf.Min(value, m_maxRotation);
            }
        }

        [SerializeField]
        protected float m_maxRotation;
        public float maxRotation
        {
            get
            {
                return m_maxRotation;
            }
            set
            {
                m_maxRotation = Mathf.Max(value, m_minRotation);
            }
        }

        [SerializeField]
        protected float m_rotationMultiplier;
        public float rotationMultiplier
        {
            get
            {
                return m_rotationMultiplier;
            }
            set
            {
                m_rotationMultiplier = Mathf.Max(0, value);
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

        public override bool isBypassed
        {
            get
            {
                return false;
            }
            set
            {
                m_isBypassed = false;
            }
        }

        protected static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/InstanceOutput";
        protected static readonly int INSTANCE_SAMPLE = Shader.PropertyToID("_InstanceSamples");
        protected static readonly int POSITIONS = Shader.PropertyToID("_Positions");
        protected static readonly int DENSITY_MAP = Shader.PropertyToID("_DensityMap");
        protected static readonly int DENSITY_MULTIPLIER = Shader.PropertyToID("_DensityMultiplier");
        protected static readonly int VERTICAL_SCALE_MAP = Shader.PropertyToID("_VerticalScaleMap");
        protected static readonly int VERTICAL_SCALE_MULTIPLIER = Shader.PropertyToID("_VerticalScaleMultiplier");
        protected static readonly int HORIZONTAL_SCALE_MAP = Shader.PropertyToID("_HorizontalScaleMap");
        protected static readonly int HORIZONTAL_SCALE_MULTIPLIER = Shader.PropertyToID("_HorizontalScaleMultiplier");
        protected static readonly int ROTATION_MAP = Shader.PropertyToID("_RotationMap");
        protected static readonly int MIN_ROTATION = Shader.PropertyToID("_MinRotation");
        protected static readonly int MAX_ROTATION = Shader.PropertyToID("_MaxRotation");
        protected static readonly int ROTATION_MULTIPLIER = Shader.PropertyToID("_RotationMultiplier");
        protected static readonly int SEED = Shader.PropertyToID("_Seed");
        private static readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");

        private static readonly int THREAD_PER_GROUP = 8;
        private static readonly int MAX_THREAD_GROUP = 64000 / THREAD_PER_GROUP;

        protected static readonly int KERNEL_INDEX = 0;

        protected static readonly string HAS_DENSITY_MAP_KW = "HAS_DENSITY_MAP";
        protected static readonly string HAS_VERTICAL_SCALE_MAP_KW = "HAS_VERTICAL_SCALE_MAP";
        protected static readonly string HAS_HORIZONTAL_SCALE_MAP_KW = "HAS_HORIZONTAL_SCALE_MAP";
        protected static readonly string HAS_ROTATION_MAP_KW = "HAS_ROTATION_MAP";

        protected ComputeShader m_computeShader;

        public InstanceOutputNodeBase() : base()
        {
            m_densityMultiplier = 1;
            m_verticalScaleMultiplier = 1;
            m_horizontalScaleMultiplier = 1;
            m_minRotation = -360;
            m_maxRotation = 360;
            m_rotationMultiplier = 1;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef positionInputRefLink = context.GetInputLink(m_id, positionInputSlot.id);
            ComputeBuffer positionInputBuffer;
            if (positionInputRefLink.Equals(SlotRef.invalid))
            {
                return;
            }

            positionInputBuffer = context.GetBuffer(positionInputRefLink);
            if (positionInputBuffer == null)
                return;
            if (positionInputBuffer.count % PositionSample.SIZE != 0)
            {
                Debug.LogError($"Cannot parse position input buffer. Node id: {m_id}");
                context.ReleaseReference(positionInputRefLink);
                return;
            }

            SlotRef densityMapRefLink = context.GetInputLink(m_id, densityInputSlot.id);
            Texture densityMap = context.GetTexture(densityMapRefLink);

            SlotRef verticalScaleMapRefLink = context.GetInputLink(m_id, verticalScaleInputSlot.id);
            Texture verticalScaleMap = context.GetTexture(verticalScaleMapRefLink);

            SlotRef horizontalScaleMapRefLink = context.GetInputLink(m_id, horizontalScaleInputSlot.id);
            Texture horizontalScaleMap = context.GetTexture(horizontalScaleMapRefLink);

            SlotRef rotationMapRefLink = context.GetInputLink(m_id, rotationInputSlot.id);
            Texture rotationMap = context.GetTexture(rotationMapRefLink);

            int instanceCount = positionInputBuffer.count / PositionSample.SIZE;
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(instanceCount * InstanceSample.SIZE);
            ComputeBuffer treeSamplesBuffer = context.CreateBuffer(desc, outputRef);

            m_computeShader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
            m_computeShader.SetBuffer(KERNEL_INDEX, INSTANCE_SAMPLE, treeSamplesBuffer);
            m_computeShader.SetBuffer(KERNEL_INDEX, POSITIONS, positionInputBuffer);

            m_computeShader.SetFloat(DENSITY_MULTIPLIER, m_densityMultiplier);
            if (densityMap != null)
            {
                m_computeShader.SetTexture(KERNEL_INDEX, DENSITY_MAP, densityMap);
                m_computeShader.EnableKeyword(HAS_DENSITY_MAP_KW);
            }
            else
            {
                m_computeShader.DisableKeyword(HAS_DENSITY_MAP_KW);
            }

            m_computeShader.SetFloat(VERTICAL_SCALE_MULTIPLIER, m_verticalScaleMultiplier);
            if (verticalScaleMap != null)

            {
                m_computeShader.SetTexture(KERNEL_INDEX, VERTICAL_SCALE_MAP, verticalScaleMap);
                m_computeShader.EnableKeyword(HAS_VERTICAL_SCALE_MAP_KW);
            }
            else
            {
                m_computeShader.DisableKeyword(HAS_VERTICAL_SCALE_MAP_KW);
            }

            m_computeShader.SetFloat(HORIZONTAL_SCALE_MULTIPLIER, m_horizontalScaleMultiplier);
            if (horizontalScaleMap != null)
            {
                m_computeShader.SetTexture(KERNEL_INDEX, HORIZONTAL_SCALE_MAP, horizontalScaleMap);
                m_computeShader.EnableKeyword(HAS_HORIZONTAL_SCALE_MAP_KW);
            }
            else
            {
                m_computeShader.DisableKeyword(HAS_HORIZONTAL_SCALE_MAP_KW);
            }

            m_computeShader.SetFloat(MIN_ROTATION, m_minRotation * Mathf.Deg2Rad);
            m_computeShader.SetFloat(MAX_ROTATION, m_maxRotation * Mathf.Deg2Rad);
            m_computeShader.SetFloat(ROTATION_MULTIPLIER, m_rotationMultiplier);
            if (rotationMap != null)
            {
                m_computeShader.SetTexture(KERNEL_INDEX, ROTATION_MAP, rotationMap);
                m_computeShader.EnableKeyword(HAS_ROTATION_MAP_KW);
            }
            else
            {
                m_computeShader.DisableKeyword(HAS_ROTATION_MAP_KW);
            }

            int baseSeed = context.GetArg(Args.SEED).intValue;
            System.Random rnd = new System.Random(m_seed ^ baseSeed);
            m_computeShader.SetVector(SEED, new Vector4((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()));

            int totalThreadGroupX = (instanceCount + THREAD_PER_GROUP - 1) / THREAD_PER_GROUP;
            int iteration = (totalThreadGroupX + MAX_THREAD_GROUP - 1) / MAX_THREAD_GROUP;
            for (int i = 0; i < iteration; ++i)
            {
                int threadGroupX = Mathf.Min(MAX_THREAD_GROUP, totalThreadGroupX);
                totalThreadGroupX -= MAX_THREAD_GROUP;
                int baseIndex = i * MAX_THREAD_GROUP * THREAD_PER_GROUP;
                m_computeShader.SetInt(BASE_INDEX, baseIndex);
                m_computeShader.Dispatch(KERNEL_INDEX, threadGroupX, 1, 1);
            }

            context.ReleaseReference(positionInputRefLink);
            context.ReleaseReference(densityMapRefLink);
            context.ReleaseReference(verticalScaleMapRefLink);
            context.ReleaseReference(horizontalScaleMapRefLink);
            context.ReleaseReference(rotationMapRefLink);
            Resources.UnloadAsset(m_computeShader);
        }
    }
}
#endif
