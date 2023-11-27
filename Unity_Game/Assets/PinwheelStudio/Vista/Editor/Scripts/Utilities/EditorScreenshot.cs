#if VISTA
using UnityEditorInternal;
using UnityEngine;

namespace Pinwheel.VistaEditor
{
    public static class EditorScreenshot
    {
        public static Texture2D Capture(Vector2 pos, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            Color[] colors = InternalEditorUtility.ReadScreenPixel(pos, width, height);
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }
    }
}
#endif
