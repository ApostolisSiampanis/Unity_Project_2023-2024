#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Expand",
        path = "Masking/Expand",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.urhom5i7w4mm",
        keywords = "",
        description = "Gradually expand/grow the mask. Useful when you want to include more regions in the mask.")]
    public class ExpandNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

        [SerializeField]
        private float m_ratio;
        public float ratio
        {
            get
            {
                return m_ratio;
            }
            set
            {
                m_ratio = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private int m_iterationCount;
        public int iterationCount
        {
            get
            {
                return m_iterationCount;
            }
            set
            {
                m_iterationCount = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private int m_iterationPerFrame;
        public int iterationPerFrame
        {
            get
            {
                return m_iterationPerFrame;
            }
            set
            {
                m_iterationPerFrame = Mathf.Max(0, value);
            }
        }

        private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/Expand";
        private static readonly int TARGET_RT = Shader.PropertyToID("_TargetRT");
        private static readonly int RATIO = Shader.PropertyToID("_Ratio");
        private static readonly int KERNEL_INDEX = 0;
        private ComputeShader m_computeShader;

        private struct Textures
        {
            public RenderTexture targetRt;
        }

        public ExpandNode() : base()
        {
            m_ratio = 0.1f;
            m_iterationCount = 10;
            m_iterationPerFrame = 10;
        }

        private Textures CreateTextures(GraphContext context)
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

            int resolution = this.CalculateResolution(baseResolution, inputResolution);
            DataPool.RtDescriptor desc = DataPool.RtDescriptor.Create(resolution, resolution);
            SlotRef outputRef = new SlotRef(m_id, outputSlot.id);
            RenderTexture targetRt = context.CreateRenderTarget(desc, outputRef);
            Drawing.Blit(inputTexture, targetRt);

            Textures textures = new Textures();
            textures.targetRt = targetRt;
            return textures;
        }

        private void CreateComputeShader(GraphContext context, Textures textures)
        {
            m_computeShader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
            m_computeShader.SetTexture(KERNEL_INDEX, TARGET_RT, textures.targetRt);
            m_computeShader.SetFloat(RATIO, m_ratio);
        }

        private void Dispatch(int threadGroupX, int threadGroupY, int threadGroupZ)
        {
            m_computeShader.Dispatch(KERNEL_INDEX, threadGroupX, threadGroupY, threadGroupZ);
        }

        private void CleanUp(GraphContext context)
        {
            SlotRef inputRefLink = context.GetInputLink(m_id, inputSlot.id);
            context.ReleaseReference(inputRefLink);
            Resources.UnloadAsset(m_computeShader);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            Textures textures = CreateTextures(context);
            CreateComputeShader(context, textures);

            for (int i = 0; i < m_iterationCount; ++i)
            {
                int threadGroupX = (textures.targetRt.width + 7) / 8;
                int threadGroupY = 1;
                int threadGroupZ = (textures.targetRt.height + 7) / 8;

                Dispatch(threadGroupX, threadGroupY, threadGroupZ);
                if (i % m_iterationPerFrame == 0)
                {
                    yield return null;
                }
            }
            CleanUp(context);
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            Textures textures = CreateTextures(context);
            CreateComputeShader(context, textures);

            for (int i = 0; i < m_iterationCount; ++i)
            {
                int threadGroupX = (textures.targetRt.width + 7) / 8;
                int threadGroupY = 1;
                int threadGroupZ = (textures.targetRt.height + 7) / 8;
                Dispatch(threadGroupX, threadGroupY, threadGroupZ);
            }
            CleanUp(context);
        }
    }
}
#endif
