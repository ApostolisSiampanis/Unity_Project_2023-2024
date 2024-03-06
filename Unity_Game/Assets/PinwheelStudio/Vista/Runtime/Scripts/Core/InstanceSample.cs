#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista
{
    [Serializable]
    public struct InstanceSample : IEquatable<InstanceSample>
    {
        public static readonly int SIZE = 7; //*sizeof(float)

        public float isValid;
        public Vector3 position;
        public float verticalScale;
        public float horizontalScale;
        public float rotationY;

        public bool Equals(InstanceSample other)
        {
            return this.isValid == other.isValid &&
                this.position == other.position &&
                this.verticalScale == other.verticalScale &&
                this.horizontalScale == other.horizontalScale &&
                this.rotationY == other.rotationY;
        }
    }
}
#endif
