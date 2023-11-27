#if VISTA
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pinwheel.Vista
{
    public static class Utilities
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        public static void Swap<T>(ref T[] array, int i0, int i1)
        {
            T tmp = array[i0];
            array[i0] = array[i1];
            array[i1] = tmp;
        }

        public static Material CreateMaterial(string shaderName)
        {
            Shader shader = ShaderUtilities.Find(shaderName);
            Material material;
            if (shader != null)
            {
                material = new Material(shader);
            }
            else
            {
                material = null;
            }
            return material;
        }

        public static Texture2D TextureFromCurve(AnimationCurve curve, int resolution = 2048)
        {
            Texture2D tex = new Texture2D(resolution, 1, TextureFormat.RGBAFloat, false, true);
            Color[] colors = new Color[resolution];
            for (int i = 0; i < resolution; ++i)
            {
                float f = i * 1.0f / (resolution - 1);
                Color c = Color.white * curve.Evaluate(f);
                colors[i] = c;
            }
            tex.SetPixels(colors);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }

        public static AnimationCurve EaseInOutCurve()
        {
            Keyframe[] keys = new Keyframe[]
            {
                new Keyframe(0,0),
                new Keyframe(0.1f, 1),
                new Keyframe(0.9f, 1),
                new Keyframe(1,0)
            };
            return new AnimationCurve(keys);
        }

        public static Gradient CreateGradient(Color start, Color end)
        {
            Gradient g = new Gradient();
            GradientColorKey startColor = new GradientColorKey() { time = 0, color = start };
            GradientColorKey endColor = new GradientColorKey() { time = 1, color = end };
            g.colorKeys = new GradientColorKey[] { startColor, endColor };
            return g;
        }

        public static Texture2D TextureFromGradient(Gradient gradient, int resolution = 2048)
        {
            Texture2D tex = new Texture2D(resolution, 1, TextureFormat.RGBAFloat, false, true);
            Color[] colors = new Color[resolution];
            for (int i = 0; i < resolution; ++i)
            {
                float f = i * 1.0f / (resolution - 1);
                Color c = gradient.Evaluate(f);
                colors[i] = c;
            }
            tex.SetPixels(colors);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }

        public static void SetKeywordEnable(this Material mat, string kw, bool enable)
        {
            if (enable)
            {
                mat.EnableKeyword(kw);
            }
            else
            {
                mat.DisableKeyword(kw);
            }
        }

        public static void Fill<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = value;
            }
        }

        public static int GetLayerIndex(this TerrainData data, TerrainLayer layer)
        {
            TerrainLayer[] layers = data.terrainLayers;
            for (int i = 0; i < layers.Length; ++i)
            {
                if (layer == layers[i])
                    return i;
            }
            return -1;
        }

        public static int MultipleOf8(int value)
        {
            int m = Mathf.CeilToInt(value / 8.0f);
            return m * 8;
        }

        public static float InverseLerpUnclamped(float a, float b, float t)
        {
            if (a == b)
            {
                return 0;
            }
            else
            {
                return (t - a) / (b - a);
            }
        }

        public static float DistancePointToLine(Vector2 l1, Vector2 l2, Vector2 p)
        {
            float num = Mathf.Abs((l2.x - l1.x) * (l1.y - p.y) - (l1.x - p.x) * (l2.y - l1.y));
            float denom = Mathf.Sqrt((l2.x - l1.x) * (l2.x - l1.x) + (l2.y - l1.y) * (l2.y - l1.y));
            return num / denom;
        }

        public static Vector2 XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Texture2D TextureFromFloats(float[] data, int width, int height)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width & Height must >0");
            if (data.Length != width * height)
                throw new ArgumentException("Data & Dimension not match");

            Texture2D tex = new Texture2D(width, height, TextureFormat.RFloat, false, true);
            tex.SetPixelData<float>(data, 0);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            return tex;
        }

        public static float[] FloatsFromTexture(Texture2D tex)
        {
            if (tex == null)
                throw new ArgumentNullException("tex");
            if (tex.format != TextureFormat.RFloat)
                throw new ArgumentException("Texture format must be RFloat");

            Color[] colors = tex.GetPixels();
            float[] data = new float[colors.Length];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = colors[i].r;
            }
            return data;
        }

        public static Bounds Snap(this Bounds b)
        {
            Vector3 min = b.min;
            min.x = Mathf.Floor(min.x);
            min.y = Mathf.Floor(min.y);
            min.z = Mathf.Floor(min.z);

            Vector3 max = b.max;
            max.x = Mathf.Floor(max.x);
            max.y = Mathf.Floor(max.y);
            max.z = Mathf.Floor(max.z);

            b.min = min;
            b.max = max;
            return b;
        }

        public static Vector2 PointToNormalized(Rect r, Vector2 p)
        {
            return new Vector2(InverseLerpUnclamped(r.min.x, r.max.x, p.x), InverseLerpUnclamped(r.min.y, r.max.y, p.y));
        }

        public static Vector2 NormalizedToPoint(Rect r, Vector2 p)
        {
            return new Vector2(Mathf.LerpUnclamped(r.min.x, r.max.x, p.x), Mathf.LerpUnclamped(r.min.y, r.max.y, p.y));
        }

        public static float[] ResampleBilinear(float[] data, int width, int height, int newWidth, int newHeight)
        {
            if (data.Length != width * height)
            {
                throw new ArgumentException("Source data dimension mismatched");
            }

            Texture2D srcTex2D = TextureFromFloats(data, width, height);
            RenderTexture targetRt = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            Graphics.Drawing.Blit(srcTex2D, targetRt);

            RenderTexture.active = targetRt;
            Texture2D dstTex2D = new Texture2D(newWidth, newHeight, TextureFormat.RFloat, false, true);
            dstTex2D.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            dstTex2D.Apply();
            RenderTexture.active = null;

            float[] newData = FloatsFromTexture(dstTex2D);

            RenderTexture.ReleaseTemporary(targetRt);
            Object.DestroyImmediate(srcTex2D);
            Object.DestroyImmediate(dstTex2D);

            return newData;
        }

        public static IEnumerable<FieldInfo> GetSerializedObjectFields(Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            List<FieldInfo> serializedObjectFields = new List<FieldInfo>();
            foreach (FieldInfo f in fields)
            {
                SerializeField serializeAttribute = f.GetCustomAttribute<SerializeField>();
                NonSerializedAttribute nonSerializeAttribute = f.GetCustomAttribute<NonSerializedAttribute>();
                bool isPublic = f.IsPublic;

                bool serializable = false;
                if (nonSerializeAttribute != null)
                    serializable = false;
                else if (isPublic)
                    serializable = true;
                else if (serializeAttribute != null)
                    serializable = true;

                bool isObject = f.FieldType.IsSubclassOf(typeof(UnityEngine.Object));

                if (serializable && isObject)
                {
                    serializedObjectFields.Add(f);
                }
            }
            return serializedObjectFields;
        }        
    }
}
#endif
