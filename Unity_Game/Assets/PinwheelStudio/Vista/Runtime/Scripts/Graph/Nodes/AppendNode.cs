#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Append",
        path = "General/Append",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.v91mznpqvcyu",
        keywords = "add, combine",
        description = "Append 2 buffers.")]
    public class AppendNode : ExecutableNodeBase
    {
        public readonly BufferSlot inputSlot0 = new BufferSlot("Input 0", SlotDirection.Input, 0);
        public readonly BufferSlot inputSlot1 = new BufferSlot("Input 1", SlotDirection.Input, 1);

        public readonly BufferSlot outputSlot = new BufferSlot("Output", SlotDirection.Output, 100);

        private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/Append";
        private static readonly int SOURCE_BUFFER = Shader.PropertyToID("_SrcBuffer");
        private static readonly int DEST_BUFFER = Shader.PropertyToID("_DestBuffer");
        private static readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");
        private static readonly int DEST_OFFSET = Shader.PropertyToID("_DestOffset");
        private static readonly int KERNEL = 0;

        private static readonly int THREAD_PER_GROUP = 8;
        private static readonly int MAX_THREAD_GROUP = 64000 / THREAD_PER_GROUP;

        private ComputeShader m_shader;

        public AppendNode() : base()
        {

        }

        public override void ExecuteImmediate(GraphContext context)
        {
            SlotRef input0RefLink = context.GetInputLink(m_id, inputSlot0.id);
            ComputeBuffer inputBuffer0 = context.GetBuffer(input0RefLink);
            if (inputBuffer0 != null && inputBuffer0.count % PositionSample.SIZE != 0)
            {
                Debug.LogError($"Cannot parse buffer {inputSlot0.name}, node id {m_id}");
                return;
            }

            SlotRef input1RefLink = context.GetInputLink(m_id, inputSlot1.id);
            ComputeBuffer inputBuffer1 = context.GetBuffer(input1RefLink);
            if (inputBuffer1 != null && inputBuffer1.count % PositionSample.SIZE != 0)
            {
                Debug.LogError($"Cannot parse buffer {inputSlot1.name}, node id {m_id}");
                return;
            }

            m_shader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            if (inputBuffer0 != null && inputBuffer1 != null)
            {
                int size = (inputBuffer0.count + inputBuffer1.count);
                DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(size);
                ComputeBuffer destBuffer = context.CreateBuffer(desc, outputRef);
                               
                m_shader.SetBuffer(KERNEL, DEST_BUFFER, destBuffer);
                m_shader.SetBuffer(KERNEL, SOURCE_BUFFER, inputBuffer0);
                m_shader.SetInt(DEST_OFFSET, 0);

                int instanceCount = inputBuffer0.count / PositionSample.SIZE;
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

                m_shader.SetBuffer(KERNEL, SOURCE_BUFFER, inputBuffer1);
                m_shader.SetInt(DEST_OFFSET, inputBuffer0.count / PositionSample.SIZE);

                instanceCount = inputBuffer1.count / PositionSample.SIZE;
                totalThreadGroupX = (instanceCount + THREAD_PER_GROUP - 1) / THREAD_PER_GROUP;
                iteration = (totalThreadGroupX + MAX_THREAD_GROUP - 1) / MAX_THREAD_GROUP;

                for (int i = 0; i < iteration; ++i)
                {
                    int threadGroupX = Mathf.Min(MAX_THREAD_GROUP, totalThreadGroupX);
                    totalThreadGroupX -= MAX_THREAD_GROUP;
                    int baseIndex = i * MAX_THREAD_GROUP * THREAD_PER_GROUP;
                    m_shader.SetInt(BASE_INDEX, baseIndex);
                    m_shader.Dispatch(KERNEL, threadGroupX, 1, 1);
                }
            }
            else if (inputBuffer0 != null && inputBuffer1 == null)
            {
                int size = (inputBuffer0.count);
                DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(size);
                ComputeBuffer destBuffer = context.CreateBuffer(desc, outputRef);

                m_shader.SetBuffer(KERNEL, DEST_BUFFER, destBuffer);
                m_shader.SetBuffer(KERNEL, SOURCE_BUFFER, inputBuffer0);
                m_shader.SetInt(DEST_OFFSET, 0);

                int instanceCount = inputBuffer0.count / PositionSample.SIZE;
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
            }
            else if (inputBuffer0 == null && inputBuffer1 != null)
            {
                int size = (inputBuffer1.count);
                DataPool.BufferDescriptor desc = DataPool.BufferDescriptor.Create(size);
                ComputeBuffer destBuffer = context.CreateBuffer(desc, outputRef);

                m_shader.SetBuffer(KERNEL, DEST_BUFFER, destBuffer);
                m_shader.SetBuffer(KERNEL, SOURCE_BUFFER, inputBuffer1);
                m_shader.SetInt(DEST_OFFSET, 0);

                int instanceCount = inputBuffer1.count / PositionSample.SIZE;
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
            }

            context.ReleaseReference(input0RefLink);
            context.ReleaseReference(input1RefLink);
            Resources.UnloadAsset(m_shader);
        }
    }
}
#endif
