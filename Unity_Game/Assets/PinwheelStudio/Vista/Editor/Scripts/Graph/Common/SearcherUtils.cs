#if VISTA

namespace Pinwheel.VistaEditor.Graph
{
    public static class SearcherUtils
    {
        internal delegate bool EnableSmartSearchHandler();
        internal static event EnableSmartSearchHandler enableSmartSearchCallback;

        public static bool IsSmartSearchSupported()
        {
            return enableSmartSearchCallback != null && enableSmartSearchCallback.Invoke() == true;
        }
    }
}
#endif
