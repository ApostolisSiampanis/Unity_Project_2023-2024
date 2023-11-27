#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista
{
    [Serializable]
    public struct PositionSample : IEquatable<PositionSample>
    {
        public static readonly int SIZE = 4; //*sizeof(float)

        public float isValid;
        public Vector3 position;

        public bool Equals(PositionSample other)
        {
            return this.isValid == other.isValid && this.position == other.position;
        }
    }
}
#endif
