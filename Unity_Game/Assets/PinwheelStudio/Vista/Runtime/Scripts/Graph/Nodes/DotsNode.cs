#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Dots",
        path = "Masking/Dots",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.nudmohh16gl1",
        keywords = "",
        description = "Turn position data to pixels.")]
    public class DotsNode : ImageNodeBase
    {
        public readonly BufferSlot positionSlot = new BufferSlot("Positions", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/Dots";
        private static readonly int POSITIONS = Shader.PropertyToID("_Positions");
        private static readonly int TARGET_RT = Shader.PropertyToID("_TargetRT");
        private static readonly int RESOLUTION = Shader.PropertyToID("_Resolution");
        private static readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");

        private static readonly int THREAD_PER_GROUP = 8;
        private static readonly int MAX_THREAD_GROUP = 64000 / THREAD_PER_GROUP;

        private static readonly int KERNEL_INDEX = 0;
        private ComputeShader m_computeShader;

        public DotsNode() : base()
        {

        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            int resolution = this.CalculateResolution(baseResolution, baseResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);
            Drawing.Blit(Texture2D.blackTexture, targetRt);

            SlotRef positionSlotRefLink = context.GetInputLink(m_id, positionSlot.id);
            ComputeBuffer positionBuffer = context.GetBuffer(positionSlotRefLink);
            if (positionBuffer == null)
            {
                return;
            }
            if (positionBuffer.count % PositionSample.SIZE != 0)
            {
                Debug.Log($"Cannot parse position buffer, node id {m_id}");
                context.ReleaseReference(positionSlotRefLink);
                return;
            }

            m_computeShader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
            m_computeShader.SetBuffer(KERNEL_INDEX, POSITIONS, positionBuffer);
            m_computeShader.SetTexture(KERNEL_INDEX, TARGET_RT, targetRt);
            m_computeShader.SetVector(RESOLUTION, new Vector4(targetRt.width, targetRt.height, 0, 0));

            int instanceCount = positionBuffer.count / PositionSample.SIZE;

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

            context.ReleaseReference(positionSlotRefLink);
            Resources.UnloadAsset(m_computeShader);
        }
    }
}
#endif
