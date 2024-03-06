#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public class DataPool : System.IDisposable
    {
        public struct RtDescriptor : System.IEquatable<RtDescriptor>, System.IEquatable<RenderTextureDescriptor>
        {
            public int width { get; set; }
            public int height { get; set; }
            public RenderTextureFormat format { get; set; }

            public static RtDescriptor Create(int width, int height, RenderTextureFormat format = RenderTextureFormat.RFloat)
            {
                RtDescriptor d = new RtDescriptor();
                d.width = width;
                d.height = height;
                d.format = format;
                return d;
            }

            public bool Equals(RtDescriptor other)
            {
                return this.width == other.width &&
                    this.height == other.height &&
                    this.format == other.format;
            }

            public bool Equals(RenderTextureDescriptor other)
            {
                return this.width == other.width &&
                    this.height == other.height &&
                    this.format == other.colorFormat;
            }

            public static implicit operator RenderTextureDescriptor(RtDescriptor desc)
            {
                RenderTextureDescriptor d = new RenderTextureDescriptor(desc.width, desc.height, desc.format);
                d.sRGB = false;
                d.enableRandomWrite = true;
                d.mipCount = 0;
                d.useMipMap = false;
                return d;
            }
        }

        public struct BufferDescriptor : System.IEquatable<BufferDescriptor>
        {
            public const int DEFAULT_STRIDE = sizeof(float);

            public int count { get; set; }
            public int stride
            {
                get
                {
                    return DEFAULT_STRIDE;
                }
            }

            public static BufferDescriptor Create(int size)
            {
                BufferDescriptor desc = new BufferDescriptor();
                desc.count = size;
                return desc;
            }

            public bool Equals(BufferDescriptor other)
            {
                return this.count == other.count;
            }
        }

        public struct MemoryStats
        {
            public int textureCount { get; set; }
            public int bufferCount { get; set; }
            public float megabyte { get; set; }

            private static Dictionary<RenderTextureFormat, int> memoryMap;

            public static int GetBytePerPixel(RenderTextureFormat format)
            {
                if (memoryMap == null)
                {
                    CreateMemoryMap();
                }
                int bpp;
                if (memoryMap.TryGetValue(format, out bpp))
                {
                    return bpp;
                }
                return 0;
            }

            private static void CreateMemoryMap()
            {
                memoryMap = new Dictionary<RenderTextureFormat, int>();
                memoryMap[RenderTextureFormat.ARGB32] = 4;
                memoryMap[RenderTextureFormat.Depth] = 0;
                memoryMap[RenderTextureFormat.ARGBHalf] = 8;
                memoryMap[RenderTextureFormat.Shadowmap] = 0;
                memoryMap[RenderTextureFormat.RGB565] = 2;
                memoryMap[RenderTextureFormat.ARGB4444] = 2;
                memoryMap[RenderTextureFormat.ARGB1555] = 2;
                memoryMap[RenderTextureFormat.Default] = 0;
                memoryMap[RenderTextureFormat.ARGB2101010] = 4;
                memoryMap[RenderTextureFormat.DefaultHDR] = 0;
                memoryMap[RenderTextureFormat.ARGB64] = 8;
                memoryMap[RenderTextureFormat.ARGBFloat] = 16;
                memoryMap[RenderTextureFormat.RGFloat] = 8;
                memoryMap[RenderTextureFormat.RGHalf] = 4;
                memoryMap[RenderTextureFormat.RFloat] = 4;
                memoryMap[RenderTextureFormat.RHalf] = 2;
                memoryMap[RenderTextureFormat.R8] = 1;
                memoryMap[RenderTextureFormat.ARGBInt] = 16;
                memoryMap[RenderTextureFormat.RGInt] = 8;
                memoryMap[RenderTextureFormat.RInt] = 4;
                memoryMap[RenderTextureFormat.BGRA32] = 4;
                memoryMap[RenderTextureFormat.RGB111110Float] = 4;
                memoryMap[RenderTextureFormat.RG32] = 4;
                memoryMap[RenderTextureFormat.RGBAUShort] = 8;
                memoryMap[RenderTextureFormat.RG16] = 2;
                memoryMap[RenderTextureFormat.BGRA10101010_XR] = 10;
                memoryMap[RenderTextureFormat.BGR101010_XR] = 0;
                memoryMap[RenderTextureFormat.R16] = 2;
            }
        }

        public static readonly string TEMP_HEIGHT_NAME = "~TempHeight";
        public static readonly int TEMP_HEIGHT_RESOLUTION = 512;

        private List<GraphRenderTexture> m_renderTextures;

        private List<GraphBuffer> m_buffers;

        private Dictionary<string, int> m_refCount;

        public DataPool()
        {
            m_renderTextures = new List<GraphRenderTexture>();
            m_buffers = new List<GraphBuffer>();
            m_refCount = new Dictionary<string, int>();
        }

        ~DataPool()
        {
#if UNITY_EDITOR
            MemoryStats stats = GetMemoryStats();
            if (stats.textureCount > 0 || stats.bufferCount > 0)
            {
                Debug.LogWarning("A Data Pool is not disposed, this may causes a memory leak.");
            }
#endif
        }

        public GraphRenderTexture CreateRenderTarget(RtDescriptor desc, string name)
        {
            GraphRenderTexture result = m_renderTextures.Find(rt => desc.Equals(rt.renderTexture.descriptor) && GetReferenceCount(rt.identifier) <= 0);
            if (result == null)
            {
#if UNITY_EDITOR
                if (desc.width % 8 != 0 || desc.height % 8 != 0)
                {
                    Debug.LogWarning($"Attempt to create a new render target whose resolution is not a multiple of 8 ({desc.width}x{desc.height}), name: {name}");
                }
#endif
                result = new GraphRenderTexture(desc.width, desc.height, desc.format);
                result.renderTexture.wrapMode = TextureWrapMode.Clamp;
                result.renderTexture.filterMode = FilterMode.Bilinear;
                result.renderTexture.enableRandomWrite = true;
                result.renderTexture.antiAliasing = 1;
                result.renderTexture.Create();
                m_renderTextures.Add(result);
            }
            result.identifier = name;
            return result;
        }

        public GraphRenderTexture CreateRenderTarget(RtDescriptor desc, SlotRef slotRef)
        {
            return CreateRenderTarget(desc, GetName(slotRef.nodeId, slotRef.slotId));
        }

        public GraphBuffer CreateBuffer(BufferDescriptor desc, string name)
        {
            GraphBuffer result = m_buffers.Find(b => b.buffer.count == desc.count && GetReferenceCount(b.identifier) <= 0);
            if (result == null)
            {
                result = new GraphBuffer(desc.count, desc.stride);
                m_buffers.Add(result);
            }
            result.identifier = name;
            return result;
        }

        public GraphBuffer CreateBuffer(BufferDescriptor desc, SlotRef slotRef)
        {
            return CreateBuffer(desc, GetName(slotRef.nodeId, slotRef.slotId));
        }

        public GraphRenderTexture CreateTemporaryRT(RtDescriptor desc, string uniqueName)
        {
            GraphRenderTexture rt = CreateRenderTarget(desc, uniqueName);
            if (m_refCount != null)
            {
                m_refCount[uniqueName] = 1;
            }
            return rt;
        }

        public GraphBuffer CreateTemporaryBuffer(BufferDescriptor desc, string uniqueName)
        {
            GraphBuffer rt = CreateBuffer(desc, uniqueName);
            if (m_refCount != null)
            {
                m_refCount[uniqueName] = 1;
            }
            return rt;
        }

        public GraphRenderTexture GetRT(string name)
        {
            GraphRenderTexture rt = m_renderTextures.Find(t => t.identifier.Equals(name));
            return rt;
        }

        public GraphRenderTexture GetRT(SlotRef slotRef)
        {
            return GetRT(GetName(slotRef.nodeId, slotRef.slotId));
        }

        public GraphBuffer GetBuffer(string name)
        {
            GraphBuffer buffer = m_buffers.Find(b => b.identifier.Equals(name));
            return buffer;
        }

        public GraphBuffer GetBuffer(SlotRef slotRef)
        {
            return GetBuffer(GetName(slotRef.nodeId, slotRef.slotId));
        }

        /// <summary>
        /// Remove the RT from the pool, this RT will no longer managed by the pool, RT content is kept
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GraphRenderTexture RemoveRTFromPool(string name)
        {
            GraphRenderTexture rt = m_renderTextures.Find(t => t.identifier.Equals(name));
            if (rt != null)
            {
                m_renderTextures.Remove(rt);
                m_refCount.Remove(name);
            }
            return rt;
        }

        /// <summary>
        /// Remove the RT from the pool, this RT will no longer managed by the pool, RT content is kept
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GraphRenderTexture RemoveRTFromPool(SlotRef slotRef)
        {
            return RemoveRTFromPool(GetName(slotRef.nodeId, slotRef.slotId));
        }

        public GraphBuffer RemoveBufferFromPool(string name)
        {
            GraphBuffer b = m_buffers.Find(t => t.identifier.Equals(name));
            if (b != null)
            {
                m_buffers.Remove(b);
                m_refCount.Remove(name);
            }
            return b;
        }

        public GraphBuffer RemoveBufferFromPool(SlotRef slotRef)
        {
            return RemoveBufferFromPool(GetName(slotRef.nodeId, slotRef.slotId));
        }

        public void Dispose()
        {
            for (int i = 0; i < m_renderTextures.Count; ++i)
            {

                if (m_renderTextures[i] != null)
                {
                    m_renderTextures[i].Dispose();
                    Object.DestroyImmediate(m_renderTextures[i]);
                    m_renderTextures[i] = null;
                }
            }
            m_renderTextures.Clear();

            for (int i = 0; i < m_buffers.Count; ++i)
            {
                if (m_buffers[i] != null)
                {
                    m_buffers[i].Dispose();
                    m_buffers[i] = null;
                }
            }
            m_buffers.Clear();
        }

        public void DisposeUnused()
        {
            for (int i = 0; i < m_renderTextures.Count; ++i)
            {
                if (m_renderTextures[i] != null && GetReferenceCount(m_renderTextures[i].identifier) <= 0)
                {
                    m_renderTextures[i].Dispose();
                    Object.DestroyImmediate(m_renderTextures[i]);
                    m_renderTextures[i] = null;
                }
                else if (m_renderTextures[i] != null && m_renderTextures[i].identifier.Equals(TEMP_HEIGHT_NAME))
                {

                    m_renderTextures[i].Dispose();
                    Object.DestroyImmediate(m_renderTextures[i]);
                    m_renderTextures[i] = null;
                }
            }
            m_renderTextures.RemoveAll(rt => rt == null);

            for (int i = 0; i < m_buffers.Count; ++i)
            {
                if (m_buffers[i] != null && GetReferenceCount(m_buffers[i].identifier) <= 0)
                {
                    m_buffers[i].Dispose();
                    m_buffers[i] = null;
                }
            }
            m_buffers.RemoveAll(b => b == null);
        }

        public MemoryStats GetMemoryStats()
        {
            MemoryStats stats = new MemoryStats();

            foreach (GraphRenderTexture rt in m_renderTextures)
            {
                if (rt != null && rt.renderTexture.IsCreated())
                {
                    stats.textureCount += 1;
                    float bytePerPixel = MemoryStats.GetBytePerPixel(rt.renderTexture.format);
                    stats.megabyte += bytePerPixel * rt.renderTexture.width * rt.renderTexture.height / 1048576f;
                }
            }

            foreach (GraphBuffer b in m_buffers)
            {
                if (b != null && b.buffer != null && b.buffer.IsValid())
                {
                    stats.bufferCount += 1;
                    stats.megabyte += b.buffer.count * sizeof(float) / 1048576f;
                }
            }

            return stats;
        }

        internal void SetReferenceCount(Dictionary<string, int> refCount)
        {
            this.m_refCount = refCount;
        }

        internal void SetReferenceCount(string name, int value)
        {
            m_refCount[name] = value;
        }

        internal void SetReferenceCount(SlotRef slotRef, int value)
        {
            string name = GetName(slotRef.nodeId, slotRef.slotId);
            m_refCount[name] = value;
        }

        public void ReleaseReference(string name)
        {
            if (m_refCount.ContainsKey(name))
            {
                m_refCount[name] -= 1;
            }
        }

        public void ReleaseReference(SlotRef slotRef)
        {
            ReleaseReference(GetName(slotRef.nodeId, slotRef.slotId));
        }

        public int GetReferenceCount(string name)
        {
            if (m_refCount.ContainsKey(name))
            {
                return m_refCount[name];
            }
            else
            {
                return 0;
            }
        }

        public int GetUsageCount(SlotRef slotRef)
        {
            return GetReferenceCount(GetName(slotRef.nodeId, slotRef.slotId));
        }

        public static string GetName(string nodeId, int slotId)
        {
            string s = string.Format("{0}_{1}",
                       nodeId,
                       slotId);
            return s;
        }

        internal void BindTexture(SlotRef slotRef, GraphRenderTexture texture)
        {
            string name = GetName(slotRef.nodeId, slotRef.slotId);
            texture.identifier = name;
            m_renderTextures.Add(texture);
        }

        internal void BindBuffer(SlotRef slotRef, GraphBuffer buffer)
        {
            string name = GetName(slotRef.nodeId, slotRef.slotId);
            buffer.identifier = name;
            m_buffers.Add(buffer);
        }
    }
}
#endif
