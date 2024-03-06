#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graphics
{
    public static class Drawing
    {
        private static readonly string BLIT_SHADER_NAME = "Hidden/Vista/Blit";
        private static readonly string BLIT_01_SHADER_NAME = "Hidden/Vista/Blit01";
        private static readonly string BLIT_RANGE_SHADER_NAME = "Hidden/Vista/BlitRange";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int START_UV = Shader.PropertyToID("_StartUV");
        private static readonly int END_UV = Shader.PropertyToID("_EndUV");

        public static readonly Vector2[] unitQuad = new Vector2[]
        {
            Vector2.zero,
            Vector2.up,
            Vector2.one,
            Vector2.right
        };

        public static void DrawQuad(RenderTexture rt, Vector2[] quad, Material mat, int pass)
        {
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetPass(pass);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.TexCoord(new Vector3(0, 0, 0));
            GL.Vertex3(quad[0].x, quad[0].y, 0);
            GL.TexCoord(new Vector3(0, 1, 0));
            GL.Vertex3(quad[1].x, quad[1].y, 0);
            GL.TexCoord(new Vector3(1, 1, 0));
            GL.Vertex3(quad[2].x, quad[2].y, 0);
            GL.TexCoord(new Vector3(1, 0, 0));
            GL.Vertex3(quad[3].x, quad[3].y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static void DrawQuad(RenderTexture rt, Vector2[] quadCorners, Vector2[] uvs, Material mat, int pass)
        {
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetPass(pass);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.TexCoord(uvs[0]);
            GL.Vertex3(quadCorners[0].x, quadCorners[0].y, 0);
            GL.TexCoord(uvs[1]);
            GL.Vertex3(quadCorners[1].x, quadCorners[1].y, 0);
            GL.TexCoord(uvs[2]);
            GL.Vertex3(quadCorners[2].x, quadCorners[2].y, 0);
            GL.TexCoord(uvs[3]);
            GL.Vertex3(quadCorners[3].x, quadCorners[3].y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static void DrawQuad(RenderTexture rt, Material mat, int pass)
        {
            DrawQuad(rt, unitQuad, mat, pass);
        }

        public static void DrawTexture(RenderTexture rt, Vector2[] quadCorners, Texture tex)
        {
            Material mat = new Material(ShaderUtilities.Find(BLIT_SHADER_NAME));
            mat.SetTexture(MAIN_TEX, tex);
            DrawQuad(rt, quadCorners, mat, 0);
            Object.DestroyImmediate(mat);
        }

        public static void DrawPolygonOutline(RenderTexture rt, Vector2[] vertices, Material mat, int pass)
        {
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetPass(pass);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            Vector2 v0, v1;
            for (int i = 0; i < vertices.Length - 1; ++i)
            {
                v0 = vertices[i];
                v1 = vertices[i + 1];
                GL.Vertex3(v0.x, v0.y, 0);
                GL.Vertex3(v1.x, v1.y, 0);
            }
            v0 = vertices[vertices.Length - 1];
            v1 = vertices[0];
            GL.Vertex3(v0.x, v0.y, 0);
            GL.Vertex3(v1.x, v1.y, 0);

            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static void Blit(Texture from, RenderTexture to)
        {
            Material mat = new Material(ShaderUtilities.Find(BLIT_SHADER_NAME));
            mat.SetTexture(MAIN_TEX, from);
            DrawQuad(to, unitQuad, mat, 0);
            Object.DestroyImmediate(mat);
        }

        public static void Blit01(Texture from, RenderTexture to)
        {
            Material mat = new Material(ShaderUtilities.Find(BLIT_01_SHADER_NAME));
            mat.SetTexture(MAIN_TEX, from);
            DrawQuad(to, unitQuad, mat, 0);
            Object.DestroyImmediate(mat);
        }

        public static void BlitRange(Texture from, RenderTexture to, Vector2 startUv, Vector2 endUv)
        {
            Material mat = new Material(ShaderUtilities.Find(BLIT_RANGE_SHADER_NAME));
            mat.SetTexture(MAIN_TEX, from);
            mat.SetVector(START_UV, startUv);
            mat.SetVector(END_UV, endUv);
            DrawQuad(to, unitQuad, mat, 0);
            Object.DestroyImmediate(mat);
        }

        public static void BlitAdd(Texture from, RenderTexture to)
        {
            Material mat = new Material(ShaderUtilities.Find(BLIT_SHADER_NAME));
            mat.SetTexture(MAIN_TEX, from);
            DrawQuad(to, unitQuad, mat, 1);
            Object.DestroyImmediate(mat);
        }
    }
}
#endif
