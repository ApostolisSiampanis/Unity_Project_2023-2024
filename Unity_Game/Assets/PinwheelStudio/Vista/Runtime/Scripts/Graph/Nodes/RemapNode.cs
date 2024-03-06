#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Remap",
        path = "Adjustments/Remap",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.k6uf92e8gxz2",
        keywords = "remap, normalize, auto level",
        description = "Remap the image value to an output range. Similar to normalize & auto levels filter.\nUseful when you want to brighten up a very dark image such as erosion/deposition data.")]
    public class RemapNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_min;
        public float min
        {
            get
            {
                return m_min;
            }
            set
            {
                m_min = Mathf.Min(value, m_max);
            }
        }

        [SerializeField]
        private float m_max;
        public float max
        {
            get
            {
                return m_max;
            }
            set
            {
                m_max = Mathf.Max(value, m_min);
            }
        }

        private ComputeShader m_computeShader;
        private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/Remap";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int MIN_MAX_BUFFER = Shader.PropertyToID("_MinMaxBuffer");
        private static readonly int INT_LIMIT = Shader.PropertyToID("_IntLimit");
        private static readonly int KERNEL_INDEX = 0;

        private Material m_material;
        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Remap";
        private static readonly int IN_MIN = Shader.PropertyToID("_InMin");
        private static readonly int IN_MAX = Shader.PropertyToID("_InMax");
        private static readonly int OUT_MIN = Shader.PropertyToID("_OutMin");
        private static readonly int OUT_MAX = Shader.PropertyToID("_OutMax");
        private static readonly int PASS = 0;

        public RemapNode() : base()
        {
            m_min = 0;
            m_max = 1;
        }

        public override IEnumerator Execute(GraphContext context)
        {
            ExecuteImmediate(context);
            yield return null;
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            int baseResolution = context.GetArg(Args.RESOLUTION).intValue;
            SlotRef inputRefLink = context.GetInputLink(m_id, inputSlot.id);
            Texture inputTexture = context.GetTexture(inputRefLink);
            int inputResolution;
            if (inputTexture == null)
            {
                inputTexture = Texture2D.blackTexture;
                inputResolution = baseResolution;
            }
            else
            {
                inputResolution = inputTexture.width;
            }

            int[] minMaxData = new int[2];
            if (inputTexture != Texture2D.blackTexture)
            {
                minMaxData[0] = int.MaxValue;
                minMaxData[1] = int.MinValue;
                m_computeShader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
                m_computeShader.SetTexture(KERNEL_INDEX, MAIN_TEX, inputTexture);

                ComputeBuffer minMaxBuffer = new ComputeBuffer(2, sizeof(int));
                minMaxBuffer.SetData(minMaxData);
                m_computeShader.SetBuffer(KERNEL_INDEX, MIN_MAX_BUFFER, minMaxBuffer);
                m_computeShader.SetInt(INT_LIMIT, int.MaxValue);
                int threadGroupX = (inputTexture.width + 7) / 8;
                int threadGroupY = (inputTexture.height + 7) / 8;
                int threadGroupZ = 1;
                m_computeShader.Dispatch(KERNEL_INDEX, threadGroupX, threadGroupY, threadGroupZ);

                minMaxBuffer.GetData(minMaxData);
                minMaxBuffer.Dispose();
                Resources.UnloadAsset(m_computeShader);
            }
            else
            {
                minMaxData[0] = 0;
                minMaxData[1] = 0;
            }

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MAIN_TEX, inputTexture);
            m_material.SetFloat(IN_MIN, minMaxData[0] * 1.0f / int.MaxValue);
            m_material.SetFloat(IN_MAX, minMaxData[1] * 1.0f / int.MaxValue);
            m_material.SetFloat(OUT_MIN, m_min);
            m_material.SetFloat(OUT_MAX, m_max);
            Drawing.DrawQuad(targetRt, m_material, PASS);

            context.ReleaseReference(inputRefLink);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
