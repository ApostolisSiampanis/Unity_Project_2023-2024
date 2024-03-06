#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;

namespace Pinwheel.Vista
{
    public static class ShaderUtilities
    {
        private static Dictionary<string, Shader> s_shaderCache = new Dictionary<string, Shader>();

        public static Shader Find(string shaderName)
        {
            Shader s;
            if (s_shaderCache.TryGetValue(shaderName, out s))
            {
                return s;
            }
            else
            {
                s = Shader.Find(shaderName);
                if (s != null)
                {
                    s_shaderCache.Add(shaderName, s);
                }
                return s;
            }
        }
    }
}
#endif
