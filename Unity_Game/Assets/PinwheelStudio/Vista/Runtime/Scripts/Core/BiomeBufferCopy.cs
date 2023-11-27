#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public static class BiomeBufferCopy
    {
        private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/BiomeBufferCopy";
        private static readonly int SAMPLES = Shader.PropertyToID("_Samples");
        private static readonly int DEST_SAMPLES = Shader.PropertyToID("_DestSamples");
        private static readonly int IN_BOUNDS = Shader.PropertyToID("_InBounds");
        private static readonly int OUT_BOUNDS = Shader.PropertyToID("_OutBounds");
        private static readonly int COUNT = Shader.PropertyToID("_Count");
        private static readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");

        private static readonly int THREAD_PER_GROUP = 8;
        private static readonly int MAX_THREAD_GROUP = 64000 / THREAD_PER_GROUP;

        //private static readonly int KERNEL_COUNT = 0;
        private static readonly int KERNEL_APPEND = 1;

        private static readonly string KW_DATA_TYPE_INSTANCE_SAMPLE = "DATA_TYPE_INSTANCE_SAMPLE";
        private static readonly string KW_DATA_TYPE_POSITION_SAMPLE = "DATA_TYPE_POSITION_SAMPLE";

        private static ComputeShader s_computeShader;

        public static ComputeBuffer CopyFrom<T>(ComputeBuffer srcBuffer, Rect inBounds, Rect outBounds)
        {
            int structSize;
            string dataTypeKw;
            if (typeof(T).Equals(typeof(PositionSample)))
            {
                structSize = PositionSample.SIZE;
                dataTypeKw = KW_DATA_TYPE_POSITION_SAMPLE;
            }
            else if (typeof(T).Equals(typeof(InstanceSample)))
            {
                structSize = InstanceSample.SIZE;
                dataTypeKw = KW_DATA_TYPE_INSTANCE_SAMPLE;
            }
            else
            {
                throw new System.ArgumentException($"Buffer data type must be {typeof(InstanceSample).Name} or {typeof(PositionSample).Name}");
            }

            if (srcBuffer.count % structSize != 0)
            {
                throw new System.ArgumentException("Source buffer size & struct size not match");
            }

            //int[] countData = new int[1] { 0 };
            //ComputeBuffer countArgBuffer = new ComputeBuffer(1, sizeof(int));
            //countArgBuffer.SetData(countData);

            s_computeShader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
            //s_computeShader.SetBuffer(KERNEL_COUNT, SAMPLES, srcBuffer);
            //s_computeShader.SetBuffer(KERNEL_COUNT, COUNT, countArgBuffer);
            s_computeShader.SetVector(IN_BOUNDS, new Vector4(inBounds.min.x, inBounds.min.y, inBounds.size.x, inBounds.size.y));
            s_computeShader.SetVector(OUT_BOUNDS, new Vector4(outBounds.min.x, outBounds.min.y, outBounds.size.x, outBounds.size.y));

            s_computeShader.shaderKeywords = null;
            s_computeShader.EnableKeyword(dataTypeKw);

            //int instanceCount = srcBuffer.count / structSize;
            //int totalThreadGroupX = (instanceCount + THREAD_PER_GROUP - 1) / THREAD_PER_GROUP;
            //int iteration = (totalThreadGroupX + MAX_THREAD_GROUP - 1) / MAX_THREAD_GROUP;
            //for (int i = 0; i < iteration; ++i)
            //{
            //    int threadGroupX = Mathf.Min(MAX_THREAD_GROUP, totalThreadGroupX);
            //    totalThreadGroupX -= MAX_THREAD_GROUP;
            //    int baseIndex = i * MAX_THREAD_GROUP * THREAD_PER_GROUP;
            //    s_computeShader.SetInt(BASE_INDEX, baseIndex);
            //    s_computeShader.Dispatch(KERNEL_COUNT, threadGroupX, 1, 1);
            //}

            //countArgBuffer.GetData(countData);
            //if (countData[0] <= 0)
            //{
            //    countArgBuffer.Dispose();
            //    return null;
            //}

            //int destCount = countData[0];
            //countData[0] = 0;
            //countArgBuffer.SetData(countData);

            int instanceCount = srcBuffer.count / structSize;
            ComputeBuffer destBuffer = new ComputeBuffer(instanceCount * structSize, sizeof(float));
            s_computeShader.SetBuffer(KERNEL_APPEND, SAMPLES, srcBuffer);
            s_computeShader.SetBuffer(KERNEL_APPEND, DEST_SAMPLES, destBuffer);
            //s_computeShader.SetBuffer(KERNEL_APPEND, COUNT, countArgBuffer);

            int totalThreadGroupX = (instanceCount + THREAD_PER_GROUP - 1) / THREAD_PER_GROUP;
            int iteration = (totalThreadGroupX + MAX_THREAD_GROUP - 1) / MAX_THREAD_GROUP;
            for (int i = 0; i < iteration; ++i)
            {
                int threadGroupX = Mathf.Min(MAX_THREAD_GROUP, totalThreadGroupX);
                totalThreadGroupX -= MAX_THREAD_GROUP;
                int baseIndex = i * MAX_THREAD_GROUP * THREAD_PER_GROUP;
                s_computeShader.SetInt(BASE_INDEX, baseIndex);
                s_computeShader.Dispatch(KERNEL_APPEND, threadGroupX, 1, 1);
            }

            //countArgBuffer.Dispose();
            Resources.UnloadAsset(s_computeShader);
            return destBuffer;
        }
    }
}
#endif
