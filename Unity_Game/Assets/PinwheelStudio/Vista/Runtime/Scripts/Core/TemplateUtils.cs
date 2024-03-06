#if VISTA

namespace Pinwheel.Vista
{
    public static class TemplateUtils
    {
        internal delegate bool EnableVariantsSupportHandler();
        internal static event EnableVariantsSupportHandler enableVariantsSupportCallback;

        public static bool IsVariantsSupported()
        {
            return enableVariantsSupportCallback != null && enableVariantsSupportCallback.Invoke() == true;
        }
    }
}
#endif
