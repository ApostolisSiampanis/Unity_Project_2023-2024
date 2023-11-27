#if VISTA
using Pinwheel.Vista.Graphics;
using UnityEngine;

namespace Pinwheel.Vista
{
    public static class LPBUtilities
    {
        public static readonly string BIOME_MASK_COMBINE_SHADER = "Hidden/Vista/BiomeMaskCombine";
        public static readonly int BASE_BIOME_MASK = Shader.PropertyToID("_BaseBiomeMask");
        public static readonly int BIOME_MASK_ADJUSTMENTS = Shader.PropertyToID("_BiomeMaskAdjustments");

        public static readonly int SRC_BLEND = Shader.PropertyToID("_SrcBlend");
        public static readonly int DST_BLEND = Shader.PropertyToID("_DstBlend");
        public static readonly int BLEND_OP = Shader.PropertyToID("_BlendOp");
        public static readonly string KW_BLIT_TO_DEST = "BLIT_TO_DEST";
        public static readonly int PASS = 0;

        public static void CombineBiomeMask(RenderTexture baseMask, Texture adjustmentTex)
        {
            RenderTexture baseMaskCopy = new RenderTexture(baseMask);
            Drawing.Blit(baseMask, baseMaskCopy);

            Material mat = new Material(ShaderUtilities.Find(BIOME_MASK_COMBINE_SHADER));
            mat.SetTexture(BASE_BIOME_MASK, baseMaskCopy);
            mat.SetTexture(BIOME_MASK_ADJUSTMENTS, adjustmentTex);
            Drawing.DrawQuad(baseMask, mat, PASS);

            baseMaskCopy.Release();
            Object.DestroyImmediate(baseMaskCopy);
            Object.DestroyImmediate(mat);
        }
    }
}
#endif
