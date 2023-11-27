#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Geometric
{
    public static class Geo2D
    {
        public static float Cross(Vector2 lhs, Vector2 rhs)
        {
            return lhs.y * rhs.x - lhs.x * rhs.y;
        }
    }
}
#endif
