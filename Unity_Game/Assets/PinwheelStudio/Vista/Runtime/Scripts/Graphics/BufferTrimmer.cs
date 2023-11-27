#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graphics
{
    public class BufferTrimmer
    {
        private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/BufferTrimmer";
        private static readonly int SAMPLES = Shader.PropertyToID("_Samples");
        private static readonly int DEST_SAMPLES = Shader.PropertyToID("_DestSamples");
        private static readonly int IN_BOUNDS = Shader.PropertyToID("_InBounds");
        private static readonly int OUT_BOUNDS = Shader.PropertyToID("_OutBounds");
        private static readonly int KERNEL_WRITE_BACK = 0;
        private static readonly int KERNEL_WRITE_TO_DEST = 1;

        private static readonly string KW_DATA_TYPE_INSTANCE_SAMPLE = "DATA_TYPE_INSTANCE_SAMPLE";
        private static readonly string KW_DATA_TYPE_POSITION_SAMPLE = "DATA_TYPE_POSITION_SAMPLE";

        public BufferTrimmer()
        {

        }

        public void Trim<T>(Rect inBounds, Rect outBounds, ComputeBuffer samples, ComputeBuffer destSamples = null)
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

            if (samples.count % structSize != 0)
            {
                Debug.LogError("Cannot parse samples buffer");
                return;
            }
            int kernel = destSamples != null ? KERNEL_WRITE_TO_DEST : KERNEL_WRITE_BACK;

            ComputeShader shader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
            shader.SetBuffer(kernel, SAMPLES, samples);
            if (destSamples != null)
            {
                shader.SetBuffer(kernel, DEST_SAMPLES, destSamples);
            }
            shader.SetVector(IN_BOUNDS, new Vector4(inBounds.min.x, inBounds.min.y, inBounds.size.x, inBounds.size.y));
            shader.SetVector(OUT_BOUNDS, new Vector4(outBounds.min.x, outBounds.min.y, outBounds.size.x, outBounds.size.y));
            shader.shaderKeywords = null;
            shader.EnableKeyword(dataTypeKw);

            int instanceCount = samples.count / structSize;
            int threadGroupX = (instanceCount + 7) / 8;
            int threadGroupY = 1;
            int threadGroupZ = 1;
            shader.Dispatch(kernel, threadGroupX, threadGroupY, threadGroupZ);
            Resources.UnloadAsset(shader);
        }
    }
}
#endif
