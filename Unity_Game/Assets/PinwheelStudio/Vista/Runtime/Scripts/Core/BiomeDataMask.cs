#if VISTA

namespace Pinwheel.Vista
{
    [System.Flags]
    public enum BiomeDataMask : int
    {
        HeightMap = 1,
        HoleMap = 2,
        MeshDensityMap = 4,
        AlbedoMap = 8,
        MetallicMap = 16,
        LayerWeightMaps = 32,
        TreeInstances = 64,
        DetailDensityMaps = 128,
        DetailInstances = 256,
        ObjectInstances = 512,
        GenericTextures = 1024,
        GenericBuffers = 2048
    }
}
#endif
