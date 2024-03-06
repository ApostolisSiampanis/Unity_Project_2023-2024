#if VISTA
using System.Collections.Generic;

namespace Pinwheel.Vista
{
    public class VersionManager
    {
        public delegate void CollectVersionInfoHandler(Collector<string> versionStrings);
        public static event CollectVersionInfoHandler collectVersionInfoCallback;

        public static List<string> GetVersionStrings()
        {
            Collector<string> versionStrings = new Collector<string>();
            if (collectVersionInfoCallback != null)
            {
                collectVersionInfoCallback.Invoke(versionStrings);
            }

            return versionStrings.ToList();
        }
    }
}
#endif
