#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graphics
{
    public static class BufferHelper
    {
        private static readonly string COPY_SHADER_NAME = "Vista/Shaders/BufferCopy";
        private static readonly int SRC_BUFFER = Shader.PropertyToID("_SrcBuffer");
        private static readonly int DEST_BUFFER = Shader.PropertyToID("_DestBuffer");
        private static readonly int COPY_KERNEL = 0;

        private static readonly string APPEND_SHADER_NAME = "Vista/Shaders/Graph/Append";
        private static readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");
        private static readonly int APPEND_KERNEL = 0;

        private static ComputeShader s_copyBufferShader;
        private static ComputeShader s_appendBufferShader;

        public static void Copy(ComputeBuffer from, ComputeBuffer to)
        {
            s_copyBufferShader = Resources.Load<ComputeShader>(COPY_SHADER_NAME);
            s_copyBufferShader.SetBuffer(COPY_KERNEL, SRC_BUFFER, from);
            s_copyBufferShader.SetBuffer(COPY_KERNEL, DEST_BUFFER, to);

            int maxElementPerStep = 64000*8;
            int remainingCount = from.count;
            int baseIndex = 0;
            while (remainingCount > 0)
            {
                int count = Mathf.Min(maxElementPerStep, remainingCount);
                s_copyBufferShader.SetInt(BASE_INDEX, baseIndex);
                s_copyBufferShader.Dispatch(COPY_KERNEL, (count + 7) / 8, 1, 1);
                remainingCount -= count;
                baseIndex += count;
            }
            Resources.UnloadAsset(s_copyBufferShader);
        }

        public static ComputeBuffer Clone(ComputeBuffer src)
        {
            ComputeBuffer cloned = new ComputeBuffer(src.count, src.stride);
            Copy(src, cloned);
            return cloned;
        }

        public static ComputeBuffer Append(ComputeBuffer inputBuffer0, ComputeBuffer inputBuffer1)
        {
            s_appendBufferShader = Resources.Load<ComputeShader>(APPEND_SHADER_NAME);            
            if (inputBuffer0 != null && inputBuffer1 == null)
            {
                int size = (inputBuffer0.count);
                ComputeBuffer destBuffer = new ComputeBuffer(size, sizeof(float));

                s_appendBufferShader.SetBuffer(APPEND_KERNEL, DEST_BUFFER, destBuffer);
                s_appendBufferShader.SetBuffer(APPEND_KERNEL, SRC_BUFFER, inputBuffer0);
                s_appendBufferShader.SetInt(BASE_INDEX, 0);

                int instanceCount = inputBuffer0.count / PositionSample.SIZE;
                int threadGroupX = (instanceCount + 7) / 8;
                int threadGroupY = 1;
                int threadGroupZ = 1;
                s_appendBufferShader.Dispatch(APPEND_KERNEL, threadGroupX, threadGroupY, threadGroupZ);
                Resources.UnloadAsset(s_appendBufferShader);
                return destBuffer;
            }
            else if (inputBuffer0 == null && inputBuffer1 != null)
            {
                int size = (inputBuffer1.count);
                ComputeBuffer destBuffer = new ComputeBuffer(size, sizeof(float));

                s_appendBufferShader.SetBuffer(APPEND_KERNEL, DEST_BUFFER, destBuffer);
                s_appendBufferShader.SetBuffer(APPEND_KERNEL, SRC_BUFFER, inputBuffer1);
                s_appendBufferShader.SetInt(BASE_INDEX, 0);

                int instanceCount = inputBuffer1.count / PositionSample.SIZE;
                int threadGroupX = (instanceCount + 7) / 8;
                int threadGroupY = 1;
                int threadGroupZ = 1;
                s_appendBufferShader.Dispatch(APPEND_KERNEL, threadGroupX, threadGroupY, threadGroupZ);
                Resources.UnloadAsset(s_appendBufferShader);
                return destBuffer;
            }
            else
            {
                int size = (inputBuffer0.count + inputBuffer1.count);
                ComputeBuffer destBuffer = new ComputeBuffer(size, sizeof(float));

                s_appendBufferShader.SetBuffer(APPEND_KERNEL, DEST_BUFFER, destBuffer);
                s_appendBufferShader.SetBuffer(APPEND_KERNEL, SRC_BUFFER, inputBuffer0);
                s_appendBufferShader.SetInt(BASE_INDEX, 0);

                int instanceCount = inputBuffer0.count / PositionSample.SIZE;
                int threadGroupX = (instanceCount + 7) / 8;
                int threadGroupY = 1;
                int threadGroupZ = 1;
                s_appendBufferShader.Dispatch(APPEND_KERNEL, threadGroupX, threadGroupY, threadGroupZ);

                s_appendBufferShader.SetBuffer(APPEND_KERNEL, SRC_BUFFER, inputBuffer1);
                s_appendBufferShader.SetInt(BASE_INDEX, inputBuffer0.count / PositionSample.SIZE);
                instanceCount = inputBuffer1.count / PositionSample.SIZE;
                threadGroupX = (instanceCount + 7) / 8;
                s_appendBufferShader.Dispatch(APPEND_KERNEL, threadGroupX, threadGroupY, threadGroupZ);
                Resources.UnloadAsset(s_appendBufferShader);
                return destBuffer;
            }
        }
    }
}
#endif
