#if VISTA
using Pinwheel.Vista;
using UnityEngine;

namespace PinwheelStudio.Vista.Samples.Scenes.Demo
{
    public class DemoController : MonoBehaviour
    {
        public VistaManager vistaManager;
        public LocalProceduralBiome biome;

        public void Regenerate()
        {
            if (vistaManager == null || biome == null)
                return;

            biome.seed = Random.Range(0, 10000);
            vistaManager.ForceGenerate();
        }
    }
}
#endif