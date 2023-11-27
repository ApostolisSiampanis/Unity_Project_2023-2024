#if VISTA
using Pinwheel.Vista.Graphics;
using System.Collections;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [NodeMetadata(
        title = "Smooth",
        path = "Adjustments/Smooth",
        icon = "",
        documentation = "https://docs.google.com/document/d/1zRDVjqaGY2kh4VXFut91oiyVCUex0OV5lTUzzCSwxcY/edit#heading=h.rxr5tevsb1u",
        keywords = "smooth, blur",
        description = "Gradually smooth out the image.")]
    public class SmoothNode : ImageNodeBase
    {
        public readonly MaskSlot inputSlot = new MaskSlot("Input", SlotDirection.Input, 0);
        public readonly MaskSlot maskSlot = new MaskSlot("Mask", SlotDirection.Input, 1);

        public readonly MaskSlot outputSlot = new MaskSlot("Output", SlotDirection.Output, 100);

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
                m_iterationPerFrame = Mathf.Max(1, value);
            }
        }

        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/Smooth";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int MASK_MAP = Shader.PropertyToID("_MaskMap");
        private static readonly int PASS = 0;
        private static readonly string TEMP_TEXTURE_NAME = "SmoothTemp";

        private Material m_material;

        private struct Textures
        {
            public RenderTexture targetRt;
            public RenderTexture tmpRt;
        }

        public SmoothNode() : base()
        {
            m_iterationCount = 5;
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
            RenderTexture tmpRt = context.CreateTemporaryRT(desc, TEMP_TEXTURE_NAME + m_id);
            Drawing.Blit(inputTexture, targetRt);

            Textures textures = new Textures();
            textures.targetRt = targetRt;
            textures.tmpRt = tmpRt;
            return textures;
        }

        private void CreateMaterial(GraphContext context)
        {
            SlotRef maskRefLink = context.GetInputLink(m_id, maskSlot.id);
            Texture maskTexture;
            if (maskRefLink.Equals(SlotRef.invalid))
            {
                maskTexture = Texture2D.whiteTexture;
            }
            else
            {
                maskTexture = context.GetTexture(maskRefLink);
            }

            m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            m_material.SetTexture(MASK_MAP, maskTexture);
        }

        private void Render(int i, Textures textures)
        {
            RenderTexture src, dst;
            if (i % 2 == 0)
            {
                src = textures.targetRt;
                dst = textures.tmpRt;
            }
            else
            {
                src = textures.tmpRt;
                dst = textures.targetRt;
            }

            m_material.SetTexture(MAIN_TEX, src);
            Drawing.DrawQuad(dst, m_material, PASS);
        }

        private void FinalBlit(Textures textures)
        {
            if (m_iterationCount % 2 != 0)
            {
                Drawing.Blit(textures.tmpRt, textures.targetRt);
            }
        }

        private void CleanUp(GraphContext context)
        {
            SlotRef inputRefLink = context.GetInputLink(m_id, inputSlot.id);
            SlotRef maskRefLink = context.GetInputLink(m_id, maskSlot.id);

            context.ReleaseReference(inputRefLink);
            context.ReleaseReference(maskRefLink);
            context.ReleaseTemporary(TEMP_TEXTURE_NAME + m_id);

            Object.DestroyImmediate(m_material);
        }

        public override IEnumerator Execute(GraphContext context)
        {
            Textures textures = CreateTextures(context);
            CreateMaterial(context);

            for (int i = 0; i < m_iterationCount; ++i)
            {
                Render(i, textures);
                if (i % m_iterationPerFrame == 0)
                {
                    yield return null;
                }
            }
            FinalBlit(textures);
            CleanUp(context);
        }

        public override void ExecuteImmediate(GraphContext context)
        {
            Textures textures = CreateTextures(context);
            CreateMaterial(context);

            for (int i = 0; i < m_iterationCount; ++i)
            {
                Render(i, textures);
            }
            FinalBlit(textures);
            CleanUp(context);
        }
    }
}
#endif
