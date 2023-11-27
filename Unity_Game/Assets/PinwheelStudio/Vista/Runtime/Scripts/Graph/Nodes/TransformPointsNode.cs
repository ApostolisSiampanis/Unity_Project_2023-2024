#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Transform Points",
        path = "General/Transform Points",
        icon = "",
        documentation = "",
        keywords = "offset, rotation, scale",
        description = "Perform some 2D transformation on a set of points.")]
    public class TransformPointsNode : ExecutableNodeBase
    {
        public readonly BufferSlot inputSlot = new BufferSlot("Input", SlotDirection.Input, 0);
        public readonly BufferSlot outputSlot = new BufferSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private Vector2 m_offset;
        public Vector2 offset
        {
            get
            {
                return m_offset;
            }
            set
            {
                m_offset = value;
            }
        }

        [SerializeField]
        private float m_rotation;
        public float rotation
        {
            get
            {
                return m_rotation;
            }
            set
            {
                m_rotation = value;
            }
        }

        [SerializeField]
        private Vector2 m_scale;
        public Vector2 scale
        {
            get
            {
                return m_scale;
            }
            set
            {
                m_scale = value;
            }
        }

        private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/TransformPoints";
        private static readonly int POSITION_INPUT_BUFFER = Shader.PropertyToID("_PositionInput");
        private static readonly int POSITION_OUTPUT_BUFFER = Shader.PropertyToID("_PositionOutput");
        private static readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");
        private static readonly int TRANSFORM_MATRIX = Shader.PropertyToID("_TransformMatrix");
        private static readonly int KERNEL = 0;

        private static readonly int THREAD_PER_GROUP = 8;
        private static readonly int MAX_THREAD_GROUP = 64000 / THREAD_PER_GROUP;

        private ComputeShader m_shader;

        public TransformPointsNode() : base()
        {
            m_offset = Vector2.zero;
            m_rotation = 0;
            m_scale = Vector2.one * 100;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            SlotRef inputRefLink = context.GetInputLink(m_id, inputSlot.id);
            ComputeBuffer inputBuffer = context.GetBuffer(inputRefLink);
            if (inputBuffer == null)
            {
                return;
            }

            if (inputBuffer.count % PositionSample.SIZE != 0)
            {
                Debug.LogError($"Cannot parse position buffer, node id {m_id}");
                context.ReleaseReference(inputRefLink);
                return;
            }

            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(inputBuffer.count);
            ComputeBuffer outputBuffer = context.CreateBuffer(desc, outputRef);

            m_shader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
            m_shader.SetBuffer(KERNEL, POSITION_INPUT_BUFFER, inputBuffer);
            m_shader.SetBuffer(KERNEL, POSITION_OUTPUT_BUFFER, outputBuffer);

            Matrix4x4 recenterMatrix = Matrix4x4.Translate(new Vector3(-0.5f, 0, -0.5f));
            Matrix4x4 transformMatrix = Matrix4x4.TRS(
                new Vector3(m_offset.x / 100f, 0, m_offset.y / 100f),
                Quaternion.Euler(0, m_rotation, 0),
                new Vector3(m_scale.x / 100f, 1, m_scale.y / 100f));

            m_shader.SetMatrix(TRANSFORM_MATRIX, recenterMatrix.inverse * transformMatrix * recenterMatrix);

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

            context.ReleaseReference(inputRefLink);
            Resources.UnloadAsset(m_shader);
        }
    }
}
#endif
