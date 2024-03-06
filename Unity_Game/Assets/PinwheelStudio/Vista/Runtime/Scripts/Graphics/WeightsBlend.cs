#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graphics
{
    public static class WeightsBlend
    {
        private static readonly string SHADER_NAME = "Hidden/Vista/Graph/WeightBlend";
        private static readonly int BACKGROUND = Shader.PropertyToID("_Background");
        private static readonly int FOREGROUND = Shader.PropertyToID("_Foreground");
        private static readonly int FOREGROUND_MASK = Shader.PropertyToID("_ForegroundMask");
        private static readonly int TARGET_VALUE = Shader.PropertyToID("_TargetValue");
        private static readonly int PASS = 0;

        public static void Blend<T>(RenderTexture[] targetRts, T[] inputTextures, T[] inputMasks = null) where T : Texture
        {
            RenderTexture firstNotNullTexture = null;
            for (int i = 0; i < targetRts.Length; ++i)
            {
                if (targetRts[i] != null)
                {
                    firstNotNullTexture = targetRts[i];
                    break;
                }
            }

            RenderTexture tempRt = new RenderTexture(firstNotNullTexture);
            Material m_material = new Material(ShaderUtilities.Find(SHADER_NAME));
            for (int i = 0; i < targetRts.Length; ++i)
            {
                RenderTexture targetRt = targetRts[i];
                if (targetRt == null)
                    continue;
                Drawing.Blit(Texture2D.blackTexture, tempRt);
                for (int j = i; j < inputTextures.Length; ++j)
                {
                    m_material.SetTexture(BACKGROUND, tempRt);
                    m_material.SetTexture(FOREGROUND, inputTextures[j]);
                    if (inputMasks != null)
                    {
                        m_material.SetTexture(FOREGROUND_MASK, inputMasks[j]);
                    }
                    else
                    {
                        m_material.SetTexture(FOREGROUND_MASK, Texture2D.whiteTexture);
                    }
                    m_material.SetFloat(TARGET_VALUE, (i == j) ? 1f : 0f);
                    Drawing.DrawQuad(targetRt, m_material, PASS);
                    Drawing.Blit(targetRt, tempRt);
                }
            }

            tempRt.Release();
            Object.DestroyImmediate(tempRt);
            Object.DestroyImmediate(m_material);
        }
    }
}
#endif
