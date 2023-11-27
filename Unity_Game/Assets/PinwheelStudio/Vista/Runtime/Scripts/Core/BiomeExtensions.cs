#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public static class BiomeExtensions
    {
        public static void MarkChanged(this IBiome b)
        {
            b.updateCounter = System.DateTime.Now.Ticks;
        }

        public static void GenerateBiomesInGroup(this IBiome b)
        {
            VistaManager manager = GetVistaManagerInstance(b);
            if (manager != null)
            {
                manager.GenerateAll();
            }
        }

        public static VistaManager GetVistaManagerInstance(this IBiome b)
        {
            VistaManager manager = b.gameObject.GetComponentInParent<VistaManager>();
            if (manager == null)
            {
                BiomeVMConnector connector = b.gameObject.GetComponent<BiomeVMConnector>();
                if (connector != null)
                {
                    foreach (VistaManager vm in VistaManager.allInstances)
                    {
                        if (string.Equals(vm.id, connector.managerId))
                        {
                            manager = vm;
                            break;
                        }
                    }
                }
            }
            return manager;
        }
    }
}
#endif
