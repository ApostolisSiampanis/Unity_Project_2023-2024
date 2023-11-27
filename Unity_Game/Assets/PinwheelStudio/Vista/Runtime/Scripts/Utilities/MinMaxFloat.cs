#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Pinwheel.Vista
{
    [System.Serializable]
    public struct MinMaxFloat : IEquatable<MinMaxFloat>
    {
        public static readonly MinMaxFloat FULL_RANGE = new MinMaxFloat(float.MinValue, float.MaxValue);

        [SerializeField]
        public float min;
        [SerializeField]
        public float max;

        public MinMaxFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Equals(MinMaxFloat other)
        {
            return this.min == other.min && this.max == other.max;
        }

        public static bool operator ==(MinMaxFloat a, MinMaxFloat b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MinMaxFloat a, MinMaxFloat b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (min, max).GetHashCode();
        }
    }
}
#endif
