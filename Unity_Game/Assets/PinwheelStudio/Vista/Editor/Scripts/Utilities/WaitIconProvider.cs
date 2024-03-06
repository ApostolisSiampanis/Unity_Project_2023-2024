#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using UnityEditor;

namespace Pinwheel.VistaEditor
{
    public static class WaitIconProvider
    {
        private static Texture2D[] s_waitTextures;
        private static Texture2D[] waitTextures
        {
            get
            {
                if (s_waitTextures == null)
                {
                    s_waitTextures = new Texture2D[8];
                    for (int i = 0; i < 8; ++i)
                    {
                        s_waitTextures[i] = Resources.Load<Texture2D>($"Vista/Textures/Wait{i}");
                    }
                }
                return s_waitTextures;
            }
        }

        public static Texture2D GetTexture()
        {
            double t = (EditorApplication.timeSinceStartup % 1) * 8;
            int i = (int)t;
            return waitTextures[i];
        }
    }
}
#endif
