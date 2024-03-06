#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graphics
{
    public static class PolygonMaskRenderer
    {
        public class Configs
        {
            public Vector2[] vertices { get; set; }
            public Vector2[] falloffVertices { get; set; }
            public Texture falloffTexture { get; set; }
        }

        private static readonly string COMPUTE_SHADER_NAME = "Vista/Shaders/Graph/PolygonMask";
        private static readonly int MASK_TEX = Shader.PropertyToID("_MaskTex");
        private static readonly int MASK_RESOLUTION = Shader.PropertyToID("_MaskResolution");
        private static readonly int SEGMENTS = Shader.PropertyToID("_Segments");
        private static readonly int SEGMENT_COUNT = Shader.PropertyToID("_SegmentCount");
        private static readonly int KERNEL_INDEX = 0;

        private static readonly string FALLOFF_SHADER_NAME = "Hidden/Vista/Graph/PolygonMaskFalloff";
        private static readonly int FALLOFF_MAP = Shader.PropertyToID("_FalloffMap");
        private static readonly int PASS = 0;
        private static readonly string KW_HAS_FALLOFF_MAP = "HAS_FALLOFF_MAP";

        public static void Render(RenderTexture targetRt, Configs configs)
        {
            Drawing.Blit(Texture2D.blackTexture, targetRt);
            DrawBaseMask(targetRt, configs);
            DrawFalloffMask(targetRt, configs);
        }

        private static void DrawBaseMask(RenderTexture targetRt, Configs configs)
        {
            const int MAX_VERTEX = 999;
            Vector2[] vertices = configs.vertices;
            Vector2[] falloffVertices = configs.falloffVertices;
            Vector4[] segments = new Vector4[vertices.Length + 1];
            for (int i = 0; i < vertices.Length; ++i)
            {
                segments[i] = (vertices[i] + falloffVertices[i]) * 0.5f;
            }
            segments[segments.Length - 1] = (vertices[0] + falloffVertices[0]) * 0.5f;
            int segmentCount = vertices.Length;

            if (vertices.Length > MAX_VERTEX)
            {
                Debug.LogWarning("VISTA: Too many vertices in a polygon mask may lead to incorrect result. Try reducing vertex count.");
            }

            ComputeShader computeShader = Resources.Load<ComputeShader>(COMPUTE_SHADER_NAME);
            computeShader.SetTexture(KERNEL_INDEX, MASK_TEX, targetRt);
            computeShader.SetVector(MASK_RESOLUTION, new Vector4(targetRt.width, targetRt.height, 0, 0));
            computeShader.SetVectorArray(SEGMENTS, segments);
            computeShader.SetInt(SEGMENT_COUNT, segmentCount);

            int threadX = (targetRt.width + 7) / 8;
            int threadY = (targetRt.height + 7) / 8;
            int threadZ = 1;
            computeShader.Dispatch(KERNEL_INDEX, threadX, threadY, threadZ);
            Resources.UnloadAsset(computeShader);
        }

        private static void DrawFalloffMask(RenderTexture targetRt, Configs configs)
        {
            if (IsFalloffDistanceZero(configs))
                return;

            Vector2[] vertices = configs.vertices;
            Vector2[] falloffVertices = configs.falloffVertices;
            Vector4[] segments = new Vector4[vertices.Length + 1];
            Vector4[] falloffSegments = new Vector4[vertices.Length + 1];
            for (int i = 0; i < vertices.Length; ++i)
            {
                segments[i] = vertices[i];
                falloffSegments[i] = falloffVertices[i];
            }
            segments[segments.Length - 1] = vertices[0];
            falloffSegments[falloffSegments.Length - 1] = falloffVertices[0];

            Material material = new Material(ShaderUtilities.Find(FALLOFF_SHADER_NAME));

            if (configs.falloffTexture != null)
            {
                material.SetTexture(FALLOFF_MAP, configs.falloffTexture);
                material.EnableKeyword(KW_HAS_FALLOFF_MAP);
            }
            else
            {
                material.DisableKeyword(KW_HAS_FALLOFF_MAP);
            }

            RenderTexture.active = targetRt;
            GL.PushMatrix();
            material.SetPass(PASS);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);

            for (int i = 0; i < segments.Length - 1; ++i)
            {
                GL.Color(Color.white);
                GL.Vertex3(segments[i].x, segments[i].y, 0);
                GL.Color(Color.white);
                GL.Vertex3(segments[i + 1].x, segments[i + 1].y, 0);
                GL.Color(Color.black);
                GL.Vertex3(falloffSegments[i + 1].x, falloffSegments[i + 1].y, 0);
                GL.Color(Color.black);
                GL.Vertex3(falloffSegments[i].x, falloffSegments[i].y, 0);
            }

            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;

            Object.DestroyImmediate(material);
        }

        private static bool IsFalloffDistanceZero(Configs configs)
        {
            Vector2[] vertices = configs.vertices;
            Vector2[] falloffVertices = configs.falloffVertices;

            for (int i = 0; i < vertices.Length; ++i)
            {
                if (vertices[i] != falloffVertices[i])
                    return false;
            }

            return true;
        }
    }
}
#endif
