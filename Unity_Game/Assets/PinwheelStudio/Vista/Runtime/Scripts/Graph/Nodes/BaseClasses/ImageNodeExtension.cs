#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public static class ImageNodeExtension
    {
        public static int CalculateResolution(this ImageNodeBase n, int graphResolution, int mainInputResolution)
        {
            int res;
            if (n.resolutionOverride == ResolutionOverrideOptions.RelativeToGraph)
            {
                res = Mathf.Max(8, Mathf.CeilToInt(graphResolution * n.resolutionMultiplier));
            }
            else if (n.resolutionOverride == ResolutionOverrideOptions.RelativeToMainInput)
            {
                res = Mathf.Max(8, Mathf.CeilToInt(mainInputResolution * n.resolutionMultiplier));
            }
            else
            {
                res = n.resolutionAbsolute;
            }
            res = Utilities.MultipleOf8(res);
            return res;
        }
    }
}
#endif
