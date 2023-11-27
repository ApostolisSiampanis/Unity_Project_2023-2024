#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public static class NodeLibraryUtilities
    {
        public static class MathNode
        {
            private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Math";
            private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
            private static readonly int CONFIGS = Shader.PropertyToID("_Configs");

            private static readonly string TEMP_RT_NAME = "~TempRT";

            private static Material s_material;

            /// <summary>
            /// Invert (1-value) the content of the targetRt itself
            /// </summary>
            /// <param name="context"></param>
            /// <param name="targetRt"></param>
            public static void Invert(GraphContext context, RenderTexture targetRt)
            {
                DataPool.RtDescriptor tempRtDesc = DataPool.RtDescriptor.Create(targetRt.width, targetRt.height);
                RenderTexture tempRt = context.CreateTemporaryRT(tempRtDesc, TEMP_RT_NAME);
                Drawing.Blit(targetRt, tempRt);

                s_material = new Material(ShaderUtilities.Find(SHADER_NAME));
                s_material.SetTexture(MAIN_TEX, tempRt);

                Graph.MathNode.MathConfig c = new Graph.MathNode.MathConfig()
                {
                    enabled = true,
                    number = 1f,
                    ops = Graph.MathNode.Operator.OneMinus
                };
                Vector4[] configArray = new Vector4[] { c.ToVector() };
                s_material.SetVectorArray(CONFIGS, configArray);
                Drawing.DrawQuad(targetRt, s_material, 0);

                context.ReleaseTemporary(TEMP_RT_NAME);
                Object.DestroyImmediate(s_material);
            }

            /// <summary>
            /// Multiply to the targetRt itself
            /// </summary>
            /// <param name="context"></param>
            /// <param name="targetRt"></param>
            /// <param name="multiplier"></param>
            public static void Multiply(GraphContext context, RenderTexture targetRt, float multiplier)
            {
                DataPool.RtDescriptor tempRtDesc = DataPool.RtDescriptor.Create(targetRt.width, targetRt.height);
                RenderTexture tempRt = context.CreateTemporaryRT(tempRtDesc, TEMP_RT_NAME);
                Drawing.Blit(targetRt, tempRt);

                s_material = new Material(ShaderUtilities.Find(SHADER_NAME));
                s_material.SetTexture(MAIN_TEX, tempRt);

                Graph.MathNode.MathConfig c = new Graph.MathNode.MathConfig()
                {
                    enabled = true,
                    number = multiplier,
                    ops = Graph.MathNode.Operator.Multiply
                };
                Vector4[] configArray = new Vector4[] { c.ToVector() };
                s_material.SetVectorArray(CONFIGS, configArray);
                Drawing.DrawQuad(targetRt, s_material, 0);

                context.ReleaseTemporary(TEMP_RT_NAME);
                Object.DestroyImmediate(s_material);
            }
        }

        public static class ThinOutNode
        {
            private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/ThinOut";
            private static readonly int POSITION_INPUT = Shader.PropertyToID("_PositionInput");
            private static readonly int MASK = Shader.PropertyToID("_Mask");
            private static readonly int MASK_MULTIPLIER = Shader.PropertyToID("_MaskMultiplier");
            private static readonly int POSITION_OUTPUT = Shader.PropertyToID("_PositionOutput");
            private static readonly int SEED = Shader.PropertyToID("_Seed");
            private static readonly int BASE_INDEX = Shader.PropertyToID("_BaseIndex");
            private static readonly int KERNEL_INDEX = 0;

            private static readonly int THREAD_PER_GROUP = 8;
            private static readonly int MAX_THREAD_GROUP = 64000 / THREAD_PER_GROUP;

            private static readonly string HAS_MASK_KW = "HAS_MASK";
            private static ComputeShader s_computeShader;

            public static void Execute(GraphContext context, ComputeBuffer inputPositionBuffer, Texture maskTexture, float maskMultiplier, int seed, ComputeBuffer outputPositionBuffer)
            {
                s_computeShader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
                s_computeShader.SetBuffer(KERNEL_INDEX, POSITION_OUTPUT, outputPositionBuffer);
                s_computeShader.SetBuffer(KERNEL_INDEX, POSITION_INPUT, inputPositionBuffer);

                s_computeShader.SetFloat(MASK_MULTIPLIER, maskMultiplier);
                if (maskTexture != null)
                {
                    s_computeShader.SetTexture(KERNEL_INDEX, MASK, maskTexture);
                    s_computeShader.EnableKeyword(HAS_MASK_KW);
                }
                else
                {
                    s_computeShader.DisableKeyword(HAS_MASK_KW);
                }

                int baseSeed = context.GetArg(Args.SEED).intValue;
                System.Random rnd = new System.Random(seed ^ baseSeed);
                s_computeShader.SetVector(SEED, new Vector4((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble()));

                int instanceCount = inputPositionBuffer.count / PositionSample.SIZE;
                int totalThreadGroupX = (instanceCount + THREAD_PER_GROUP - 1) / THREAD_PER_GROUP;
                int iteration = (totalThreadGroupX + MAX_THREAD_GROUP - 1) / MAX_THREAD_GROUP;
                for (int i = 0; i < iteration; ++i)
                {
                    int threadGroupX = Mathf.Min(MAX_THREAD_GROUP, totalThreadGroupX);
                    totalThreadGroupX -= MAX_THREAD_GROUP;
                    int baseIndex = i * MAX_THREAD_GROUP * THREAD_PER_GROUP;
                    s_computeShader.SetInt(BASE_INDEX, baseIndex);
                    s_computeShader.Dispatch(KERNEL_INDEX, threadGroupX, 1, 1);
                }
                Resources.UnloadAsset(s_computeShader);
            }
        }
    }
}
#endif
