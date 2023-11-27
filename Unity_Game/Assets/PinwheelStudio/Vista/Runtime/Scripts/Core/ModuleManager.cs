#if VISTA
using System.Collections.Generic;

namespace Pinwheel.Vista
{
    public static class ModuleManager
    {
        private static HashSet<IModuleInfo> s_registeredModules;

        public static void Register(IModuleInfo module)
        {
            if (s_registeredModules == null)
            {
                s_registeredModules = new HashSet<IModuleInfo>();
            }

            s_registeredModules.Add(module);
        }

    }
}
#endif
