#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Offset",
        path = "General/Offset",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.pk2a268itps6",
        keywords = "",
        description = "Offset position samples in the direction defined by the direction map. The direction map will be converted to a normal map to determine the offset vector.")]
    public class OffsetNode : ExecutableNodeBase
    {
        public readonly BufferSlot inputSlot = new BufferSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot directionSlot = new MaskSlot("Direction", SlotDirection.Input, 1);
        public readonly MaskSlot intensitySlot = new MaskSlot("Intensity", SlotDirection.Input, 2);

        public readonly BufferSlot outputSlot = new BufferSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_minAngle;
        public float minAngle
        {
            get
            {
                return m_minAngle;
            }
            set
            {
                m_minAngle = Mathf.Clamp(Mathf.Min(value, m_maxAngle), -360f, 360f);
            }
        }

        [SerializeField]
        private float m_maxAngle;
        public float maxAngle
        {
            get
            {
                return m_maxAngle;
            }
            set
            {
                m_maxAngle = Mathf.Clamp(Mathf.Max(value, m_minAngle), -360f, 360f);
            }
        }

        [SerializeField]
        private float m_angleMultiplier;
        public float angleMultiplier
        {
            get
            {
                return m_angleMultiplier;
            }
            set
            {
                m_angleMultiplier = value;
            }
        }

        [SerializeField]
        private float m_intensityMultiplier;
        public float intensityMultiplier
        {
            get
            {
                return m_intensityMultiplier;
            }
            set
            {
                m_intensityMultiplier = value;
            }
        }

        private static readonly string SHADER_NAME = "Vista/Shaders/Graph/Offset";
        private static readonly int SOURCE_BUFFER = Shader.PropertyToID("_SrcBuffer");
        private static readonly int DEST_BUFFER = Shader.PropertyToID("_DestBuffer");
        private static readonly int DIRECTION_MAP = Shader.PropertyToID("_DirectionMap");
        private static readonly int INTENSITY_MAP = Shader.PropertyToID("_IntensityMap");
        private static readonly int MIN_ANGLE = Shader.PropertyToID("_MinAngle");
        private static readonly int MAX_ANGLE = Shader.PropertyToID("_MaxAngle");
        private static readonly int ANGLE_MULTIPLIER = Shader.PropertyToID("_AngleMultiplier");
        private static readonly int INTENSITY_MULTIPLIER = Shader.PropertyToID("_IntensityMultiplier");
        private static readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");

        private static readonly string KW_HAS_DIRECTION_MAP = "HAS_DIRECTION_MAP";
        private static readonly string KW_HAS_INTENSITY_MAP = "HAS_INTENSITY_MAP";

        private static readonly int KERNEL = 0;

        private static readonly int THREAD_PER_GROUP = 8;
        private static readonly int MAX_THREAD_GROUP = 64000 / THREAD_PER_GROUP;

        private ComputeShader m_shader;

        public OffsetNode() : base()
        {
            m_minAngle = -360f;
            m_maxAngle = 360;
            m_angleMultiplier = 1f;
            m_intensityMultiplier = 0f;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            SlotRef inputBufferRefLink = context.GetInputLink(m_id, inputSlot.id);
            ComputeBuffer inputBuffer = context.GetBuffer(inputBufferRefLink);
            if (inputBuffer == null)
            {
                return;
            }
            if (inputBuffer.count % PositionSample.SIZE != 0)
            {
                Debug.LogError($"Unable to parse buffer {inputSlot.name}, node id {m_id}");
                return;
            }

            SlotRef directionRefLink = context.GetInputLink(m_id, directionSlot.id);
            Texture directionMap = context.GetTexture(directionRefLink);

            SlotRef intensityRefLink = context.GetInputLink(m_id, intensitySlot.id);
            Texture intensityMap = context.GetTexture(intensityRefLink);

            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(inputBuffer.count);
            ComputeBuffer outputBuffer = context.CreateBuffer(desc, outputRef);

            m_shader = Resources.Load<ComputeShader>(SHADER_NAME);
            m_shader.SetBuffer(KERNEL, SOURCE_BUFFER, inputBuffer);
            m_shader.SetBuffer(KERNEL, DEST_BUFFER, outputBuffer);
            m_shader.SetFloat(MIN_ANGLE, m_minAngle * Mathf.Deg2Rad);
            m_shader.SetFloat(MAX_ANGLE, m_maxAngle * Mathf.Deg2Rad);
            m_shader.SetFloat(ANGLE_MULTIPLIER, m_angleMultiplier);
            m_shader.SetFloat(INTENSITY_MULTIPLIER, m_intensityMultiplier);

            if (directionMap != null)
            {
                m_shader.SetTexture(KERNEL, DIRECTION_MAP, directionMap);
                m_shader.EnableKeyword(KW_HAS_DIRECTION_MAP);
            }
            else
            {
                m_shader.DisableKeyword(KW_HAS_DIRECTION_MAP);
            }

            if (intensityMap != null)
            {
                m_shader.SetTexture(KERNEL, INTENSITY_MAP, intensityMap);
                m_shader.EnableKeyword(KW_HAS_INTENSITY_MAP);
            }
            else
            {
                m_shader.DisableKeyword(KW_HAS_INTENSITY_MAP);
            }

            int instanceCount = inputBuffer.count / PositionSample.SIZE;
            int totalThreadGroupX = (instanceCount + THREAD_PER_GROUP - 1) / THREAD_PER_GROUP;
            int iteration = (totalThreadGroupX + MAX_THREAD_GROUP - 1) / MAX_THREAD_GROUP;
            for (int i = 0; i < iteration; ++i)
            {
                int threadGroupX = Mathf.Min(MAX_THREAD_GROUP, totalThreadGroupX);
                totalThreadGroupX -= MAX_THREAD_GROUP;
                int baseIndex = i * MAX_THREAD_GROUP * THREAD_PER_GROUP;
                m_shader.SetInt(BASE_INDEX, baseIndex);
                m_shader.Dispatch(KERNEL, threadGroupX, 1, 1);
            }

            context.ReleaseReference(inputBufferRefLink);
            context.ReleaseReference(directionRefLink);
            context.ReleaseReference(intensityRefLink);
            Resources.UnloadAsset(m_shader);
        }
    }
}
#endif
